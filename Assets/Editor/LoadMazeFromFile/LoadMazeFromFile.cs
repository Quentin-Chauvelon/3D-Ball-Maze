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

        private Dictionary<GameObject, Obstacle> _obstacles = new Dictionary<GameObject, Obstacle>();


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
            _obstacles.Clear();

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
            GameObject absolutelyPositionnableObstaclesContainer = new GameObject("AbsolutelyPositionnableObstacles");
            GameObject relativelyPositionnableObstaclesContainer = new GameObject("RelativelyPositionnableObstacles");
            flagTargetsContainer.transform.SetParent(maze.transform);
            floorsContainer.transform.SetParent(maze.transform);
            wallsContainer.transform.SetParent(maze.transform);
            cornersContainer.transform.SetParent(maze.transform);
            absolutelyPositionnableObstaclesContainer.transform.SetParent(maze.transform);
            relativelyPositionnableObstaclesContainer.transform.SetParent(maze.transform);

            GameObject start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(maze.transform);

            // Floor tiles
            foreach (Floor floor in level.floors)
            {
                GameObject floorGameObject = floor.Render(_obstacles);
                floorGameObject.transform.SetParent(floorsContainer.transform);
                _obstacles[floorGameObject] = floor;
            }


            // Walls
            foreach (Wall wall in level.walls)
            {
                wall.Render(_obstacles).transform.SetParent(wallsContainer.transform);
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                corner.Render(_obstacles).transform.SetParent(cornersContainer.transform);
            }

            // Target
            level.target.Render(_obstacles).transform.SetParent(flagTargetsContainer.transform);

            stopwatch.Stop();
            Debug.Log("Loading: Done (" + stopwatch.ElapsedMilliseconds.ToString() + "ms)");
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