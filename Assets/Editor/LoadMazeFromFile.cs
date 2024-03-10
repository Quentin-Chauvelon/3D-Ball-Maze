using BallMaze;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadMazeFromFile : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset _visualTreeAsset = default;
    private TextField _fileName;
    private TextField _id;

    private Button _loadButton;


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

        GameObject targetsContainer = new GameObject("Targets");
        GameObject floorTilesContainer = new GameObject("FloorTiles");
        GameObject wallsContainer = new GameObject("Walls");
        GameObject cornersContainer = new GameObject("Corners");
        GameObject obstaclesContainer = new GameObject("Obstacles");
        targetsContainer.transform.SetParent(maze.transform);
        floorTilesContainer.transform.SetParent(maze.transform);
        wallsContainer.transform.SetParent(maze.transform);
        cornersContainer.transform.SetParent(maze.transform);
        obstaclesContainer.transform.SetParent(maze.transform);

        GameObject start = new GameObject("Start");
        start.transform.position = level.startPosition;
        start.transform.SetParent(maze.transform);

        // Create all the targets
        foreach (Target target in level.targets)
        {
            GameObject targetObject = (GameObject)Instantiate(Resources.Load("Level/Targets/Target" + target.id.ToString()));
            targetObject.name = "Target";
            targetObject.transform.position = target.p;
            targetObject.transform.rotation = Quaternion.Euler(target.r);
            targetObject.transform.SetParent(targetsContainer.transform);
        }

        // Create all the floor tiles
        foreach (FloorTile floorTile in level.floorTiles)
        {
            // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after running some tests
            // It appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
            GameObject floorTileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorTileObject.name = "Floor";
            floorTileObject.transform.localScale = new Vector3(1, 0.1f, 1);
            floorTileObject.transform.position = floorTile.p;
            floorTileObject.transform.SetParent(floorTilesContainer.transform);
        }

        // Create all the walls
        foreach (Wall wall in level.walls)
        {
            GameObject wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObject.name = "Wall";

            // Adapt the wall's scale to the direction it's facing (equivalent to rotating the wall)
            if (wall.d == Direction.North || wall.d == Direction.South)
            {
                wallObject.transform.localScale = new Vector3(1, 0.5f, 0.1f);
            }
            else
            {
                wallObject.transform.localScale = new Vector3(0.1f, 0.5f, 1);
            }

            // Find the floor tile the wall is on
            foreach (FloorTile floorTile in level.floorTiles)
            {
                if (wall.id == floorTile.id)
                {
                    wallObject.transform.position = floorTile.p + new Vector3(0, 0.2f, 0);
                    break;
                }
            }

            // Move the wall to the correct position based on the direction it's facing
            switch (wall.d)
            {
                case Direction.North:
                    wallObject.transform.position -= new Vector3(0, 0, 0.45f);
                    break;
                case Direction.East:
                    wallObject.transform.position -= new Vector3(0.45f, 0, 0);
                    break;
                case Direction.South:
                    wallObject.transform.position += new Vector3(0, 0, 0.45f);
                    break;
                case Direction.West:
                    wallObject.transform.position += new Vector3(0.45f, 0, 0);
                    break;
            }

            wallObject.transform.SetParent(wallsContainer.transform);
        }

        // Create all the corners
        foreach (Corner corner in level.corners)
        {
            GameObject cornerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cornerObject.name = "Corner";
            cornerObject.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

            // Find the floor tile the corner is on
            foreach (FloorTile floorTile in level.floorTiles)
            {
                if (corner.id == floorTile.id)
                {
                    cornerObject.transform.position = floorTile.p + new Vector3(0, 0.2f, 0);
                    break;
                }
            }

            // Move the corner to the correct position based on the direction it's facing
            switch (corner.d)
            {
                case Direction.NorthEast:
                    cornerObject.transform.position -= new Vector3(0.45f, 0, 0.45f);
                    break;
                case Direction.SouthEast:
                    cornerObject.transform.position += new Vector3(-0.45f, 0, 0.45f);
                    break;
                case Direction.SouthWest:
                    cornerObject.transform.position += new Vector3(0.45f, 0, 0.45f);
                    break;
                case Direction.NorthWest:
                    cornerObject.transform.position += new Vector3(0.45f, 0, -0.45f);
                    break;
            }

            cornerObject.transform.SetParent(cornersContainer.transform);
        }

        stopwatch.Stop();
        Debug.Log("Loading: Done (" + stopwatch.ElapsedMilliseconds.ToString() + "ms)");
    }
}
