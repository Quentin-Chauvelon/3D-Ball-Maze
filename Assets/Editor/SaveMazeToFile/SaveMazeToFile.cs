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

            // Move the maze to the origin because rotating the maze will do so around the origin, so we need the maze to be centered around (0, 0, 0)
            MoveMazeToOrigin();

            Level level = new Level();

            level.id = _levelId.value;
            level.name = _name.value;
            level.description = _description.value;
            level.difficulty = (DailyLevelDifficulty)_difficulty.value;
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

            // Absolutely positionnable obstacles
            // These obstacles have to be computed first as they are used by other obstacles (walls, corners...)
            level.obstacles = new Obstacle[_maze.transform.Find("AbsolutelyPositionnableObstacles").childCount + _maze.transform.Find("RelativelyPositionnableObstacles").childCount];
            for (int i = 0; i < _maze.transform.Find("AbsolutelyPositionnableObstacles").childCount; i++)
            {
                Transform obstacleGameObject = _maze.transform.Find("AbsolutelyPositionnableObstacles").GetChild(i);

                Obstacle obstacle = null;
                if (obstacleGameObject.name.Contains("CorneredRail"))
                {
                    obstacle = new CorneredRail(Id, obstacleGameObject.position, (CardinalDirection)(obstacleGameObject.rotation.eulerAngles.y % 360 / 90));
                }
                else if (obstacleGameObject.name.Contains("Rail"))
                {
                    obstacle = new Rail(Id, obstacleGameObject.position, (CardinalDirection)(obstacleGameObject.rotation.eulerAngles.y % 360 / 90));
                }
                else if (obstacleGameObject.name.Contains("HalfSphere"))
                {
                    obstacle = new HalfSphere(Id, obstacleGameObject.position);
                }
                else if (obstacleGameObject.name.Contains("HalfCylinder"))
                {
                    obstacle = new HalfCylinder(Id, obstacleGameObject.position, (CardinalDirection)(obstacleGameObject.rotation.eulerAngles.y % 360 / 90));
                }
                else if (obstacleGameObject.name.Contains("Tunnel"))
                {
                    TunnelType tunnelType;
                    if (obstacleGameObject.name.Contains("2WayTunnel"))
                    {
                        tunnelType = TunnelType.TwoWay;
                    }
                    else if (obstacleGameObject.name.Contains("2WayCornerTunnel"))
                    {
                        tunnelType = TunnelType.TwoWayCornered;
                    }
                    else if (obstacleGameObject.name.Contains("3WayTunnel"))
                    {
                        tunnelType = TunnelType.ThreeWay;
                    }
                    else
                    {
                        tunnelType = TunnelType.FourWay;
                    }

                    obstacle = new Tunnel(Id, tunnelType, obstacleGameObject.position, (CardinalDirection)(obstacleGameObject.rotation.eulerAngles.y % 360 / 90));
                }
                else if (obstacleGameObject.name.Contains("Spikes"))
                {
                    obstacle = new Spikes(Id, GetObstacleIdUnderObstacle(obstacleGameObject));
                }
                else if (obstacleGameObject.name.Contains("Wedge"))
                {
                    obstacle = new Wedge(Id, obstacleGameObject.position, (CardinalDirection)(obstacleGameObject.rotation.eulerAngles.y % 360 / 90), (int)obstacleGameObject.localScale.z);
                }
                else if (obstacleGameObject.name.Contains("FloorHole"))
                {
                    obstacle = new FloorHole(Id, obstacleGameObject.position);
                }

                level.obstacles[i] = obstacle;

                if (obstacle != null)
                {
                    _obstacles[obstacle.id] = obstacle;
                }
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

            // Relatively positionnable obstacles
            for (int i = 0; i < _maze.transform.Find("RelativelyPositionnableObstacles").childCount; i++)
            {

            }

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

            if (!_maze.transform.Find("FlagTargets"))
            {
                new GameObject("FlagTargets").transform.SetParent(_maze.transform);
            }

            if (!_maze.transform.Find("Floors"))
            {
                new GameObject("Floors").transform.SetParent(_maze.transform);
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
        /// Move the maze to the origin.
        /// The maze needs to be centered around (0, 0, 0) because rotating the maze will do so around the origin and not the center of the maze.
        /// </summary>
        private void MoveMazeToOrigin()
        {
            // Include all renderers to get the total size of the maze in order to get the center
            Renderer[] renderers = _maze.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // Invert the x and z axis otherwise it moves in the wrong direction
            bounds.center = new Vector3(-bounds.center.x, bounds.center.y, -bounds.center.z);

            // Since some obstacles are taller than others, the y bounds is not reliable.
            // Thus, we set the center to the first floor's y position, or 0 if there are no floors
            if (_maze.transform.Find("Floors").childCount > 0)
            {
                bounds.center = new Vector3(bounds.center.x, _maze.transform.Find("Floors").GetChild(0).transform.position.y, bounds.center.z);
            }
            else
            {
                bounds.center = new Vector3(bounds.center.x, 0, bounds.center.z);
            }

            // Offset all obstacles by the bounds center value so that the maze is centered around (0, 0, 0)
            _maze.transform.Find("Start").position += bounds.center;
            foreach (Transform child in _maze.transform)
            {
                foreach (Transform obstacle in child)
                {
                    obstacle.position += bounds.center;
                }
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