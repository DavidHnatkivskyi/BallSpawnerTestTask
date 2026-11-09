using UnityEngine;

namespace BallGame
{
    [CreateAssetMenu(fileName = "BallGameConfig", menuName = "Ball Game/Gameplay Config")]
    public sealed class BallGameConfig : ScriptableObject
    {
        [Header("Player")]
        [SerializeField, Min(0.1f)] private float _startingPlayerDiameter = 3.6f;
        [SerializeField, Min(0.1f)] private float _criticalPlayerDiameter = 0.55f;
        [SerializeField, Min(0.1f)] private float _playerSpeed = 3f;

        [Header("Shot")]
        [SerializeField, Min(0.1f)] private float _minimumShotDiameter = 0.35f;
        [SerializeField, Min(0.1f)] private float _chargeDiameterPerSecond = 0.8f;
        [SerializeField, Min(0.1f)] private float _projectileRadiusMultiplier = 3f;
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 4.67f;
        [SerializeField, Min(0.1f)] private float _infectionRadiusMultiplier = 3.33f;
        [SerializeField, Min(0.1f)] private float _requiredInfectionRadius = 1.75f;
        [SerializeField, Min(0.01f)] private float _infectionStepDelay = 0.14f;
        [SerializeField] private bool _isChainInfectionEnabled;
        [SerializeField] private bool _isInfectionRadiusAtLeastPathWidth = true;
        [SerializeField, Range(0f, 1f)] private float _startingSizeReserveFraction = 0.2f;
        [SerializeField, Min(0)] private int _areaObstacleCount = 120;
        [SerializeField, Min(0f)] private float _obstacleGap = 0.08f;
        [SerializeField, Min(0f)] private float _playerSpawnClearRadius = 3f;
        [SerializeField, Min(0f)] private float _doorSpawnClearRadius = 3f;

        [Header("Visualization")]
        [SerializeField] private bool _isProjectileInfectionRadiusVisible = true;

        [Header("Level")]
        [SerializeField, Min(0.1f)] private float _doorOpenDistance = 5f;
        [SerializeField] private Vector2 _obstacleDiameterRange = new(0.65f, 1.25f);
        [SerializeField] private Vector2 _obstacleHeightRange = new(1.1f, 2.2f);

        public float StartingPlayerDiameter => _startingPlayerDiameter;
        public float CriticalPlayerDiameter => _criticalPlayerDiameter;
        public float PlayerSpeed => _playerSpeed;
        public float MinimumShotDiameter => _minimumShotDiameter;
        public float ChargeDiameterPerSecond => _chargeDiameterPerSecond;
        public float ProjectileRadiusMultiplier => _projectileRadiusMultiplier;
        public float ProjectileSpeed => _projectileSpeed;
        public float InfectionRadiusMultiplier => _infectionRadiusMultiplier;
        public float RequiredInfectionRadius => _requiredInfectionRadius;
        public float InfectionStepDelay => _infectionStepDelay;
        public bool IsChainInfectionEnabled => _isChainInfectionEnabled;
        public bool IsInfectionRadiusAtLeastPathWidth => _isInfectionRadiusAtLeastPathWidth;
        public float StartingSizeReserveFraction => _startingSizeReserveFraction;
        public int AreaObstacleCount => _areaObstacleCount;
        public float ObstacleGap => _obstacleGap;
        public float PlayerSpawnClearRadius => _playerSpawnClearRadius;
        public float DoorSpawnClearRadius => _doorSpawnClearRadius;
        public bool IsProjectileInfectionRadiusVisible => _isProjectileInfectionRadiusVisible;
        public float DoorOpenDistance => _doorOpenDistance;
        public Vector2 ObstacleDiameterRange => _obstacleDiameterRange;
        public Vector2 ObstacleHeightRange => _obstacleHeightRange;
    }
}
