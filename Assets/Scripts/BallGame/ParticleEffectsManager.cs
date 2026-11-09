using System.Collections.Generic;
using UnityEngine;

namespace BallGame
{
    internal sealed class ParticleEffectsManager
    {
        private readonly ParticleEffectsConfig _config;
        private readonly Dictionary<GameObject, Queue<PooledEffect>> _availableEffects = new();
        private readonly List<PooledEffect> _activeEffects = new();

        private Transform _poolRoot;

        public ParticleEffectsManager(ParticleEffectsConfig config)
        {
            _config = config;
        }

        public void PlayObstacleDestroyed(Vector3 position)
        {
            Play(_config.ObstacleDestroyEffect, position);
        }

        public void PlayDoorOpened(Vector3 position)
        {
            Play(_config.DoorOpenEffect, position);
        }

        public void Tick()
        {
            for (var index = _activeEffects.Count - 1; index >= 0; index--)
            {
                var effect = _activeEffects[index];
                if (effect.IsAlive)
                {
                    continue;
                }

                effect.GameObject.SetActive(false);
                GetPool(effect.Prefab).Enqueue(effect);
                _activeEffects.RemoveAt(index);
            }
        }

        private void Play(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
            {
                return;
            }

            var effect = GetEffect(prefab);
            effect.GameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            effect.GameObject.SetActive(true);
            effect.Play();
            _activeEffects.Add(effect);
        }

        private PooledEffect GetEffect(GameObject prefab)
        {
            var pool = GetPool(prefab);
            return pool.Count > 0 ? pool.Dequeue() : CreateEffect(prefab);
        }

        private Queue<PooledEffect> GetPool(GameObject prefab)
        {
            if (_availableEffects.TryGetValue(prefab, out var pool))
            {
                return pool;
            }

            pool = new Queue<PooledEffect>();
            _availableEffects.Add(prefab, pool);
            return pool;
        }

        private PooledEffect CreateEffect(GameObject prefab)
        {
            if (_poolRoot == null)
            {
                var root = new GameObject("Particle Effects Pool");
                Object.DontDestroyOnLoad(root);
                _poolRoot = root.transform;
            }

            var instance = Object.Instantiate(prefab, _poolRoot);
            instance.name = $"{prefab.name} (Pooled)";
            instance.SetActive(false);
            return new PooledEffect(prefab, instance);
        }

        private sealed class PooledEffect
        {
            private readonly ParticleSystem[] _particleSystems;

            public PooledEffect(GameObject prefab, GameObject gameObject)
            {
                Prefab = prefab;
                GameObject = gameObject;
                _particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            }

            public GameObject Prefab { get; }
            public GameObject GameObject { get; }

            public bool IsAlive
            {
                get
                {
                    foreach (var particleSystem in _particleSystems)
                    {
                        if (particleSystem.IsAlive(true))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            public void Play()
            {
                foreach (var particleSystem in _particleSystems)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    particleSystem.Play(true);
                }
            }
        }
    }
}
