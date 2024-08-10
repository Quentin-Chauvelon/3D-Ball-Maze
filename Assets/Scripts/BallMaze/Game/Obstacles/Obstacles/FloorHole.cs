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
        public Vector3 offset { get; set; }


        public FloorHole(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject floorHole;

            if (Application.isPlaying)
            {
                floorHole = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/floorhole.fbx");
            }
            else
            {
#if UNITY_EDITOR
                floorHole = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/FloorHole.fbx", typeof(GameObject)));
#else
                            Debug.LogError("Cannot instantiate HalfSphere");
                            return null;
#endif
            }

            floorHole.name = "FloorHole";
            floorHole.transform.position = position + offset;

            if (Application.isPlaying)
            {
                floorHole.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
            }

            return floorHole;
        }
    }
}