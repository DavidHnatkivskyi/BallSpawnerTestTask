using UnityEngine;

namespace BallGame
{
    [CreateAssetMenu(fileName = "ParticleEffectsConfig", menuName = "Ball Game/Particle Effects Config")]
    public sealed class ParticleEffectsConfig : ScriptableObject
    {
        [SerializeField] private GameObject _obstacleDestroyEffect;
        [SerializeField] private GameObject _doorOpenEffect;

        public GameObject ObstacleDestroyEffect => _obstacleDestroyEffect;
        public GameObject DoorOpenEffect => _doorOpenEffect;
    }
}
