using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class Wedge : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Wedge;

        public override bool canRespawnOn => false;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public CardinalDirection direction { get; set; }

        [JsonProperty("h")]
        public int height;


        public Wedge(int id, Vector3 position, CardinalDirection direction, int height = 50) : base(id)
        {
            this.position = position;
            this.direction = direction;
            this.height = height;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject wedge;

            if (Application.isPlaying)
            {
                wedge = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/wedge.fbx");
            }
            else
            {
#if UNITY_EDITOR
                wedge = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Wedge.fbx", typeof(GameObject)));
#else
            return null;
#endif
            }

            wedge.name = "Wedge";

            wedge.transform.position = position;
            wedge.transform.localScale = new Vector3(100, 100, height);
            wedge.transform.rotation = Quaternion.Euler(-90, (int)direction * 90, 0);

            if (Application.isPlaying)
            {
                wedge.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/baseobstacle.mat");
            }

            return wedge;
        }
    }
}