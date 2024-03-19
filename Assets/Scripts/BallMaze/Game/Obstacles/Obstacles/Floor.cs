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
    public class Floor : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.Floor;

        public override bool canRespawnOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }


        public Floor(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after benchmarking it,
            // it appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(1, 0.1f, 1);
            floor.transform.position = position;
            floor.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return floor;
        }
    }
}
