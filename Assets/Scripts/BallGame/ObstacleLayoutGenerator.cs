using System;
using System.Collections.Generic;
using UnityEngine;

namespace BallGame
{
    internal sealed class ObstacleLayoutGenerator
    {
        private readonly BallGameConfig _config;
        private readonly Vector3 _playerStart;
        private readonly Vector3 _doorPosition;

        public ObstacleLayoutGenerator(BallGameConfig config, Vector3 playerStart, Vector3 doorPosition)
        {
            _config = config;
            _playerStart = playerStart;
            _doorPosition = doorPosition;
        }

        public List<ObstacleSpawn> Generate(System.Random random)
        {
            var halfWidth = _doorPosition.x / 0.68f;
            var halfHeight = _doorPosition.y / 0.6f;
            var spawns = new List<ObstacleSpawn>();
            var maximumAttempts = _config.AreaObstacleCount * 100;
            for (var attempt = 0; attempt < maximumAttempts && spawns.Count < _config.AreaObstacleCount; attempt++)
            {
                var diameter = RandomRange(_config.ObstacleDiameterRange, random);
                var position = new Vector3(RandomRange(-halfWidth * 0.82f, halfWidth * 0.82f, random), RandomRange(-halfHeight * 0.78f, halfHeight * 0.78f, random));
                if (IsValidPosition(position, diameter * 0.5f, spawns))
                {
                    spawns.Add(new ObstacleSpawn(position, diameter, RandomRange(_config.ObstacleHeightRange, random)));
                }
            }

            return spawns;
        }

        private bool IsValidPosition(Vector3 position, float radius, List<ObstacleSpawn> spawns)
        {
            if (Vector3.Distance(position, _playerStart) <= _config.PlayerSpawnClearRadius + radius || Vector3.Distance(position, _doorPosition) <= _config.DoorSpawnClearRadius + radius)
            {
                return false;
            }

            foreach (var spawn in spawns)
            {
                if (Vector3.Distance(position, spawn.Position) < radius + spawn.Diameter * 0.5f + _config.ObstacleGap)
                {
                    return false;
                }
            }

            return true;
        }

        private static float RandomRange(Vector2 range, System.Random random) => Mathf.Lerp(range.x, range.y, (float)random.NextDouble());
        private static float RandomRange(float minimum, float maximum, System.Random random) => Mathf.Lerp(minimum, maximum, (float)random.NextDouble());
    }
}
