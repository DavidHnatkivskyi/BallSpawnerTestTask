using UnityEngine;

namespace BallGame
{
    internal readonly struct ObstacleSpawn
    {
        public ObstacleSpawn(Vector3 position, float diameter, float height)
        {
            Position = position;
            Diameter = diameter;
            Height = height;
        }

        public Vector3 Position { get; }
        public float Diameter { get; }
        public float Height { get; }
    }
}
