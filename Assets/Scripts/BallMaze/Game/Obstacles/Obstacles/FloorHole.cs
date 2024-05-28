using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class FloorHole : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.FloorHole;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => false;

        public Vector3 position { get; set; }


        public FloorHole(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject floorhole = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/FloorHole.fbx", typeof(GameObject)));
            floorhole.name = "FloorHole";

            floorhole.transform.position = position;

            floorhole.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return floorhole;
        }
    }
}