using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityExtensionMethods;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple wall
    /// </summary>
    public class Wall : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Wall;

        public override bool canRespawnOn => false;

        public override bool canRollOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }


        public Wall(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject wall;

            Maze.GetPositionInTypesMap(obstaclesTypesMap, this, out int x, out int y, obstacles);

            bool north = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.North) == ObstacleType.Wall;
            bool east = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.East) == ObstacleType.Wall;
            bool south = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.South) == ObstacleType.Wall;
            bool west = Maze.GetAdjacentObstacleInDirection(obstaclesTypesMap, x, y, CardinalDirection.West) == ObstacleType.Wall;
            bool mesh = !((north && south) || (east && west)); // true if the wall is a mesh (cornered, half rounded or rounded)

            if (Application.isPlaying)
            {
                // If there are at least walls on both sides of the wall (north and south or east and west), use a basic flat cube (straight without rounded end)
                if ((north && south) || (east && west))
                {
                    wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                // If there are walls on two adjacents sides (north and east, east and south, ...), the wall is a corner
                else if ((north && east) || (east && south) || (south && west) || (west && north))
                {
                    wall = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/walls/cornered_wall.fbx");
                }
                // If there is only a wall on one side (rounded end)
                else if (north || east || south || west)
                {
                    wall = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/walls/half_rounded_wall.fbx");
                }
                // Otherwise it's a standalone wall (fully rounded)
                else
                {
                    wall = LevelManager.Instance.Maze.GetObstacleGameObjectFromPath("assets/art/models/obstacles/walls/rounded_wall.fbx");
                }
            }
            else
            {
#if UNITY_EDITOR
                if ((north && south) || (east && west))
                {
                    wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                else if ((north && east) || (east && south) || (south && west) || (west && north))
                {
                    wall = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Walls/Cornered_Wall.fbx", typeof(GameObject)));
                }
                else if (north || east || south || west)
                {
                    wall = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Walls/Half_Rounded_Wall.fbx", typeof(GameObject)));
                }
                else
                {
                    wall = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)AssetDatabase.LoadAssetAtPath("Assets/Art/Models/Obstacles/Walls/Rounded_Wall.fbx", typeof(GameObject)));
                }
#else
            return null;
#endif
            }

            wall.name = "Wall";

            wall.transform.localScale = mesh ? new Vector3(1f, 1f, 0.5f) : new Vector3(1f, 0.5f, 1f);

            PositionObstacleOverObstacleFromId(obstacles, wall.transform, obstacleId, new Vector3(0, 0.2f, 0));

            if ((north && south) || (east && west)) { } // Nothing to do here but still needed so that we can use the last condition with all OR
            else if ((north && east) || (east && south) || (south && west) || (west && north))
            {
                if (north && east)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, -90, 0);
                }
                else if (east && south)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, 180, 0);
                }
                else if (south && west)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, 90, 0);
                }
            }
            else if (north || east || south || west)
            {
                if (north)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, 180, 0);
                }
                else if (east)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, -90, 0);
                }
                else if (west)
                {
                    wall.transform.rotation = Quaternion.Euler(-90, 90, 0);
                }
            }

            if (Application.isPlaying)
            {
                wall.GetComponent<MeshRenderer>().material = LevelManager.Instance.Maze.GetObstacleMaterialFromPath("assets/art/materials/obstacles/wall.mat");
            }

            return wall;
        }
    }
}