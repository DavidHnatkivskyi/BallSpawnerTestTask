using System.Collections.Generic;
using UnityEngine;

namespace BallGame
{
    internal sealed class BallGameScene
    {
        private const float LayoutHalfHeight = 7.5f;
        private const string SimpleLitMaterialPath = "Materials/MobileSimpleLit";
        private const string UnlitMaterialPath = "Materials/MobileUnlit";
        private readonly BallGameConfig _config;
        private readonly Camera _camera;
        private readonly Light _light;
        private readonly GameObject _obstaclePrefab;
        private readonly List<Obstacle> _obstacles = new();
        private readonly Stack<Obstacle> _obstaclePool = new();
        private readonly List<Material> _materials = new();
        private Transform _root;
        private Transform _obstaclePoolRoot;
        private Transform _player;
        private Door _door;
        private LineRenderer _aimLine;
        private Vector3 _playerStart;
        private Vector3 _doorPosition;

        public BallGameScene(BallGameConfig config, Camera camera, Light light, GameObject obstaclePrefab)
        {
            _config = config;
            _camera = camera ?? throw new System.ArgumentNullException(nameof(camera));
            _light = light ?? throw new System.ArgumentNullException(nameof(light));
            _obstaclePrefab = obstaclePrefab ?? throw new System.ArgumentNullException(nameof(obstaclePrefab));
        }

        public IReadOnlyList<Obstacle> Obstacles => _obstacles;
        public Transform Player => _player;
        public Door Door => _door;
        public Vector3 PlayerStart => _playerStart;
        public Vector3 DoorPosition => _doorPosition;

        public void CreateLevel()
        {
            ConfigurePresentation();
            _root = new GameObject("Generated Ball Level").transform;
            _obstaclePoolRoot ??= new GameObject("Obstacle Pool").transform;
            CreateBackdrop();
            CreateDoor();
            CreateObstacles();
        }

        public void CreatePlayer(float diameter)
        {
            var playerObject = CreatePrimitive(PrimitiveType.Sphere, "Player Ball", _playerStart, new Color(0.22f, 0.75f, 1f));
            RemoveCollider(playerObject);
            _player = playerObject.transform;
            _player.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            SetPlayerDiameter(diameter);
        }

        public Transform CreateProjectile()
        {
            var projectile = CreatePrimitive(PrimitiveType.Sphere, "Charged Shot", Vector3.zero, new Color(0.65f, 0.95f, 1f));
            RemoveCollider(projectile);
            return projectile.transform;
        }

        public void CreateAimLine()
        {
            _aimLine = CreateLine("Player Size Aim Line", new Color(0.25f, 0.85f, 1f, 0.25f), false);
            _aimLine.positionCount = 2;
            _aimLine.numCapVertices = 12;
            _aimLine.sortingOrder = -1;
        }

        public LineRenderer CreateInfectionRadiusPreview(Transform projectile)
        {
            if (!_config.IsProjectileInfectionRadiusVisible) return null;
            var line = CreateLine("Infection Radius Preview", new Color(1f, 0.78f, 0.18f, 0.9f), true);
            line.transform.SetParent(projectile, false);
            line.transform.localPosition = new Vector3(0f, 0f, 0.2f);
            line.loop = true;
            line.positionCount = 48;
            line.startWidth = line.endWidth = 0.045f;
            return line;
        }

        public void UpdateAimLine(float diameter)
        {
            _aimLine.SetPosition(0, _player.position + new Vector3(0f, 0f, 0.2f));
            _aimLine.SetPosition(1, _doorPosition + new Vector3(0f, 0f, 0.2f));
            _aimLine.startWidth = _aimLine.endWidth = diameter;
        }

        public void SetPlayerDiameter(float diameter) => _player.localScale = Vector3.one * diameter;
        public void SetPlayerColor(Color color) => _player.GetComponent<Renderer>().sharedMaterial.color = color;

        public void Dispose()
        {
            ReturnAllObstaclesToPool();
            if (_root != null) Object.Destroy(_root.gameObject);
            foreach (var material in _materials) Object.Destroy(material);
            _materials.Clear();
            _obstacles.Clear();
            _root = null;
            _player = null;
            _door = null;
            _aimLine = null;
        }

        private void ConfigurePresentation()
        {
            _camera.transform.position = new Vector3(0f, -7f, -19f);
            _camera.transform.LookAt(Vector3.zero);
            _camera.orthographic = false;
            _camera.fieldOfView = 45f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.035f, 0.06f, 0.12f);
            _light.type = LightType.Directional;
            _light.transform.rotation = Quaternion.Euler(35f, -25f, 0f);
            _light.intensity = 1.4f;
            _light.shadows = LightShadows.Soft;
            var halfWidth = LayoutHalfHeight * _camera.aspect;
            _playerStart = new Vector3(-halfWidth * 0.68f, -LayoutHalfHeight * 0.6f);
            _doorPosition = new Vector3(halfWidth * 0.68f, LayoutHalfHeight * 0.6f);
        }

