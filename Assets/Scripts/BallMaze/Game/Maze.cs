using BallMaze.Obstacles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace BallMaze
{
    public class Maze : MonoBehaviour
    {
        private GameObject _maze;
        private LevelLoader _levelLoader;

        public GameObject start;

        // The y level at which the player loses when the ball falls. Calculated when the maze is built
        public float voidYLevel = -10f;

        // A list of all the obstacles of the maze
        public Obstacle[] obstaclesList;

        // A dictionary that associates each obstacle GameObject to its corresponding Obstacle object
        public Dictionary<GameObject, Obstacle> obstacles = new Dictionary<GameObject, Obstacle>();

        // This represents a 2D map of the level and allows to easily get adjacents obstacles for example
        public int[,] obstaclesTypesMap;

        private AssetBundle _obstaclesAssetBundle;
        private AssetBundleCreateRequest _obstaclesAssetBundleCreateRequest;

        // Map all obstacles names with the associated loaded game object
        private Dictionary<string, UnityEngine.Object> _obstaclesGameObjects = new Dictionary<string, UnityEngine.Object>();

        private bool _initialized = false;
        public static bool ObstaclesBundleLoaded = false;


        void Awake()
        {
            _maze = GameObject.Find("Maze");

            _levelLoader = GetComponent<LevelLoader>();
        }


        /// <summary>
        /// Initialize the obstacles containers. Must be called once at the beginning of the game.
        /// </summary>
        public void InitMaze()
        {
            if (_initialized)
            {
                return;
            }

            // Delete the maze if it already exists before loading (from the editor for example)
            foreach (Transform transform in _maze.transform)
            {
                Destroy(transform.gameObject);
            }

            // Create all container objects
            new GameObject("FlagTargets").transform.SetParent(_maze.transform);
            new GameObject("Floors").transform.SetParent(_maze.transform);
            new GameObject("Walls").transform.SetParent(_maze.transform);
            new GameObject("Corners").transform.SetParent(_maze.transform);
            new GameObject("AbsolutelyPositionnableObstacles").transform.SetParent(_maze.transform);
            new GameObject("RelativelyPositionnableObstacles").transform.SetParent(_maze.transform);

            // Get the start object if it already exists or create a new one
            start = new GameObject("Start");
            start.transform.SetParent(_maze.transform);

            // Load the obstacles asset bundle so that obstacles can then be created
            _obstaclesAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "ObstaclesAssetBundle", "obstacles"));
            _obstaclesAssetBundleCreateRequest.completed += OnObstacleAssetBundleLoaded;

            _initialized = true;
        }


        /// <summary>
        /// Builds the given maze.
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelId"></param>
        /// <param name="offset">The offset at which to build the maze relative to the origin (0,0,0)</param>
        /// <returns>A boolean indicating if the maze could be loaded</returns>
        public bool BuildMaze(LevelType levelType, Level level, Vector3? offset = null)
        {
            offset = offset ?? Vector3.zero;

            obstacles.Clear();
            obstaclesList = null;
            obstaclesTypesMap = null;

            // Update the void level based on the maze size. The formula uses trigonometry (a = c * sin(α)) and then multiplies by 5
            // to have a margin and negative to have the void level below the maze
            voidYLevel = level.mazeSize.x > level.mazeSize.z
            ? level.mazeSize.x / 2 * Mathf.Sin(Controls.MAX_MAZE_ORIENTATION * Mathf.Deg2Rad) * -5f
            : level.mazeSize.z / 2 * Mathf.Sin(Controls.MAX_MAZE_ORIENTATION * Mathf.Deg2Rad) * -5f;

            obstaclesList = new Obstacle[level.nbObstacles];
            obstaclesTypesMap = InitObstaclesTypesMap((int)Mathf.Round(level.mazeSize.x), (int)Mathf.Round(level.mazeSize.z));

            _maze.transform.Find("Start").position = level.startPosition;

            foreach (Floor floor in level.floors)
            {
                floor.offset = offset.Value;

                obstaclesList[floor.id] = floor;
                AddObstacleToTypesMap(obstaclesTypesMap, floor);
            }

            // Obstacles
            foreach (Obstacle obstacle in level.obstacles)
            {
                if (obstacle is IAbsolutelyPositionnable)
                {
                    (obstacle as IAbsolutelyPositionnable).offset = offset.Value;

                    obstaclesList[obstacle.id] = obstacle;
                    AddObstacleToTypesMap(obstaclesTypesMap, obstacle);
                }
                else if (obstacle is IRelativelyPositionnable)
                {
                    obstaclesList[obstacle.id] = obstacle;
                    AddObstacleToTypesMap(obstaclesTypesMap, obstacle);
                }
            }

            // Walls
            foreach (Wall wall in level.walls)
            {
                obstaclesList[wall.id] = wall;
                AddObstacleToTypesMap(obstaclesTypesMap, wall);
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                obstaclesList[corner.id] = corner;
                AddObstacleToTypesMap(obstaclesTypesMap, corner);
            }

            // Target
            obstaclesList[level.target.id] = level.target;
            AddObstacleToTypesMap(obstaclesTypesMap, level.target);

            return true;
        }


        /// <summary>
        /// Builds the maze matching the given level.
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelId"></param>
        /// <param name="offset">The offset at which to build the maze relative to the origin (0,0,0)</param>
        /// <returns>A boolean indicating if the maze could be loaded</returns>
        public bool BuildMaze(LevelType levelType, string levelId, Vector3? offset = null)
        {
            Level level = _levelLoader.DeserializeLevel(levelType, levelId);
            if (level == null)
            {
                return false;
            }

            return BuildMaze(levelType, level, offset);
        }


        /// <summary>
        /// Render all the obstacles of the maze.
        /// </summary>
        /// <param name="obstaclesList">A list of the obstacles to render</param>
        /// <param name="obstacles">A dictionary to link the newly instantiated GameObject with its obstacle</param>
        /// <param name="obstaclesTypesMap">A 2D int matrix representing the map viewed from above. This allows to get neighboring obstacles easily</param>
        /// <param name="mazeRootName">The name of the game object to which the maze will be parented to</param>
        public static void RenderAllObstacles(Obstacle[] obstaclesList, Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap, string mazeRootName = "Maze")
        {
            GameObject maze = GameObject.Find(mazeRootName);

            if (maze == null)
            {
                Debug.LogError("Maze object not found");
                return;
            }

            Transform flagTargetsContainer = maze.transform.Find("FlagTargets");
            Transform floorsContainer = maze.transform.Find("Floors");
            Transform wallsContainer = maze.transform.Find("Walls");
            Transform cornersContainer = maze.transform.Find("Corners");
            Transform absolutelyPositionnableObstaclesContainer = maze.transform.Find("AbsolutelyPositionnableObstacles");
            Transform relativelyPositionnableObstaclesContainer = maze.transform.Find("RelativelyPositionnableObstacles");

            foreach (Obstacle obstacle in obstaclesList)
            {
                GameObject obstacleGameObject = obstacle.Render(obstacles, obstaclesTypesMap);

                if (obstacleGameObject == null)
                {
                    continue;
                }

                switch (obstacle.obstacleType)
                {
                    case ObstacleType.Floor:
                        obstacleGameObject.transform.SetParent(floorsContainer);
                        break;
                    case ObstacleType.Wall:
                        obstacleGameObject.transform.SetParent(wallsContainer);
                        break;
                    case ObstacleType.Corner:
                        obstacleGameObject.transform.SetParent(cornersContainer);
                        break;
                    case ObstacleType.FlagTarget:
                        obstacleGameObject.transform.SetParent(flagTargetsContainer);
                        break;
                    default:
                        if (obstacle is IAbsolutelyPositionnable)
                        {
                            obstacleGameObject.transform.SetParent(absolutelyPositionnableObstaclesContainer);
                        }
                        else if (obstacle is IRelativelyPositionnable)
                        {
                            obstacleGameObject.transform.SetParent(relativelyPositionnableObstaclesContainer);
                        }
                        break;
                }

                obstacles[obstacleGameObject] = obstacle;
            }
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
        public void ClearMaze(GameObject mazeGameObject = null)
        {
            mazeGameObject = mazeGameObject ?? gameObject;

            // Loop through all the obstacles and delete them
            foreach (Transform transform in mazeGameObject.transform)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
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


        /// <summary>
        /// Save the obstacles asset bundle once it's loaded
        /// </summary>
        /// <param name="_"></param>
        private void OnObstacleAssetBundleLoaded(AsyncOperation _)
        {
            _obstaclesAssetBundle = _obstaclesAssetBundleCreateRequest.assetBundle;

            if (_obstaclesAssetBundle == null)
            {
                ExceptionManager.ShowExceptionMessage(new Exception($"Failed to load obstacles asset bundle"), "ExceptionMessagesTable", "ObstaclesAssetBundleLoadError");
                return;
            }

            foreach (string obstacleName in _obstaclesAssetBundle.GetAllAssetNames())
            {
                _obstaclesGameObjects[obstacleName] = _obstaclesAssetBundle.LoadAsset<UnityEngine.Object>(obstacleName);
            }

            ObstaclesBundleLoaded = true;
        }


        /// <summary>
        /// Return an instance of the gameobject at the given path. The gameobject must be in the obstacles asset bundle.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject GetObstacleGameObjectFromPath(string path)
        {
            if (_obstaclesGameObjects.ContainsKey(path))
            {
                return Instantiate((GameObject)_obstaclesGameObjects[path]);
            }

            ExceptionManager.ShowExceptionMessage(new Exception("Failed to load obstacle from asset bundle"), "ExceptionMessagesTable", "ObstaclesAssetBundleLoadError");
            return null;
        }


        /// <summary>
        /// Return the material at the given path. The material must be in the obstacles asset bundle.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Material GetObstacleMaterialFromPath(string path)
        {
            if (_obstaclesGameObjects.ContainsKey(path))
            {
                return (Material)_obstaclesGameObjects[path];
            }

            ExceptionManager.ShowExceptionMessage(new Exception("Failed to load obstacle from asset bundle"), "ExceptionMessagesTable", "ObstaclesAssetBundleLoadError");
            return null;
        }


        private void OnDestroy()
        {
            if (_obstaclesAssetBundle != null)
            {
                _obstaclesAssetBundle.Unload(true);
            }
        }
    }
}