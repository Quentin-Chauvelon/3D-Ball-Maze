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

        public override bool canRollOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }
        public Vector3 offset { get; set; }

        public CardinalDirection direction { get; set; }


        public Rail(int id, Vector3 position, CardinalDirection direction) : base(id)
        {
            this.position = position;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject rail;
            Quaternion rotationIncrement = Quaternion.identity;

            // Boolean to know if there are other rails at each end of the rail
            bool isFirstEndRail = false;
            bool isLastEndRail = false;

            Maze.GetPositionInTypesMap(obstaclesTypesMap, this, out int x, out int y);

            switch (direction)
            {
                case CardinalDirection.North:
                case CardinalDirection.South:
                    // Increment the rotation of the rail based on the direction
                    rotationIncrement = Quaternion.Euler(0, 0, 0);

                    // Get the north and south adjacent obstacles to update which rail model to instantiate
                    ObstacleType northObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.North);
                    isFirstEndRail = northObstacleType == ObstacleType.Rail || northObstacleType == ObstacleType.CorneredRail;

                    ObstacleType southObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.South);
                    isLastEndRail = southObstacleType == ObstacleType.Rail || southObstacleType == ObstacleType.CorneredRail;

                    break;

                case CardinalDirection.East:
                case CardinalDirection.West:
                    rotationIncrement = Quaternion.Euler(0, 90, 0);

                    ObstacleType eastObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.East);
                    if (eastObstacleType == ObstacleType.Rail || eastObstacleType == ObstacleType.CorneredRail)
                    {
                        isFirstEndRail = true;
                    }

                    ObstacleType westObstacleType = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.West);
                    if (westObstacleType == ObstacleType.Rail || westObstacleType == ObstacleType.CorneredRail)
                    {
                        isLastEndRail = true;
                    }

                    break;
            }

            if (Application.isPlaying)
            {
                // Load the correct rail model based on the adjacents obstacles
                if (isFirstEndRail && isLastEndRail)
                {
                    rail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/straightrail0connections.prefab");
                }
                else if (!isFirstEndRail && !isLastEndRail)
                {
                    rail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/straightrail2connections.fbx");
                }
                else
                {
                    rail = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/rails/straightrail1connections.fbx");

                    // Rotate the rail 180 degrees if it is connected to something other than a rail towards the south
                    if (isFirstEndRail)
                    {
                        rail.transform.rotation *= Quaternion.Euler(0, 180, 0);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                // Load the correct rail model based on the adjacents obstacles
                if (isFirstEndRail && isLastEndRail)
                {
                    rail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/StraightRail0Connections.prefab", typeof(GameObject)));
                }
                else if (!isFirstEndRail && !isLastEndRail)
                {
                    rail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/StraightRail2Connections.fbx", typeof(GameObject)));
                }
                else
                {
                    rail = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Rails/StraightRail1Connections.fbx", typeof(GameObject)));

                    // Rotate the rail 180 degrees if it is connected to something other than a rail towards the south
                    if (isFirstEndRail)
                    {
                        rail.transform.rotation *= Quaternion.Euler(0, 180, 0);
                    }
                }
#else
                return null;
#endif
            }

            rail.name = "Rail";
            rail.transform.position = position + offset;
            rail.transform.rotation *= rotationIncrement;

            if (Application.isPlaying)
            {
                rail.transform.Find("StraightRail_LeftRail").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/rail/rail.mat");
                rail.transform.Find("StraightRail_RightRail").GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/rail/rail.mat");
            }

            // If debug mode is off, remove the hitboxes gameobjects since they shouldn't be visible
            if (!GameManager.DEBUG)
            {
                // If the rail is the 2 connections rail, there are 4 hitboxes to remove
                if (!isFirstEndRail && !isLastEndRail)
                {
                    rail.transform.Find("StraightRail_LeftRail_HitBox1").GetComponent<MeshRenderer>().enabled = false;
                    rail.transform.Find("StraightRail_LeftRail_HitBox2").GetComponent<MeshRenderer>().enabled = false;
                    rail.transform.Find("StraightRail_RightRail_HitBox1").GetComponent<MeshRenderer>().enabled = false;
                    rail.transform.Find("StraightRail_RightRail_HitBox2").GetComponent<MeshRenderer>().enabled = false;
                }
                // Otherwise, there is only 2
                else
                {
                    rail.transform.Find("StraightRail_LeftRail_HitBox").GetComponent<MeshRenderer>().enabled = false;
                    rail.transform.Find("StraightRail_RightRail_HitBox").GetComponent<MeshRenderer>().enabled = false;
                }

                rail.transform.Find("StraightRail_Floor1").GetComponent<MeshRenderer>().enabled = false;
                rail.transform.Find("StraightRail_Floor2").GetComponent<MeshRenderer>().enabled = false;
            }

            return rail;
        }
    }
}