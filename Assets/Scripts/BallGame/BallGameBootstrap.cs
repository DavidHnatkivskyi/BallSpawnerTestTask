using UnityEngine;
using Zenject;

namespace BallGame
{
    internal sealed class BallGameBootstrap : MonoBehaviour
    {
        private BallGameController _controller;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<BallGameBootstrap>() != null)
            {
                return;
            }

            var bootstrap = new GameObject("Ball Game");
            DontDestroyOnLoad(bootstrap);
            bootstrap.AddComponent<BallGameBootstrap>();
        }

        private void Awake()
        {
            var config = Resources.Load<BallGameConfig>("BallGameConfig");
            var particleEffectsConfig = Resources.Load<ParticleEffectsConfig>("ParticleEffectsConfig");
            var obstaclePrefab = Resources.Load<GameObject>("Prefabs/ObstacleCapsule");
            var camera = Camera.main;
            var light = Object.FindFirstObjectByType<Light>();
            if (config == null || particleEffectsConfig == null || obstaclePrefab == null || camera == null || light == null)
            {
                Debug.LogError("Ball Game requires both configuration assets, the obstacle prefab, a Main Camera, and a Light.");
                enabled = false;
                return;
            }

            var container = new DiContainer();
            BallGameBindings.Install(container, config, particleEffectsConfig, new BallGameScene(config, camera, light, obstaclePrefab));
            _controller = container.Resolve<BallGameController>();
            _controller.Initialize();
        }

        private void Update()
        {
            _controller?.Tick(Time.deltaTime);
        }
    }
}
