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
        Lost
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

        // Start is called before the first frame update
        void Start()
        {
            _gameState = GameState.Loading;
            _controls = GetComponent<Controls>();

            LEVELS_PATH = Path.Combine(Application.persistentDataPath, "levels");

            if (!Directory.Exists(LEVELS_PATH))
            {
                Directory.CreateDirectory(LEVELS_PATH);
            }

            if (!GameObject.Find("Maze") || !GameObject.Find("Ball"))
            {
                ExceptionManager.Instance.ShowExceptionMessage("Sorry, there seems to have been a problem loading the level. Please try again", ExceptionManager.ExceptionAction.BackToLevels);
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
            //TODO: Read the file from the server to get information about the level and load it.
            if (levelToLoad != "")
            {
                _maze.BuildMaze(levelType, levelId);
            }

            ResetLevel();

            // Game loaded, waiting to start
            _gameState = GameState.WaitingToStart;
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
    }
}