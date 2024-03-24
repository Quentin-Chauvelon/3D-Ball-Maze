using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class HalfCylinder : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.HalfCylinder;

        public override bool canRespawnOn => true;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public CardinalDirection direction { get; set; }


        public HalfCylinder(int id, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject halfcylinder = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/HalfCylinder.fbx", typeof(GameObject)));
            halfcylinder.name = "HalfCylinder";

            halfcylinder.transform.position = position;

            switch (direction)
            {
                case CardinalDirection.North:
                case CardinalDirection.South:
                    halfcylinder.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case CardinalDirection.East:
                case CardinalDirection.West:
                    halfcylinder.transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
            }

            halfcylinder.transform.Find("HalfCylinder_Floor").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));
            halfcylinder.transform.Find("HalfCylinder_Cylinder").GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return halfcylinder;
        }
    }
}