using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BallGame
{
    internal sealed class BallGameController
    {
        private readonly BallGameConfig _config;
        private readonly GameWindowManager _windowManager;
        private readonly ParticleEffectsManager _particleEffectsManager;
        private readonly BallGameScene _scene;

        private Transform _projectile;
        private LineRenderer _infectionRadiusLine;
        private float _playerDiameter;
        private float _shotDiameter;
        private bool _isCharging;
        private bool _isProjectileLaunched;
        private bool _isPlayerMoving;
        private bool _wasPressing;
        private GameState _state;

        public BallGameController(
            BallGameConfig config,
            GameWindowManager windowManager,
            ParticleEffectsManager particleEffectsManager,
            BallGameScene scene)
        {
            _config = config;
            _windowManager = windowManager;
            _particleEffectsManager = particleEffectsManager;
            _scene = scene;
        }

        public void Initialize()
        {
            _windowManager.Initialize(Restart);
            InitializeLevel();
        }

        public void Tick(float deltaTime)
        {
            _particleEffectsManager.Tick();
            if (_state != GameState.Playing)
            {
                return;
            }

            HandleShooting(deltaTime);
            MoveProjectile(deltaTime);
            UpdateInfections(deltaTime);
            MovePlayer();
            UpdateDoor();
            _scene.UpdateAimLine(_playerDiameter);
        }

        private float CalculateStartingPlayerDiameter()
        {
            var requiredShotDiameter = Mathf.Max(
                _config.MinimumShotDiameter,
                _config.RequiredInfectionRadius * 2f / _config.InfectionRadiusMultiplier);
            var playerDiameterPerShot = requiredShotDiameter / _config.ProjectileRadiusMultiplier;
            var requiredShotCount = CountRequiredShots(_config.RequiredInfectionRadius);
            var requiredDiameter = _config.CriticalPlayerDiameter + requiredShotCount * playerDiameterPerShot;
            var calculatedDiameter = requiredDiameter * (1f + _config.StartingSizeReserveFraction);
            return Mathf.Max(_config.StartingPlayerDiameter, calculatedDiameter);
        }

        private int CountRequiredShots(float infectionRadius)
        {
            var remainingObstacles = new List<Obstacle>();
            foreach (var obstacle in _scene.Obstacles)
            {
                if (!obstacle.IsDestroyed && IsInMinimumPassage(obstacle))
                {
                    remainingObstacles.Add(obstacle);
                }
            }

            var shotCount = 0;
            while (remainingObstacles.Count > 0)
            {
                var explosionCenter = GetNearestObstacleToPlayer(remainingObstacles);
                shotCount++;
                for (var index = remainingObstacles.Count - 1; index >= 0; index--)
                {
                    var obstacle = remainingObstacles[index];
                    if (Vector3.Distance(explosionCenter.transform.position, obstacle.transform.position)
                        <= infectionRadius + obstacle.Radius)
                    {
                        remainingObstacles.RemoveAt(index);
                    }
                }
            }

            return shotCount;
        }

        private Obstacle GetNearestObstacleToPlayer(List<Obstacle> obstacles)
        {
            var nearestObstacle = obstacles[0];
            var nearestDistance = Vector3.Distance(_scene.PlayerStart, nearestObstacle.transform.position);
            for (var index = 1; index < obstacles.Count; index++)
            {
                var candidate = obstacles[index];
                var distance = Vector3.Distance(_scene.PlayerStart, candidate.transform.position);
                if (distance < nearestDistance)
                {
                    nearestObstacle = candidate;
                    nearestDistance = distance;
                }
            }

            return nearestObstacle;
        }

        private bool IsInMinimumPassage(Obstacle obstacle)
        {
            return DistanceToPath(obstacle.transform.position) <= _config.CriticalPlayerDiameter * 0.5f + obstacle.Radius;
        }

        private void HandleShooting(float deltaTime)
        {
            if (_isPlayerMoving)
            {
                _wasPressing = false;
                return;
            }

            var isPressing = BallGameInput.IsShotPressed();
            if (isPressing && !_wasPressing)
            {
                BeginCharging();
            }

            if (isPressing && _isCharging)
            {
                ChargeShot(deltaTime);
            }

            if (!isPressing && _wasPressing && _isCharging)
            {
                LaunchShot();
            }

            _wasPressing = isPressing;
        }

        private void BeginCharging()
        {
            if (_projectile != null)
            {
                return;
            }

            _shotDiameter = _config.MinimumShotDiameter;
            SpendPlayerDiameter(_shotDiameter / _config.ProjectileRadiusMultiplier);
            if (_state != GameState.Playing)
            {
                return;
            }

            _projectile = _scene.CreateProjectile();
            _isCharging = true;
            _isProjectileLaunched = false;
            UpdateProjectileScale();
            _infectionRadiusLine = _scene.CreateInfectionRadiusPreview(_projectile);
            UpdateInfectionRadiusPreview();
            PlaceChargingProjectile();
        }

        private void ChargeShot(float deltaTime)
        {
            var spentDiameter = _config.ChargeDiameterPerSecond * deltaTime;
            _shotDiameter += spentDiameter * _config.ProjectileRadiusMultiplier;
            SpendPlayerDiameter(spentDiameter);
            if (_state == GameState.Playing)
            {
                UpdateProjectileScale();
                UpdateInfectionRadiusPreview();
                PlaceChargingProjectile();
            }
        }

        private void LaunchShot()
        {
            _isCharging = false;
            _isProjectileLaunched = true;
        }

        private void MoveProjectile(float deltaTime)
        {
            if (_projectile == null || !_isProjectileLaunched)
            {
                return;
            }

            var direction = (_scene.DoorPosition - _projectile.position).normalized;
            var travelDistance = _config.ProjectileSpeed * deltaTime;
            var activationRadius = GetInfectionRadius();
            if (TryGetNearestPathObstacle(direction, travelDistance, activationRadius, out var triggerDistance))
            {
                _projectile.position += direction * triggerDistance;
                StartInfection(_projectile.position);
                return;
            }

            _projectile.position += direction * travelDistance;
            if (Vector3.Distance(_projectile.position, _scene.DoorPosition) < 0.25f)
            {
                DestroyProjectile();
            }
        }

        private bool TryGetNearestPathObstacle(
            Vector3 direction,
            float travelDistance,
            float activationRadius,
            out float triggerDistance)
        {
            triggerDistance = float.MaxValue;
            foreach (var obstacle in _scene.Obstacles)
            {
                if (obstacle.IsDestroyed || !IsOnPlayerPath(obstacle))
                {
                    continue;
                }

                var offset = obstacle.transform.position - _projectile.position;
                var forwardDistance = Vector3.Dot(offset, direction);
                var sidewaysDistance = Vector3.ProjectOnPlane(offset, direction).magnitude;
                if (forwardDistance < 0f
                    || sidewaysDistance > activationRadius + obstacle.Radius
                    || forwardDistance > travelDistance
                    || forwardDistance >= triggerDistance)
                {
                    continue;
                }

                triggerDistance = forwardDistance;
            }

            return triggerDistance < float.MaxValue;
        }

        private void StartInfection(Vector3 center)
        {
            InfectInRadius(center, GetInfectionRadius());
            DestroyProjectile();
        }

        private void UpdateInfections(float deltaTime)
        {
            foreach (var obstacle in _scene.Obstacles)
            {
                if (!obstacle.TryExplode(deltaTime))
                {
                    continue;
                }

                if (_config.IsChainInfectionEnabled)
                {
                    InfectInRadius(obstacle.transform.position, obstacle.InfectionRadius);
                }

                _particleEffectsManager.PlayObstacleDestroyed(obstacle.transform.position);
                obstacle.DestroyObstacle();
            }
        }

        private void InfectInRadius(Vector3 center, float radius)
        {
            foreach (var target in _scene.Obstacles)
            {
                if (!target.IsDestroyed && Vector3.Distance(center, target.transform.position) <= radius + target.Radius)
                {
                    target.Infect(_config.InfectionStepDelay, radius);
                }
            }
        }

        private void MovePlayer()
        {
            if (HasPathObstacles() || _isPlayerMoving)
            {
                return;
            }

            _isPlayerMoving = true;
            var duration = Vector3.Distance(_scene.Player.position, _scene.DoorPosition) / _config.PlayerSpeed;
            _scene.Player.DOMove(_scene.DoorPosition, duration)
                .SetEase(Ease.Linear)
                .OnComplete(CompleteLevel);
        }

        private void CompleteLevel()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            _state = GameState.Won;
            _scene.SetPlayerColor(new Color(0.35f, 1f, 0.45f));
            _windowManager.ShowWin();
        }

        private void UpdateDoor()
        {
            if (!_scene.Door.IsOpen && Vector3.Distance(_scene.Player.position, _scene.DoorPosition) <= _config.DoorOpenDistance)
            {
                _scene.Door.Open();
                _particleEffectsManager.PlayDoorOpened(_scene.Door.transform.position);
            }
        }

        private void SpendPlayerDiameter(float diameter)
        {
            _playerDiameter -= diameter;
            _scene.SetPlayerDiameter(_playerDiameter);
            if (_playerDiameter < _config.CriticalPlayerDiameter)
            {
                _state = GameState.Lost;
                _isCharging = false;
                _isProjectileLaunched = false;
                _isPlayerMoving = false;
                _scene.Player.DOKill();
                _scene.SetPlayerColor(new Color(1f, 0.14f, 0.18f));
                DestroyProjectile();
                _windowManager.ShowLose();
            }
        }

        private void Restart()
        {
            _scene.Dispose();
            _projectile = null;
            _infectionRadiusLine = null;
            _isCharging = false;
            _isProjectileLaunched = false;
            _isPlayerMoving = false;
            _wasPressing = false;
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            _scene.CreateLevel();
            _playerDiameter = CalculateStartingPlayerDiameter();
            _scene.CreatePlayer(_playerDiameter);
            _scene.CreateAimLine();
            _scene.UpdateAimLine(_playerDiameter);
            _state = GameState.Playing;
        }

        private void UpdateProjectileScale()
        {
            _projectile.localScale = Vector3.one * _shotDiameter;
        }

        private void PlaceChargingProjectile()
        {
            var direction = (_scene.DoorPosition - _scene.Player.position).normalized;
            _projectile.position = _scene.Player.position + direction * ((_playerDiameter + _shotDiameter) * 0.5f + 0.08f);
        }

        private void DestroyProjectile()
        {
            if (_projectile == null)
            {
                return;
            }

            Object.Destroy(_projectile.gameObject);
            _projectile = null;
            _infectionRadiusLine = null;
        }

        private void UpdateInfectionRadiusPreview()
        {
            if (_infectionRadiusLine == null)
            {
                return;
            }

            var radius = GetInfectionRadius();
            for (var index = 0; index < _infectionRadiusLine.positionCount; index++)
            {
                var angle = index * Mathf.PI * 2f / _infectionRadiusLine.positionCount;
                _infectionRadiusLine.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
            }
        }

        private float GetInfectionRadius()
        {
            var infectionRadius = _shotDiameter * 0.5f * _config.InfectionRadiusMultiplier;
            return _config.IsInfectionRadiusAtLeastPathWidth
                ? Mathf.Max(infectionRadius, _playerDiameter * 0.5f)
                : infectionRadius;
        }

        private bool HasPathObstacles()
        {
            foreach (var obstacle in _scene.Obstacles)
            {
                if (!obstacle.IsDestroyed && IsOnPlayerPath(obstacle))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOnPlayerPath(Obstacle obstacle)
        {
            return DistanceToPath(obstacle.transform.position) <= _playerDiameter * 0.5f + obstacle.Radius;
        }

        private float DistanceToPath(Vector3 position)
        {
            var path = _scene.DoorPosition - _scene.PlayerStart;
            var progress = Mathf.Clamp01(Vector3.Dot(position - _scene.PlayerStart, path) / path.sqrMagnitude);
            var nearestPoint = _scene.PlayerStart + path * progress;
            return Vector3.Distance(position, nearestPoint);
        }

        private enum GameState
        {
            Playing,
            Won,
            Lost
        }
    }
}
