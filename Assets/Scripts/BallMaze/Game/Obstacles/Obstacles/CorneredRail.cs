using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    [Serializable]
    public class CorneredRail : Obstacle, IAbsolutelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.CorneredRail;

        public override bool canRespawnOn => true;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public CardinalDirection direction { get; set; }


        public CorneredRail(int id, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject corneredRail;
            int railDirection = (int)direction;

            Maze.GetPositionInTypesMap(obstaclesTypesMap, this, out int x, out int y);

            // Get the adjacent obstacles in the direction of the cornered rail and the direction 90 degrees to the right
            ObstacleType firstEndObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, direction);
            ObstacleType lastEndObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, (CardinalDirection)(railDirection + 1));

            bool isFirstEndRail = firstEndObstacleType == ObstacleType.Rail || firstEndObstacleType == ObstacleType.CorneredRail;
            bool isLastEndRail = lastEndObstacleType == ObstacleType.Rail || lastEndObstacleType == ObstacleType.CorneredRail;

            if (Application.isPlaying)
            {
                // Load the correct rail model based on the adjacent obstacles
                if (isFirstEndRail && isLastEndRail)
                {
                    corneredRail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/corneredrail0connections.fbx");
                }
                else if (!isFirstEndRail && !isLastEndRail)
                {
                    corneredRail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/corneredrail2connections.fbx");
                }
                else if (isFirstEndRail && !isLastEndRail)
                {
                    corneredRail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/corneredrail1connectionsright.fbx");

                    // Update the direction of the rail, otherwise the model will be not rotated properly
                    railDirection += 1;
                }
                else
                {
                    corneredRail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/corneredrail1connectionsleft.fbx");
                }
            }
            else
            {
#if UNITY_EDITOR
                // Load the correct rail model based on the adjacent obstacles
                if (isFirstEndRail && isLastEndRail)
                {
                    corneredRail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/CorneredRail0Connections.fbx", typeof(GameObject)));
                }
                else if (!isFirstEndRail && !isLastEndRail)
                {
                    corneredRail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/CorneredRail2Connections.fbx", typeof(GameObject)));
                }
                else if (isFirstEndRail && !isLastEndRail)
                {
                    corneredRail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/CorneredRail1ConnectionsRight.fbx", typeof(GameObject)));

                    // Update the direction of the rail, otherwise the model will be not rotated properly
                    railDirection += 1;
                }
                else
                {
                    corneredRail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/CorneredRail1ConnectionsLeft.fbx", typeof(GameObject)));
                }
#else
            return null;
#endif
            }


            corneredRail.name = "CorneredRail";
            corneredRail.transform.position = position;
            corneredRail.transform.rotation = Quaternion.Euler(0, railDirection * 90, 0);

            if (Application.isPlaying)
            {
                corneredRail.transform.Find("CorneredRail_InteriorRail").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/rail/rail.mat");
                corneredRail.transform.Find("CorneredRail_ExteriorRail").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/rail/rail.mat");
            }

            // If debug mode is off, remove the hitboxes gameobjects since they shouldn't be visible
            if (!GameManager.DEBUG)
            {
                corneredRail.transform.Find("CorneredRail_InteriorRail_HitBox").GetComponent<MeshRenderer>().enabled = false;
                corneredRail.transform.Find("CorneredRail_ExteriorRail_HitBox").GetComponent<MeshRenderer>().enabled = false;
                corneredRail.transform.Find("CorneredRail_Floor1").GetComponent<MeshRenderer>().enabled = false;
                corneredRail.transform.Find("CorneredRail_Floor2").GetComponent<MeshRenderer>().enabled = false;
                corneredRail.transform.Find("CorneredRail_Floor3").GetComponent<MeshRenderer>().enabled = false;
            }

            return corneredRail;
        }
    }
}