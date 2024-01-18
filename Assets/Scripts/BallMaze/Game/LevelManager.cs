using BallMaze.Events;
using System.IO;
using TMPro.EditorUtilities;
using UnityEngine;


namespace BallMaze
{
    /// <summary>
    /// The possible states of the game.
    /// </summary>
    public enum GameState
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
        UserCreated
    }


    public class LevelManager : MonoBehaviour
    {
        public static string levelToLoad = "";
        public static LevelType levelType = LevelType.Default;
        public static string LEVELS_PATH = "";

        private GameState _gameState;

        private Controls _controls;
        private Maze _maze;
        private Ball _ball;
        private CameraManager _camera;

        [SerializeField]
        private Config _config;

        // Start is called before the first frame update
        void Start()
        {
            _gameState = GameState.Loading;
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
                _gameState = GameState.Error;
                ExceptionManager.Instance.ShowExceptionMessage("Sorry, there seems to have been a problem loading the level. Please try again", ExceptionManager.ExceptionAction.BackToLevels);
                return;
            }

            _maze = GameObject.Find("Maze").GetComponent<Maze>();
            _ball = GameObject.Find("Ball").GetComponent<Ball>();
            _camera = Camera.main.GetComponent<CameraManager>();

            if (_config.setLevelToLoad)
            {
                levelToLoad = _config.levelToLoad;
            }

            LoadLevel(levelToLoad);

            // Restart the level if the player updates a setting linked to the start method (start on, cooldown duration...)
            SettingsEvents.UpdatedStartMethod += ResetLevel;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_gameState)
            {
                case GameState.WaitingToStart:
                    // If the player has start on touch enabled and the player clicks or touches the screen, start the game
                    if (SettingsManager.Instance.startOn == StartOnSettings.Touch &&
                        (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
                    {
                        StartLevel();
                    }

                    break;

                case GameState.Playing:
                    _maze.UpdateMazeOrientation(_controls.GetControlsOrientation());
                    _ball.AddForce(_controls.GetRawControlsDirection());

                    break;
            }
        }


        /// <summary>
        /// Loads the the given level.
        /// </summary>
        /// <param name="level">The level to load</param>
        private void LoadLevel(string levelId)
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
                    _gameState = GameState.Error;
                    return;
                }

                // Fit the maze in the camera's perspective
                _camera.FitMazeInPerspective(_maze.GetMazeBounds());

                ListenToTargetTrigger();
            }

            ResetLevel();

            // Game loaded, waiting to start
            _gameState = GameState.WaitingToStart;

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
            // Bind the target's triggerAction to the TargetReached method
            foreach (GameObject target in _maze.targets)
            {
                // Observer pattern
                target.transform.Find("Trigger").GetComponent<Obstacles.Target>().triggerAction += TargetReached;
            }
        }


        private void ResumeLevel()
        {
            _controls.EnableAndShowControls();

            _ball.SetBallVisible(true);
            _ball.FreezeBall(false);

            _gameState = GameState.Playing;
        }


        private void PauseLevel()
        {
            _controls.DisableAndHideControls();

            _ball.FreezeBall(true);

            _gameState = GameState.Paused;
        }


        private void StartLevel()
        {
            ResumeLevel();
        }

        
        private void ResetLevel()
        {
            _gameState = GameState.WaitingToStart;

            _controls.DisableAndShowControls();

            _ball.SetBallVisible(true);
            _ball.FreezeBall(true);
            // Move the ball to the start position
            _ball.MoveBallToPosition(_maze.start.transform.TransformPoint(transform.position));

            _maze.ResetMazeOrientation();
        }


        /// <summary>
        /// Player reached the target and won the level.
        /// Binded to the target's triggerAction, and so called when the ball enters a target's trigger.
        /// </summary>
        private void TargetReached()
        {
            if (_gameState == GameState.Playing)
            {
                PauseLevel();

                _gameState = GameState.Won;
            }
        }


        /// <summary>
        /// Player lost the level. Either by falling off the maze or by hitting a killing obstacle.
        /// </summary>
        private void Lost()
        {
            if (_gameState == GameState.Playing)
            {
                PauseLevel();

                _gameState = GameState.Lost;
            }
        }


        /// <summary>
        /// Destroys all the maze's children and unbinds all actions (targets triggers...).
        /// </summary>
        private void ClearMaze()
        {
            // Unbind the target's triggerAction to the TargetReached method
            foreach (GameObject target in _maze.targets)
            {
                target.transform.Find("Trigger").GetComponent<Obstacles.Target>().triggerAction -= TargetReached;
            }

            _maze.ClearMaze();
        }


        /// <summary>
        /// Quit the level and resets everything.
        /// </summary>
        private void QuitLevel()
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