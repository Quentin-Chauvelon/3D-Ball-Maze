using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class HalfSphere : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.HalfSphere;

        public override bool canRespawnOn => true;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }
        public Vector3 offset { get; set; }


        public HalfSphere(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject halfSphere;

            if (Application.isPlaying)
            {
                halfSphere = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/halfsphere.fbx");
            }
            else
            {
#if UNITY_EDITOR
                halfSphere = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/HalfSphere.fbx", typeof(GameObject)));
#else
                Debug.LogError("Cannot instantiate HalfSphere");
                return null;
#endif
            }

            halfSphere.name = "HalfSphere";
            halfSphere.transform.position = position + offset;

            if (Application.isPlaying)
            {
                halfSphere.transform.Find("HalfSphere_Floor").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
                halfSphere.transform.Find("HalfSphere_Sphere").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
            }

            return halfSphere;
        }
    }
}