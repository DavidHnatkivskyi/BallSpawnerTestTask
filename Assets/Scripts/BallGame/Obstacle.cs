using DG.Tweening;
using System;
using UnityEngine;

namespace BallGame
{
    internal sealed class Obstacle : MonoBehaviour
    {
        private Renderer _renderer;
        private Collider _collider;
        private Material _spawnMaterial;
        private Material _runtimeMaterial;
        private Action<Obstacle> _returnToPool;
        private bool _isDestroyed;
        private bool _isInfected;
        private float _infectionCountdown;
        private float _infectionRadius;

        public float Radius => Mathf.Max(transform.lossyScale.x, transform.lossyScale.y) * 0.5f;
        public float InfectionRadius => _infectionRadius;
        public bool IsDestroyed => _isDestroyed;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
        }

        public void Spawn(Material material, Action<Obstacle> returnToPool)
        {
            _spawnMaterial = material;
            _returnToPool = returnToPool;
            _renderer.sharedMaterial = _spawnMaterial;
            _collider.enabled = true;
            _isDestroyed = false;
            _isInfected = false;
            _infectionCountdown = 0f;
            _infectionRadius = 0f;
        }

        public void Infect(float delay, float radius)
        {
            if (_isDestroyed || _isInfected)
            {
                return;
            }

            _isInfected = true;
            _infectionCountdown = delay;
            _infectionRadius = radius;
            _runtimeMaterial = _renderer.material;
            _runtimeMaterial.DOColor(new Color(1f, 0.82f, 0.16f), delay).SetEase(Ease.OutQuad);
            transform.DOPunchScale(Vector3.one * 0.18f, delay, 3, 0.6f);
        }

        public bool TryExplode(float deltaTime)
        {
            if (!_isInfected || _isDestroyed)
            {
                return false;
            }

            _infectionCountdown -= deltaTime;
            return _infectionCountdown <= 0f;
        }

        public void DestroyObstacle()
        {
            if (_isDestroyed)
            {
                return;
            }

            _isDestroyed = true;
            _collider.enabled = false;
            transform.DOKill();
            transform.DOScale(transform.localScale * 1.3f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => transform.DOScale(Vector3.zero, 0.12f).OnComplete(ReturnToPool));
        }

        public void ResetForPool()
        {
            transform.DOKill();
            _collider.enabled = false;
            _isDestroyed = false;
            _isInfected = false;
            _infectionCountdown = 0f;
            _infectionRadius = 0f;
            _returnToPool = null;
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.DOKill();
                Destroy(_runtimeMaterial);
                _runtimeMaterial = null;
            }

            _renderer.sharedMaterial = _spawnMaterial;
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.DOKill();
                Destroy(_runtimeMaterial);
            }

            if (_spawnMaterial != null)
            {
                Destroy(_spawnMaterial);
            }
        }

        private void ReturnToPool()
        {
            _returnToPool?.Invoke(this);
        }
    }
}
