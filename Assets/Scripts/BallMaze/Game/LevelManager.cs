using System;
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


    public class LevelManager : MonoBehaviour
    {
        public static int levelToLoad = 0;

        private GameState _gameState;

        private Controls _controls;
        private Maze _maze;
        private Ball _ball;

        // Start is called before the first frame update
        void Start()
        {
            _gameState = GameState.Loading;
            _controls = GetComponent<Controls>();

            if (!GameObject.Find("Maze") || !GameObject.Find("Ball"))
            {
                //TODO: show error message saying the level could not be loaded, please try again and a button to go back to the main menu?
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
                        Debug.Log("started");

                        StartGame();
                    }

                    break;

                case GameState.Playing:
                    break;
            }

        }


        /// <summary>
        /// Loads the the given level.
        /// </summary>
        /// <param name="level">The level to load</param>
        private void LoadLevel(int level)
        {
            //TODO: Read the file from the server to get information about the level and load it.

            _maze.BuildMaze(level);

            ResetLevel();

            // Game loaded, waiting to start
            _gameState = GameState.WaitingToStart;
        }


        private void StartGame()
        {
            _gameState = GameState.Playing;

            ResumeLevel();
        }


        private void ResumeLevel()
        {
            _ball.FreezeBall(false);
        }


        private void PauseLevel()
        {
            _ball.FreezeBall(true);
        }


        private void StartLevel()
        {
            ResumeLevel();
        }


        private void ResetLevel()
        {
            PauseLevel();
        }
    }
}