        private void CreateBackdrop()
        {
            var backdrop = CreatePrimitive(PrimitiveType.Quad, "Backdrop", new Vector3(0f, 0f, 1f), new Color(0.06f, 0.12f, 0.2f));
            RemoveCollider(backdrop);
            backdrop.transform.localScale = new Vector3(30f, 15f, 1f);
        }

        private void CreateDoor()
        {
            var doorObject = new GameObject("Exit Door");
            doorObject.transform.SetParent(_root);
            doorObject.transform.position = _doorPosition;
            _door = doorObject.AddComponent<Door>();
            _door.Initialize(CreateDoorLeaf(doorObject.transform, -0.7f), CreateDoorLeaf(doorObject.transform, 0.7f));
        }

        private Transform CreateDoorLeaf(Transform parent, float offset)
        {
            var leaf = CreatePrimitive(PrimitiveType.Cube, "Door Leaf", Vector3.zero, new Color(1f, 0.68f, 0.14f));
            RemoveCollider(leaf);
            leaf.transform.SetParent(parent);
            leaf.transform.localPosition = new Vector3(offset, 0f, 0f);
            leaf.transform.localScale = new Vector3(1.2f, 3.3f, 0.4f);
            return leaf.transform;
        }

        private void CreateObstacles()
        {
            var generator = new ObstacleLayoutGenerator(_config, _playerStart, _doorPosition);
            foreach (var spawn in generator.Generate(new System.Random()))
            {
                var obstacle = GetObstacle();
                obstacle.transform.SetPositionAndRotation(spawn.Position, Quaternion.Euler(90f, 0f, 0f));
                obstacle.transform.localScale = new Vector3(spawn.Diameter, spawn.Diameter, spawn.Height * 0.5f);
            }
        }

        private Obstacle GetObstacle()
        {
            if (_obstaclePool.Count > 0)
            {
                var pooledObstacle = _obstaclePool.Pop();
                pooledObstacle.transform.SetParent(_root);
                pooledObstacle.gameObject.SetActive(true);
                pooledObstacle.Spawn(pooledObstacle.GetComponent<Renderer>().sharedMaterial, ReturnObstacleToPool);
                _obstacles.Add(pooledObstacle);
                return pooledObstacle;
            }

            var obstacleObject = Object.Instantiate(_obstaclePrefab, _root);
            obstacleObject.name = "Obstacle";
            var material = CreateMaterial(new Color(0.95f, 0.23f, 0.35f));
            var obstacle = obstacleObject.AddComponent<Obstacle>();
            obstacle.Spawn(material, ReturnObstacleToPool);
            _obstacles.Add(obstacle);
            return obstacle;
        }

        private void ReturnAllObstaclesToPool()
        {
            for (var index = _obstacles.Count - 1; index >= 0; index--)
            {
                ReturnObstacleToPool(_obstacles[index]);
            }
        }

        private void ReturnObstacleToPool(Obstacle obstacle)
        {
            if (obstacle == null)
            {
                return;
            }

            _obstacles.Remove(obstacle);
            obstacle.ResetForPool();
            obstacle.transform.SetParent(_obstaclePoolRoot);
            obstacle.gameObject.SetActive(false);
            _obstaclePool.Push(obstacle);
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Color color)
        {
            var gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(_root);
            gameObject.transform.position = position;
            ApplyColor(gameObject, color);
            return gameObject;
        }

        private void ApplyColor(GameObject gameObject, Color color)
        {
            var material = CreateMaterial(color);
            _materials.Add(material);
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material CreateMaterial(Color color)
        {
            var material = CreateMaterialInstance(SimpleLitMaterialPath);
            material.color = color;
            material.SetFloat("_Smoothness", 0.35f);
            return material;
        }

        private static Material CreateMaterialInstance(string resourcePath)
        {
            var template = Resources.Load<Material>(resourcePath);
            if (template == null)
            {
                throw new System.InvalidOperationException($"Missing material template at Resources/{resourcePath}.");
            }

            return Object.Instantiate(template);
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Object.Destroy(gameObject.GetComponent<Collider>());
        }

        private LineRenderer CreateLine(string name, Color color, bool localSpace)
        {
            var lineObject = new GameObject(name);
            lineObject.transform.SetParent(_root);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = !localSpace;
            var material = CreateMaterialInstance(UnlitMaterialPath);
            material.color = color;
            material.SetFloat("_Surface", 1f);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
            _materials.Add(material);
            line.material = material;
            line.startColor = line.endColor = color;
            return line;
        }
    }
}
