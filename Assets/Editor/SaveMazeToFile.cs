using BallMaze;
using System;
using System.IO;
using UnityExtensionMethods;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace AssetsEditor
{
    public class SaveMazeToFile : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _visualTreeAsset = default;
        private TextField _fileName;
        private TextField _id;
        private EnumField _difficulty;
        private EnumField _levelType;

        private TextField _star1Time;
        private TextField _star2Time;
        private TextField _star3Time;

        private Button _saveButton;

        private GameObject _maze;

        
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
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/SaveMazeToFile.uxml");
            VisualElement saveMazeToFileUXML = _visualTreeAsset.Instantiate();
            root.Add(saveMazeToFileUXML);

            _fileName = saveMazeToFileUXML.Query<TextField>("FileName");
            _id = saveMazeToFileUXML.Query<TextField>("Id");
            _difficulty = saveMazeToFileUXML.Query<EnumField>("Difficulty");
            _levelType = saveMazeToFileUXML.Query<EnumField>("LevelType");

            _star1Time = saveMazeToFileUXML.Query<TextField>("Star1Time");
            _star2Time = saveMazeToFileUXML.Query<TextField>("Star2Time");
            _star3Time = saveMazeToFileUXML.Query<TextField>("Star3Time");

            _saveButton = saveMazeToFileUXML.Query<Button>("SaveButton");
            _saveButton.clicked += SaveMaze;
        }


        /// <summary>
        /// Saves the current maze to a file.
        /// </summary>
        private void SaveMaze()
        {
            Debug.Log("Saving: Started for file: " + _fileName.value);
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            _maze = GameObject.Find("Maze");
            Level level = new Level();

            level.id = _id.value;
            level.difficulty = (Difficulty)_difficulty.value;
            level.levelType = (LevelType)_levelType.value;

            level.startPosition = _maze.transform.Find("Start").position;

            level.targets = new Target[_maze.transform.Find("Targets").childCount];
            foreach (Transform targetObject in _maze.transform.Find("Targets"))
            {
                Target target = new Target();
                target.id = 1;
                target.p = targetObject.position;
                target.r = targetObject.rotation.eulerAngles;

                level.targets[targetObject.GetSiblingIndex()] = target;
            }

            level.floorTiles = new FloorTile[_maze.transform.Find("FloorTiles").childCount];
            foreach (Transform floorTileObject in _maze.transform.Find("FloorTiles"))
            {
                FloorTile floorTile = new FloorTile();
                floorTile.id = floorTileObject.GetSiblingIndex();
                floorTile.p = floorTileObject.position;

                level.floorTiles[floorTileObject.GetSiblingIndex()] = floorTile;
            }

            level.walls = new Wall[_maze.transform.Find("Walls").childCount];
            foreach (Transform wallObject in _maze.transform.Find("Walls"))
            {
                Wall wall = new Wall();
                Vector3 wallRoundedPosition = wallObject.position.Round();

                foreach (Transform floorTileObject in _maze.transform.Find("FloorTiles"))
                {
                    if (wallRoundedPosition == floorTileObject.position)
                    {
                        wall.id = floorTileObject.GetSiblingIndex();
                        break;
                    }
                }

                // If the x position doesn't have a decimal part, the wall is either facing north or south
                if ((wallObject.position.x - Math.Truncate(wallObject.position.x)).DoubleEquals(0, 0.05))
                {
                    // If the z position is 0.45 less than the rounded value (position of the floor), the wall is facing south, otherwise it is facing north (+0.45)
                    if ((Math.Round(wallObject.position.z) - wallObject.position.z).DoubleEquals(0.45, 0.05))
                    {
                        wall.d = Direction.North;
                    }
                    else
                    {
                        wall.d = Direction.South;
                    }
                }
                // If the x position doesn't have a decimal part, the wall is either facing west or east
                else
                {
                    // If the x position is 0.45 less than the rounded value (position of the floor), the wall is facing east, otherwise it is facing west (+0.45)
                    if ((Math.Round(wallObject.position.x) - wallObject.position.x).DoubleEquals(0.45, 0.05))
                    {
                        wall.d = Direction.East;
                    }
                    else
                    {
                        wall.d = Direction.West;
                    }

                }

                level.walls[wallObject.GetSiblingIndex()] = wall;
            }

            level.corners = new Corner[_maze.transform.Find("Corners").childCount];
            foreach (Transform cornerObject in _maze.transform.Find("Corners"))
            {
                Corner corner = new Corner();
                Vector3 cornerRoundedPosition = cornerObject.position.Round();

                foreach (Transform floorTileObject in _maze.transform.Find("FloorTiles"))
                {
                    if (cornerRoundedPosition == floorTileObject.position)
                    {
                        corner.id = floorTileObject.GetSiblingIndex();
                        break;
                    }
                }

                // If the z position is 0.45 less than the rounded value (position of the floor), the corner is facing north, otherwise it is facing south (+0.45)
                if ((Math.Round(cornerObject.position.z) - cornerObject.position.z).DoubleEquals(0.45, 0.05))
                {
                    // If the x position is 0.45 less than the rounded value (position of the floor), the corner is facing north-east, otherwise it is facing north-west (+0.45)
                    if ((Math.Round(cornerObject.position.x) - cornerObject.position.x).DoubleEquals(0.45, 0.05))
                    {
                        corner.d = Direction.NorthEast;
                    }
                    else
                    {
                        corner.d = Direction.NorthWest;
                    }
                }
                else
                {
                    // If the x position is 0.45 less than the rounded value (position of the floor), the corner is facing south-east, otherwise it is facing south-west (+0.45)
                    if ((Math.Round(cornerObject.position.x) - cornerObject.position.x).DoubleEquals(0.45, 0.05))
                    {
                        corner.d = Direction.SouthEast;
                    }
                    else
                    {
                        corner.d = Direction.SouthWest;
                    }
                }

                level.corners[cornerObject.GetSiblingIndex()] = corner;
            }

            level.times = new float[3];
            level.times[0] = float.Parse(_star1Time.value);
            level.times[1] = float.Parse(_star2Time.value);
            level.times[2] = float.Parse(_star3Time.value);

            string serializedJsonData = JsonUtility.ToJson(level, false);
            string serializedJsonDataPretty = JsonUtility.ToJson(level, true);

            File.WriteAllText(Path.Combine(Application.persistentDataPath, "levels", _fileName.value + ".json"), serializedJsonData);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "levels", _fileName.value + "_pretty.json"), serializedJsonDataPretty);

            stopwatch.Stop();
            Debug.Log("Saving: Done (" + stopwatch.ElapsedMilliseconds.ToString() + "ms)");
        }
    }
}