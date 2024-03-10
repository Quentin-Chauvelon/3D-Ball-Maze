using BallMaze;
using BallMaze.Obstacles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.Editor
{
    public class LoadMazeFromFile : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset = default;
        private TextField _fileName;
        private TextField _id;

        private Button _loadButton;
        private Button _clearMazeButton;

        private Dictionary<int, Obstacle> _obstacles = new Dictionary<int, Obstacle>();


        [MenuItem("Utilities/Load Maze From File", false, 21)]
        public static void OpenLoadMazeWindow()
        {
            LoadMazeFromFile loadMazeWindow = GetWindow<LoadMazeFromFile>();
            loadMazeWindow.titleContent = new GUIContent("Load Maze From File");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/LoadMazeFromFile/LoadMazeFromFile.uxml");
            VisualElement loadMazeFromFileUXML = _visualTreeAsset.Instantiate();
            root.Add(loadMazeFromFileUXML);

            _fileName = loadMazeFromFileUXML.Query<TextField>("FileName");
            _id = loadMazeFromFileUXML.Query<TextField>("Id");

            _loadButton = loadMazeFromFileUXML.Query<Button>("LoadButton");
            _loadButton.clicked += LoadMaze;

            _clearMazeButton = loadMazeFromFileUXML.Query<Button>("ClearMazeButton");
            _clearMazeButton.clicked += ClearMaze;
        }


        /// <summary>
        /// Loads maze from the file.
        /// </summary>
        private void LoadMaze()
        {
            Debug.Log("Loading: Loading level " + _id.value + " from file: " + _fileName.value);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            string jsonData = File.ReadAllText(Path.Combine(Application.persistentDataPath, "levels", _fileName.value + ".json"));

            // Get the line containing the level's data
            foreach (string line in jsonData.Split('\n'))
            {
                if (line.Contains("\"id\":\"" + _id.value + "\""))
                {
                    jsonData = line.Trim(new char[] { ' ', ',', '\r', '\n' });
                }
            }

            Level level = JsonConvert.DeserializeObject<Level>(jsonData);

            GameObject maze = GameObject.Find("Maze");
            if (!maze)
            {
                maze = new GameObject("Maze");
            }

            GameObject flagTargetsContainer = new GameObject("FlagTargets");
            GameObject floorsContainer = new GameObject("Floors");
            GameObject wallsContainer = new GameObject("Walls");
            GameObject cornersContainer = new GameObject("Corners");
            GameObject obstaclesContainer = new GameObject("Obstacles");
            flagTargetsContainer.transform.SetParent(maze.transform);
            floorsContainer.transform.SetParent(maze.transform);
            wallsContainer.transform.SetParent(maze.transform);
            cornersContainer.transform.SetParent(maze.transform);
            obstaclesContainer.transform.SetParent(maze.transform);

            GameObject start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(maze.transform);

            // Floor tiles
            foreach (Floor floor in level.floors)
            {
                // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after benchmarking it,
                // it appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
                GameObject floorGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floorGameObject.name = "Floor";
                floorGameObject.transform.localScale = new Vector3(1, 0.1f, 1);
                floorGameObject.transform.position = floor.position;
                floorGameObject.transform.SetParent(floorsContainer.transform);

                _obstacles[floor.id] = floor;
            }

            // Walls
            foreach (Wall wall in level.walls)
            {
                GameObject wallGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallGameObject.name = "Wall";

                // Adapt the wall's scale to the direction it's facing (equivalent to rotating the wall)
                if (wall.direction == CardinalDirection.North || wall.direction == CardinalDirection.South)
                {
                    wallGameObject.transform.localScale = new Vector3(1, 0.5f, 0.1f);
                }
                else
                {
                    wallGameObject.transform.localScale = new Vector3(0.1f, 0.5f, 1);
                }

                Vector3 offset = Vector3.zero;
                // Add an offset to move the wall to the correct position based on the direction it's facing
                switch (wall.direction)
                {
                    case CardinalDirection.North:
                        offset = new Vector3(0, 0, -0.45f);
                        break;
                    case CardinalDirection.East:
                        offset = new Vector3(-0.45f, 0, 0);
                        break;
                    case CardinalDirection.South:
                        offset = new Vector3(0, 0, 0.45f);
                        break;
                    case CardinalDirection.West:
                        offset = new Vector3(0.45f, 0, 0);
                        break;
                    default:
                        Debug.LogWarning($"Invalid wall direction {wall.direction} for wall {wall.id}.");
                        break;
                }

                PositionObstacleOverObstacleFromId(wallGameObject.transform, wall.obstacleId, offset + new Vector3(0, 0.2f, 0));

                wallGameObject.transform.SetParent(wallsContainer.transform);
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                GameObject cornerGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cornerGameObject.name = "Corner";
                cornerGameObject.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

                Vector3 offset = Vector3.zero;
                // Add an offset to move the wall to the correct position based on the direction it's facing
                switch (corner.direction)
                {
                    case CardinalDirection.North:
                        offset = new Vector3(-0.45f, 0, -0.45f);
                        break;
                    case CardinalDirection.East:
                        offset = new Vector3(-0.45f, 0, 0.45f);
                        break;
                    case CardinalDirection.South:
                        offset = new Vector3(0.45f, 0, 0.45f);
                        break;
                    case CardinalDirection.West:
                        offset = new Vector3(0.45f, 0, -0.45f);
                        break;
                    default:
                        Debug.LogWarning($"Invalid wall direction {corner.direction} for wall {corner.id}.");
                        break;
                }

                PositionObstacleOverObstacleFromId(cornerGameObject.transform, corner.obstacleId, offset + new Vector3(0, 0.2f, 0));

                cornerGameObject.transform.SetParent(cornersContainer.transform);
            }

            // Target
            GameObject targetGameObject = (GameObject)Instantiate(Resources.Load("Level/Targets/FlagTarget"));
            targetGameObject.name = "Target";

            PositionObstacleOverObstacleFromId(targetGameObject.transform, level.target.obstacleId, new Vector3(0, 0.4f, 0));

            targetGameObject.transform.SetParent(flagTargetsContainer.transform);

            stopwatch.Stop();
            Debug.Log("Loading: Done (" + stopwatch.ElapsedMilliseconds.ToString() + "ms)");
        }


        private void PositionObstacleOverObstacleFromId(Transform transform, int obstacleId, Vector3 offset)
        {
            if (_obstacles[obstacleId] is IAbsolutelyPositionnable)
            {
                transform.position = (_obstacles[obstacleId] as IAbsolutelyPositionnable).position + offset;
                Debug.Log("offset" + offset);
            }
            else
            {
                Debug.LogWarning($"Could not find obstacle with id {obstacleId} to position {transform.name} over.");
            }
        }


        private void ClearMaze()
        {
            GameObject maze = GameObject.Find("Maze");
            if (maze)
            {
                while (maze.transform.childCount > 0)
                {
                    DestroyImmediate(maze.transform.GetChild(0).gameObject);
                }
            }
        }
    }
}