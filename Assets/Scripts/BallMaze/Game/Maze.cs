using BallMaze.Obstacles;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityExtensionMethods;


namespace BallMaze
{
    public class Maze : MonoBehaviour
    {
        private GameObject _maze;
        private LevelLoader _levelLoader;

        public GameObject start;

        // A list of all the obstacles of the maze
        private Obstacle[] _obstaclesList;

        // A dictionary that associates each obstacle GameObject to its corresponding Obstacle object
        public Dictionary<GameObject, Obstacle> obstacles = new Dictionary<GameObject, Obstacle>();

        // This represents a 2D map of the level and allows to easily get adjacents obstacles for example
        private int[,] _obstaclesTypesMap;


        void Awake()
        {
            _maze = GameObject.Find("Maze");

            _levelLoader = GetComponent<LevelLoader>();
        }


        /// <summary>
        /// Builds the maze matching the given level.
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelId"></param>
        /// <returns>A boolean indicating if the maze could be loaded</returns>
        public bool BuildMaze(LevelType levelType, string levelId)
        {
            obstacles.Clear();
            _obstaclesList = null;
            _obstaclesTypesMap = null;

            Level level = _levelLoader.DeserializeLevel(levelType, levelId);
            if (level == null)
            {
                return false;
            }

            _obstaclesList = new Obstacle[level.nbObstacles];
            _obstaclesTypesMap = InitObstaclesTypesMap((int)Mathf.Round(level.mazeSize.x), (int)Mathf.Round(level.mazeSize.z));

            // Create all container objects
            GameObject flagTargetsContainer = new GameObject("FlagTargets");
            GameObject floorsContainer = new GameObject("Floors");
            GameObject wallsContainer = new GameObject("Walls");
            GameObject cornersContainer = new GameObject("Corners");
            GameObject absolutelyPositionnableObstaclesContainer = new GameObject("AbsolutelyPositionnableObstacles");
            GameObject relativelyPositionnableObstaclesContainer = new GameObject("RelativelyPositionnableObstacles");
            flagTargetsContainer.transform.SetParent(_maze.transform);
            floorsContainer.transform.SetParent(_maze.transform);
            wallsContainer.transform.SetParent(_maze.transform);
            cornersContainer.transform.SetParent(_maze.transform);
            absolutelyPositionnableObstaclesContainer.transform.SetParent(_maze.transform);
            relativelyPositionnableObstaclesContainer.transform.SetParent(_maze.transform);

            // Create the start object
            start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(_maze.transform);

            foreach (Floor floor in level.floors)
            {
                _obstaclesList[floor.id] = floor;
                AddObstacleToTypesMap(_obstaclesTypesMap, floor);
            }

            // Obstacles
            foreach (Obstacle obstacle in level.obstacles)
            {
                if (obstacle is IAbsolutelyPositionnable)
                {
                    _obstaclesList[obstacle.id] = obstacle;
                    AddObstacleToTypesMap(_obstaclesTypesMap, obstacle);
                }
                else if (obstacle is IRelativelyPositionnable)
                {
                    _obstaclesList[obstacle.id] = obstacle;
                    AddObstacleToTypesMap(_obstaclesTypesMap, obstacle);
                }
            }

            // Walls
            foreach (Wall wall in level.walls)
            {
                _obstaclesList[wall.id] = wall;
                AddObstacleToTypesMap(_obstaclesTypesMap, wall);
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                _obstaclesList[corner.id] = corner;
                AddObstacleToTypesMap(_obstaclesTypesMap, corner);
            }

            // Target
            _obstaclesList[level.target.id] = level.target;
            AddObstacleToTypesMap(_obstaclesTypesMap, level.target);

            RenderAllObstacles(_obstaclesList, obstacles, _obstaclesTypesMap);

            return true;
        }


        public static void RenderAllObstacles(Obstacle[] obstaclesList, Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            foreach (Obstacle obstacle in obstaclesList)
            {
                GameObject obstacleGameObject = obstacle.Render(obstacles, obstaclesTypesMap);

                switch (obstacle.obstacleType)
                {
                    case ObstacleType.Floor:
                        obstacleGameObject.transform.SetParent(GameObject.Find("Floors").transform);
                        break;
                    case ObstacleType.Wall:
                        obstacleGameObject.transform.SetParent(GameObject.Find("Walls").transform);
                        break;
                    case ObstacleType.Corner:
                        obstacleGameObject.transform.SetParent(GameObject.Find("Corners").transform);
                        break;
                    case ObstacleType.FlagTarget:
                        obstacleGameObject.transform.SetParent(GameObject.Find("FlagTargets").transform);
                        break;
                    default:
                        if (obstacle is IAbsolutelyPositionnable)
                        {
                            obstacleGameObject.transform.SetParent(GameObject.Find("AbsolutelyPositionnableObstacles").transform);
                        }
                        else if (obstacle is IRelativelyPositionnable)
                        {
                            obstacleGameObject.transform.SetParent(GameObject.Find("RelativelyPositionnableObstacles").transform);
                        }
                        break;
                }

                obstacles[obstacleGameObject] = obstacle;
            }
        }


        /// <summary>
        /// Returns a boolen indicating if a maze is already loaded.
        /// </summary>
        public bool IsMazeLoaded()
        {
            return _maze.transform.childCount > 0;
        }


        /// <summary>
        /// Returns the bounds of the maze (total size of all its descendants).
        /// </summary>
        /// <returns></returns>
        public Bounds GetMazeBounds()
        {
            Renderer[] renderers = _maze.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }


