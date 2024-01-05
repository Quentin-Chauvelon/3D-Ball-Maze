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
        public static string levelToLoad = "1";
        public static LevelType levelType = LevelType.Default;
        public static string LEVELS_PATH = "";

        private GameState _gameState;

        private Controls _controls;
        private Maze _maze;
        private Ball _ball;

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

            LoadLevel(levelToLoad);
        }

        // Update is called once per frame
        void Update()
        {
            switch (_gameState)
            {
                case GameState.WaitingToStart:
                    // If the player has start on touch enabled and the player clicks or touches the screen, start the game
                    if (SettingsManager.Instance.StartOnTouch() &&
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

                ListenToTargetTrigger();
            }

            ResetLevel();

            // Game loaded, waiting to start
            _gameState = GameState.WaitingToStart;
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
            // Unfreeze the ball
            _ball.FreezeBall(false);

            // Enable and show the controls
            _controls.EnableAndShowControls();
        }


        private void PauseLevel()
        {
            // Freeze the ball
            _ball.FreezeBall(true);

            // Disable and hide the controls
            _controls.DisableAndHideControls();
        }


        private void StartLevel()
        {
            _gameState = GameState.Playing;

            ResumeLevel();
        }


        private void ResetLevel()
        {
            PauseLevel();
        }


        /// <summary>
        /// Player reached the target and won the level.
        /// Binded to the target's triggerAction, and so called when the ball enters a target's trigger.
        /// </summary>
        private void TargetReached()
        {
            if (_gameState == GameState.Playing)
            {
                _gameState = GameState.Won;
                ResetLevel();
            }
        }


        private void ClearMaze()
        {
            // Unbind the target's triggerAction to the TargetReached method
            foreach (GameObject target in _maze.targets)
            {
                target.transform.Find("Trigger").GetComponent<Obstacles.Target>().triggerAction -= TargetReached;
            }

            _maze.ClearMaze();
        }


        private void OnDestroy()
        {
            _gameState = GameState.Paused;
            ClearMaze();
        }
    }
}