using BallMaze.Events;
using System.ComponentModel;
using System.IO;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;


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


    public class LevelManager : MonoBehaviour
    {
        public static string levelToLoad = "";
        public static LevelType levelType = LevelType.Default;
        public static string LEVELS_PATH = "";
        public const string DEFAULT_LEVELS_FILE_NAME = "defaultLevels.json";
        public const string DEFAULT_LEVELS_SELECTION_FILE_NAME = "defaultLevelsSelection.json";

        private LevelState _levelState;

        private Controls _controls;
        private Maze _maze;
        private Ball _ball;
        private CameraManager _camera;

        [SerializeField]
        private Config _config;

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

            // If the maze or ball gameobjects don't exist, there is an error
            if (!GameObject.Find("Maze") || !GameObject.Find("Ball"))
            {
                _levelState = LevelState.Error;
                ExceptionManager.ShowExceptionMessage("ExceptionMessagesTable", "LevelLoadingTryAgainGenericError", ExceptionAction.BackToLevels);
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


        private void PauseLevel()
        {
            _controls.DisableAndHideControls();

            _ball.FreezeBall(true);

            _levelState = LevelState.Paused;
        }


        private void StartLevel()
        {
            ResumeLevel();
        }

        
        private void ResetLevel()
        {
            _levelState = LevelState.WaitingToStart;

            _controls.DisableAndShowControls();

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
        /// Quit the level and resets everything.
        /// </summary>
        public void QuitLevel()
        {
            PauseLevel();

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