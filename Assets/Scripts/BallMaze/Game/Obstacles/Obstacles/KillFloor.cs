using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple floor tile
    /// </summary>
    [Serializable]
    public class KillFloor : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.KillFloor;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => true;

        public Vector3 position { get; set; }
        public Vector3 offset { get; set; }

        public KillFloor(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after benchmarking it,
            // it appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
            GameObject killFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            killFloor.name = "KillFloor";
            killFloor.transform.localScale = new Vector3(1, 0.1f, 1);
            killFloor.transform.position = position + offset;

            if (Application.isPlaying)
            {
                killFloor.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/kill.mat");
            }

            return killFloor;
        }
    }
}
