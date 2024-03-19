using BallMaze;
using System;
using System.IO;
using UnityExtensionMethods;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using BallMaze.Obstacles;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace BallMaze.Editor
{
    public class SaveMazeToFile : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset = default;
        private TextField _fileName;
        private TextField _levelId;
        private TextField _name;
        private TextField _description;
        private EnumField _difficulty;
        private EnumField _levelType;

        private TextField _star1Time;
        private TextField _star2Time;
        private TextField _star3Time;

        private Button _saveButton;

        private Button _instantiateMazeObjectButton;

        private GameObject _maze;

        private int _id = 0;
        private int Id
        {
            get => _id++;
            set => _id = value;
        }

        private Dictionary<int, Obstacle> _obstacles = new Dictionary<int, Obstacle>();


        [MenuItem("Utilities/Save Maze To File", false, 20)]
        public static void OpenSaveMazeWindow()
        {
            SaveMazeToFile saveMazeWindow = GetWindow<SaveMazeToFile>();
            saveMazeWindow.titleContent = new GUIContent("Save Maze To File");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SaveMazeToFile/SaveMazeToFile.uxml");
            VisualElement saveMazeToFileUXML = _visualTreeAsset.Instantiate();
            root.Add(saveMazeToFileUXML);

            _fileName = saveMazeToFileUXML.Query<TextField>("FileName");
            _levelId = saveMazeToFileUXML.Query<TextField>("Id");
            _name = saveMazeToFileUXML.Query<TextField>("Name");
            _description = saveMazeToFileUXML.Query<TextField>("Description");
            _difficulty = saveMazeToFileUXML.Query<EnumField>("Difficulty");
            _levelType = saveMazeToFileUXML.Query<EnumField>("LevelType");

            _star1Time = saveMazeToFileUXML.Query<TextField>("Star1Time");
            _star2Time = saveMazeToFileUXML.Query<TextField>("Star2Time");
            _star3Time = saveMazeToFileUXML.Query<TextField>("Star3Time");

            _saveButton = saveMazeToFileUXML.Query<Button>("SaveButton");
            _saveButton.clicked += SaveMaze;

            _instantiateMazeObjectButton = saveMazeToFileUXML.Query<Button>("InstantiateMazeObjectButton");
            _instantiateMazeObjectButton.clicked += InstantiateMazeObject;
        }


        /// <summary>
        /// Saves the current maze to a file.
        /// </summary>
        private void SaveMaze()
        {
            Debug.Log("Saving: Started for file: " + _fileName.value);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            Id = 0;

            _maze = GameObject.Find("Maze");
            Level level = new Level();

            level.id = _levelId.value;
            level.name = _name.value;
            level.description = _description.value;
            level.difficulty = (Difficulty)_difficulty.value;
            level.levelType = (LevelType)_levelType.value;

            level.startPosition = _maze.transform.Find("Start").position;

            // Floor tiles
            level.floors = new Floor[_maze.transform.Find("Floors").childCount];
            for (int i = 0; i < _maze.transform.Find("Floors").childCount; i++)
            {
                Transform floorGameObject = _maze.transform.Find("Floors").GetChild(i);

                // Create the floor object that will be serialized from the GameObject
                Floor floor = new Floor(Id, floorGameObject.position);

                // Add the floor to the level structure
                level.floors[i] = floor;

                // Add the floor to the obstacles dictionary, this is done so that we can find the id of this obstacle
                // if there is another obstacle on top of it (e.g. a wall).
                _obstacles[floor.id] = floor;
            }

            // Walls
            level.walls = new Wall[_maze.transform.Find("Walls").childCount];
            for (int i = 0; i < _maze.transform.Find("Walls").childCount; i++)
            {
                Transform wallGameObject = _maze.transform.Find("Walls").GetChild(i);

                Wall wall = new Wall(Id, GetObstacleIdUnderObstacle(wallGameObject), GetObstacleCardinalDirection(ObstacleType.Wall, wallGameObject));

                level.walls[i] = wall;

                // No need to add the wall to the obstacles dictionary as no other obstacle can be on top of it.
                // And so, its id will never be needed by other obstacles.
            }

            // Corners
            level.corners = new Corner[_maze.transform.Find("Corners").childCount];
            for (int i = 0; i < _maze.transform.Find("Corners").childCount; i++)
            {
                Transform cornerGameObject = _maze.transform.Find("Corners").GetChild(i);

                Corner corner = new Corner(Id, GetObstacleIdUnderObstacle(cornerGameObject), GetObstacleCardinalDirection(ObstacleType.Corner, cornerGameObject));

                level.corners[i] = corner;
            }


            // Target
            Transform targetGameObject = _maze.transform.Find("FlagTargets").GetChild(0);
            FlagTarget flagTarget = new FlagTarget(Id, GetObstacleIdUnderObstacle(targetGameObject), GetObstacleCardinalDirection(ObstacleType.FlagTarget, targetGameObject));
            level.target = flagTarget;

            // Times
            level.times = new float[3];
            level.times[0] = float.Parse(_star1Time.value);
            level.times[1] = float.Parse(_star2Time.value);
            level.times[2] = float.Parse(_star3Time.value);

            // Serialize a compact version of the level and a pretty version
            string serializedJsonData = JsonConvert.SerializeObject(level);
            string serializedJsonDataPretty = JsonConvert.SerializeObject(level, Formatting.Indented);

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "levels", _fileName.value + ".json"), serializedJsonData);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "levels", _fileName.value + "_pretty.json"), serializedJsonDataPretty);

            stopwatch.Stop();
            Debug.Log("Saving: Done (" + stopwatch.ElapsedMilliseconds.ToString() + "ms)");
        }


        private void InstantiateMazeObject()
        {
            _maze = GameObject.Find("Maze");
            if (!_maze)
            {
                _maze = new GameObject("Maze");
            }

            if (!_maze.transform.Find("Start"))
            {

                new GameObject("Start").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("Targets"))
            {
                new GameObject("Targets").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("FloorTiles"))
            {
                new GameObject("FloorTiles").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("Walls"))
            {
                new GameObject("Walls").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("Corners"))
            {
                new GameObject("Corners").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("AbsolutelyPositionnableObstacles"))
            {
                new GameObject("AbsolutelyPositionnableObstacles").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("RelativelyPositionnableObstacles"))
            {
                new GameObject("RelativelyPositionnableObstacles").transform.SetParent(_maze.transform);
            }
        }


        /// <summary>
        /// Gets the obstacle under the given obstacle
        /// </summary>
        private int GetObstacleIdUnderObstacle(Transform transform)
        {
            // Round the position to match the position of the floor (because a floor is centered on its 1x1 tile
            // while some obstacles like wall are 1x0.2 and on the edge of the tile)
            Vector3 roundedPosition = transform.position.Round();

            foreach (KeyValuePair<int, Obstacle> obstacleEntry in _obstacles)
            {
                if (obstacleEntry.Value is IAbsolutelyPositionnable && roundedPosition == ((IAbsolutelyPositionnable)obstacleEntry.Value).position)
                {
                    return obstacleEntry.Key;
                }
            }

            Debug.LogWarning($"No obstacle found under {transform.name}!");
            return 0;
        }


        private CardinalDirection GetObstacleCardinalDirection(ObstacleType obstacleType, Transform transform)
        {
            switch (obstacleType)
            {
                case ObstacleType.Wall:
                    // If the x position doesn't have a decimal part, the wall is either facing north or south
                    if ((transform.position.x - Math.Truncate(transform.position.x)).AlmostEquals(0, 0.05))
                    {
                        // If the z position is 0.45 less than the rounded value (position of the floor), the wall is facing south, otherwise it is facing north (+0.45)
                        return (Mathf.Round(transform.position.z) - transform.position.z).AlmostEquals(0.45f)
                            ? CardinalDirection.North
                            : CardinalDirection.South;
                    }
                    // If the x position doesn't have a decimal part, the wall is either facing west or east
                    else
                    {
                        // If the x position is 0.45 less than the rounded value (position of the floor), the wall is facing east, otherwise it is facing west (+0.45)
                        return (Mathf.Round(transform.position.x) - transform.position.x).AlmostEquals(0.45f)
                            ? CardinalDirection.East
                            : CardinalDirection.West;
                    }

                case ObstacleType.Corner:
                    // If the z position is 0.45 less than the rounded value (position of the floor), the corner is facing north, otherwise it is facing south (+0.45)
                    if ((Mathf.Round(transform.position.z) - transform.position.z).AlmostEquals(0.45f))
                    {
                        // If the x position is 0.45 less than the rounded value (position of the floor), the corner is facing north-east, otherwise it is facing north-west (+0.45)
                        return (Mathf.Round(transform.position.x) - transform.position.x).AlmostEquals(0.45f)
                            ? CardinalDirection.North
                            : CardinalDirection.West;
                    }
                    else
                    {
                        // If the x position is 0.45 less than the rounded value (position of the floor), the corner is facing south-east, otherwise it is facing south-west (+0.45)
                        return (Mathf.Round(transform.position.x) - transform.position.x).AlmostEquals(0.45f)
                            ? CardinalDirection.East
                            : CardinalDirection.South;
                    }

                default:
                    Debug.LogWarning($"Unable to find direction for obstacle {transform.name} (type: {obstacleType})");
                    return CardinalDirection.North;
            }
        }
    }
}