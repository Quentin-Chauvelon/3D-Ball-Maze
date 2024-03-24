using System;
using BallMaze.Events;
using System.ComponentModel;
using System.IO;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using BallMaze.Obstacles;


namespace BallMaze
{
    /// <summary>
    /// The possible states of the level.
    /// </summary>
    public enum LevelState
    {
        Loading,
        WaitingToStart,
        Playing,
        Paused,
        Won,
        Lost,
        Error
    }


    public enum LevelType
    {
        Default,
        DailyLevel,
        RankedLevel,
        UserCreated
    }


    public enum DailyLevelDifficulty
    {
        Unknown,
        VeryEasy,
        Easy,
        Medium,
        Hard,
        Extreme
    }


    public class LevelException : Exception
    {
        public LevelException() { }
        public LevelException(string message) : base(message) { }
        public LevelException(string message, Exception inner) : base(message, inner) { }
    }


    public class LevelManager : MonoBehaviour
    {
        // Singleton pattern
        private static LevelManager _instance;
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("LevelManager is null!");
                }
                return _instance;
            }
        }

        public static string levelToLoad = "";
        public static LevelType levelType = LevelType.Default;
        public static string LEVELS_PATH = "";
        public const string DEFAULT_LEVELS_FILE_NAME = "defaultLevels.json";
        public const string DEFAULT_LEVELS_SELECTION_FILE_NAME = "defaultLevelsSelection.json";

        private LevelState _levelState;
        public LevelState LevelState
        {
            get { return _levelState; }
        }

        private Controls _controls;

        private Maze _maze;
        public Maze Maze
        {
            get { return _maze; }
        }

        private Ball _ball;
        private CameraManager _camera;

        private GameObject _lastRespawnableObstacle;

        [SerializeField]
        private Config _config;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }


        // Start is called before the first frame update
        void Start()
        {
            _levelState = LevelState.Loading;
            _controls = GetComponent<Controls>();

            // Get the path to the levels folder
            LEVELS_PATH = Path.Combine(Application.persistentDataPath, "levels");

            // If the levels folder doesn't exist, create it
            if (!Directory.Exists(LEVELS_PATH))
            {
                Directory.CreateDirectory(LEVELS_PATH);
            }

            // If the maze or ball gameobjects don't exist, throw an error
            try
            {
                if (!GameObject.Find("Maze") || !GameObject.Find("Ball"))
                {
                    _levelState = LevelState.Error;
                    throw new LevelException($"Maze or Ball not found.\nMaze = {GameObject.Find("Maze")},\nBall = {GameObject.Find("Ball")}");
                }
            }
            catch (LevelException e)
            {
                ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "LevelLoadingTryAgainGenericError", ExceptionActionType.RestartGame);
                return;
            }

            _maze = GameObject.Find("Maze").GetComponent<Maze>();
            _ball = GameObject.Find("Ball").GetComponent<Ball>();
            _camera = Camera.main.GetComponent<CameraManager>();

            if (_config.setLevelToLoad)
            {
                levelToLoad = _config.levelToLoad;
            }

            SettingsEvents.UpdatedStartMethod += ResetLevel;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_levelState)
            {
                case LevelState.WaitingToStart:
                    // If the player has start on touch enabled, is not over a UI element and clicks or touches the screen, start the game
                    if (SettingsManager.Instance.startOn == StartOnSettings.Touch &&
                        !EventSystem.current.currentSelectedGameObject &&
                        (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
                    {
                        StartLevel();
                    }

                    break;

                case LevelState.Playing:
                    _maze.UpdateMazeOrientation(_controls.GetControlsOrientation());
                    _ball.AddForce(_controls.GetRawControlsDirection());

                    break;
            }
        }


        /// <summary>
        /// Loads the the given level.
        /// </summary>
        /// <param name="level">The level to load</param>
        public void LoadLevel(string levelId)
        {
            if (levelToLoad != "")
            {
                // If a maze is already loaded, clear it
                if (_maze.IsMazeLoaded())
                {
                    ClearMaze();
                }

                bool result = _maze.BuildMaze(levelType, levelId);
                if (!result)
                {
                    // No need to display the error message as it is already handled in the LevelLoader class
                    _levelState = LevelState.Error;
                    return;
                }

                // Fit the maze in the camera's perspective
                _camera.FitMazeInPerspective(_maze.GetMazeBounds());

                ListenToTargetTrigger();
            }

            ResetLevel();

            // Game loaded, waiting to start
            _levelState = LevelState.WaitingToStart;

            // Have to show the controls here because otherwise if the player uses TouchToStart
            // The first touch will both start the game and register as a touch on the joystick
            // And so the joystick will work but be invisible until the player releases and touches the screen again
            _controls.DisableAndShowControls();
        }


        /// <summary>
        /// Listen to all triggers actions
        /// </summary>
        private void ListenToTargetTrigger()
        {
            MazeEvents.targetReached += TargetReached;
        }


        private void ResumeLevel()
        {
            _controls.EnableAndShowControls();

            _ball.SetBallVisible(true);
            _ball.FreezeBall(false);

            _levelState = LevelState.Playing;
        }


        public void PauseLevel()
        {
            _controls.DisableAndHideControls();

            _ball.FreezeBall(true);

            _levelState = LevelState.Paused;
        }


        private void StartLevel()
        {
            ResumeLevel();
        }


        public void ResetLevel()
        {
            _levelState = LevelState.WaitingToStart;

            _controls.DisableAndShowControls();

            _lastRespawnableObstacle = null;

            _ball.SetBallVisible(true);
            _ball.FreezeBall(true);
            // Move the ball to the start position
            if (_maze.start)
            {
                _ball.MoveBallToPosition(_maze.start.transform.TransformPoint(transform.position));
            }

            _maze.ResetMazeOrientation();
        }


        /// <summary>
        /// Player reached the target and won the level.
        /// Binded to the target's triggerAction, and so called when the ball enters a target's trigger.
        /// </summary>
        private void TargetReached()
        {
            if (_levelState == LevelState.Playing)
            {
                PauseLevel();

                _levelState = LevelState.Won;
            }
        }


        /// <summary>
        /// Player lost the level. Either by falling off the maze or by hitting a killing obstacle.
        /// </summary>
        private void Lost()
        {
            if (_levelState == LevelState.Playing)
            {
                PauseLevel();

                _levelState = LevelState.Lost;
            }
        }


        /// <summary>
        /// Destroys all the maze's children and unbinds all actions (targets triggers...).
        /// </summary>
        private void ClearMaze()
        {
            MazeEvents.targetReached -= TargetReached;

            _maze.ClearMaze();
        }


        /// <summary>
        /// Get the obstacle from the GameObject the ball collided with.
        /// The GameObject can be the obstacle itself or one of its parents :
        /// If the GameObject is a primitive obstacle (eg: floor), the obstacle is the GameObject itself.
        /// If the GameObject is a prefab or a mesh (eg: rail), the obstacle is the GameObject's parent.
        /// If the GameObject is a prefab containing a mesh (eg: flag target), the obstacle is the GameObject's parent's parent.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public GameObject GetObstacleGameObjectFromBallCollision(GameObject gameObject)
        {
            if (gameObject.transform.parent.parent.name == "Maze")
            {
                return gameObject;
            }
            else if (gameObject.transform.parent.parent.parent.name == "Maze")
            {
                return gameObject.transform.parent.gameObject;
            }
            else if (gameObject.transform.parent.parent.parent.parent.name == "Maze")
            {
                return gameObject.transform.parent.parent.gameObject;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Quit the level and resets everything.
        /// </summary>
        public void QuitLevel()
        {
            PauseLevel();

            _lastRespawnableObstacle = null;

            _ball.SetBallVisible(false);
            _ball.FreezeBall(true);

            _maze.ResetMazeOrientation();

            ClearMaze();
        }


        private void OnDestroy()
        {
            QuitLevel();
        }
    }
}