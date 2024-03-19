using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class Rail : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Rail;

        public override bool canRespawnOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public CardinalDirection direction { get; set; }


        public Rail(int id, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject rail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rail.prefab", typeof(GameObject)));
            rail.name = "Rail";

            rail.transform.position = position;

            switch (direction)
            {
                case CardinalDirection.North:
                case CardinalDirection.South:
                    rail.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case CardinalDirection.East:
                case CardinalDirection.West:
                    rail.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }

            return rail;
        }
    }
}