        /// <summary>
        /// Deletes all descendants of the maze object.
        /// </summary>
        public void ClearMaze()
        {
            // Loop through all the maze's children and delete them
            foreach (Transform transform in gameObject.transform)
            {
                Destroy(transform.gameObject);
            }

            ResetMazeOrientation();
        }


        /// <summary>
        ///  Update the maze orientation to match the given rotation.
        /// </summary>
        /// <param name="rotation"></param>
        public void UpdateMazeOrientation(Quaternion rotation)
        {
            _maze.transform.rotation = rotation;
        }


        /// <summary>
        /// Resets the maze orientation to the default one.
        /// </summary>
        public void ResetMazeOrientation()
        {
            UpdateMazeOrientation(Quaternion.identity);
        }


        public static UnityEngine.Object InstantiateResource(string path)
        {
            return Instantiate(Resources.Load(path));
        }

        /// <summary>
        /// Initialize the obstacles types map with the given m and n size
        /// By default, all values in the array are instantiated at -1
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int[,] InitObstaclesTypesMap(int m, int n)
        {
            int[,] obstaclesTypesMap = new int[m, n];

            // Initialize all values of the obstacles types map to -1
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    obstaclesTypesMap[i, j] = -1;
                }
            }

            return obstaclesTypesMap;
        }


        /// <summary>
        /// Add the given obstacle to the obstacles types 2D map
        /// </summary>
        /// <param name="obstaclesTypesMap"></param>
        /// <param name="obstacle"></param>
        public static void AddObstacleToTypesMap(int[,] obstaclesTypesMap, Obstacle obstacle)
        {
            // If the obstacle is not positionned absolutely, it is above another obstacle
            // and thus we only want the obstacle under it
            if (!(obstacle is IAbsolutelyPositionnable))
            {
                return;
            }

            GetPositionInTypesMap(obstaclesTypesMap, obstacle, out int x, out int z);

            // Debug.Log($"position: {absolutelyPositionnable.position}");
            // Debug.Log($"len0: {obstaclesTypesMap.GetLength(0)} len1: {obstaclesTypesMap.GetLength(1)} x: {x} z: {z}");
            obstaclesTypesMap[x, z] = (int)obstacle.obstacleType;
        }


        /// <summary>
        /// Return the x and y position of the obstacle in the types map
        /// </summary>
        /// <param name="obstaclesTypesMap"></param>
        /// <param name="obstacle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void GetPositionInTypesMap(int[,] obstaclesTypesMap, Obstacle obstacle, out int x, out int y)
        {
            // If the obstacle is not positionned absolutely, it is above another obstacle
            // and thus we only want the obstacle under it
            if (!(obstacle is IAbsolutelyPositionnable))
            {
                x = 0;
                y = 0;

                return;
            }

            if (obstacle is IAbsolutelyPositionnable)
            {
                IAbsolutelyPositionnable absolutelyPositionnable = (IAbsolutelyPositionnable)obstacle;

                // Translate the position of the obstacle to have the lower left point of the maze be at (0,0)
                x = Mathf.FloorToInt(absolutelyPositionnable.position.x) + Mathf.FloorToInt(obstaclesTypesMap.GetLength(0) / 2);
                y = Mathf.FloorToInt(absolutelyPositionnable.position.z) + Mathf.FloorToInt(obstaclesTypesMap.GetLength(1) / 2);

                return;
            }

            x = 0;
            y = 0;
        }


        /// <summary>
        /// Print the obstacles types map to the console
        /// </summary>
        /// <param name="obstaclesTypesMap"></param>
        public static void PrintTypesMap(int[,] obstaclesTypesMap)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < obstaclesTypesMap.GetLength(0); i++)
            {
                for (int j = 0; j < obstaclesTypesMap.GetLength(1); j++)
                {
                    sb.Append(obstaclesTypesMap[i, j]);
                    sb.Append(' ');
                }

                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }


        public static ObstacleType GetAdjacentObstacleInDirection(int[,] obstaclesTypesMap, int x, int y, CardinalDirection direction)
        {
            switch (direction)
            {
                case CardinalDirection.North:

                    if (y > 0)
                    {
                        return (ObstacleType)obstaclesTypesMap[x, y - 1];
                    }

                    break;

                case CardinalDirection.East:

                    if (x > 0)
                    {
                        return (ObstacleType)obstaclesTypesMap[x - 1, y];
                    }

                    break;

                case CardinalDirection.South:

                    if (y < obstaclesTypesMap.GetLength(1) - 1)
                    {
                        return (ObstacleType)obstaclesTypesMap[x, y + 1];
                    }

                    break;

                case CardinalDirection.West:

                    if (x < obstaclesTypesMap.GetLength(0) - 1)
                    {
                        return (ObstacleType)obstaclesTypesMap[x + 1, y];
                    }

                    break;
            }

            return ObstacleType.Undefined;
        }


        /// <summary>
        /// Return the obstacle under the given obstacle (if it's relatively positionned)
        /// </summary>
        /// <param name="obstacles"></param>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        public static Obstacle GetObstacleIdUnderObstacle(Dictionary<GameObject, Obstacle> obstacles, Obstacle obstacle)
        {
            if (!(obstacle is IAbsolutelyPositionnable))
            {
                return obstacle;
            }

            IRelativelyPositionnable relativelyPositionnable = (IRelativelyPositionnable)obstacle;

            foreach (KeyValuePair<GameObject, Obstacle> obstacleKeyValuePair in obstacles)
            {
                if (obstacleKeyValuePair.Value.id == relativelyPositionnable.obstacleId)
                {
                    return obstacleKeyValuePair.Value;
                }
            }

            return obstacle;
        }
    }
}