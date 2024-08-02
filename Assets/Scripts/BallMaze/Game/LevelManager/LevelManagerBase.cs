using System;
using BallMaze.Events;
using BallMaze.Obstacles;
using BallMaze.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallMaze
{
    public abstract class LevelManagerBase
    {
        public abstract LevelType levelType { get; }

        public abstract string LEVELS_PATH { get; }

        protected LevelState _levelState;
        public LevelState LevelState
        {
            get { return _levelState; }
            set { _levelState = value; }
        }

        private Controls _controls;

        private Maze _maze;
        public Maze Maze
        {
            get { return _maze; }
        }

        private Ball _ball;

        private CameraManager _camera;

        protected LevelTimer _levelTimer;

        protected abstract bool _isSecondChanceEnabled { get; }
        private bool _usedSecondChance = false;

        protected string _currentLevelId = "";

        public bool HasLevelStarted { get; private set; } = false;

        private GameObject _lastRespawnableObstacle;

        // The number of milliseconds to wait before displaying the level completed view once the player reaches the target
        public const int TARGET_REACHED_UI_DELAY = 1500;


        public void Start()
        {
            _controls = GameObject.Find("LevelManager").GetComponent<Controls>();

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

            _maze.InitMaze();

            _levelTimer = new LevelTimer();

            SettingsEvents.UpdatedStartMethod += ResetLevel;
        }


        public void Update()
        {
            switch (_levelState)
            {
                case LevelState.WaitingToStart:
                    // If the player has start on touch enabled
                    if (SettingsManager.Instance.startOn == StartOnSettings.Touch)
                    {
                        // If the player is not over a UI element and clicks or touches the screen, start the game
                        if (!EventSystem.current.currentSelectedGameObject &&
                        (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
                        {
                            (UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView).SetTouchToStartLabelVisibility(false);

                            StartLevel();
                        }
                    }
                    // If the player has countdown enabled
                    else if (SettingsManager.Instance.startOn == StartOnSettings.Countdown)
                    {
                        // If the countdown has reached 0, start the game
                        if ((UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView).UpdateCountdown())
                        {
                            StartLevel();
                        }
                    }

                    break;

                case LevelState.Playing:
                    // If the player presses R, restart the level
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        ResetLevel();
                        return;
                    }

                    _maze.UpdateMazeOrientation(_controls.GetControlsOrientation());
                    _ball.AddForce(_controls.GetRawControlsDirection());

                    _levelTimer.Update(Time.deltaTime);

                    break;
            }
        }



        /// <summary>
        /// Loads the the given level.
        /// </>
        /// <param name="level">The level to load</param>
        public virtual void LoadLevel(string levelId)
        {
            if (LevelManager.levelToLoad != "")
            {
                // If a maze is already loaded, clear it
                ClearMaze();
            }

            LevelManager.levelToLoad = levelId;
            _currentLevelId = levelId;

            bool result = _maze.BuildMaze(levelType, levelId);
            if (!result)
            {
                // No need to display the error message as it is already handled in the LevelLoader class
                _levelState = LevelState.Error;
                return;
            }

            Maze.RenderAllObstacles(_maze.obstaclesList, _maze.obstacles, _maze.obstaclesTypesMap);

            // Fit the maze in the camera's perspective
            _camera.FitMazeInPerspective(_maze.GetMazeBounds());

            ListenToTargetTrigger();

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


        public void ResumeLevel()
        {
            _controls.EnableAndShowControls();

            _ball.SetBallVisible(true);
            _ball.FreezeBall(false);

            _levelState = LevelState.Playing;

            _levelTimer.Resume();
        }


        public void PauseLevel()
        {
            _controls.DisableAndHideControls();

            _ball.FreezeBall(true);

            _levelState = LevelState.Paused;

            _levelTimer.Pause();
        }


        private void StartLevel()
        {
            ResumeLevel();

            _levelTimer.Start();

            HasLevelStarted = true;
        }


        public void ResetLevel()
        {
            _levelState = LevelState.WaitingToStart;

            _controls.DisableAndShowControls();

            _lastRespawnableObstacle = null;

            _ball.SetBallVisible(true);
            _ball.FreezeBall(true);

            _usedSecondChance = false;

            _maze.ResetMazeOrientation();

            // Move the ball to the start position
            if (_maze.start != null)
            {
                _ball.MoveBallToPosition(_maze.start.transform.position);
            }

            if (SettingsManager.Instance.startOn == StartOnSettings.Touch)
            {
                (UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView).SetTouchToStartLabelVisibility(true);
            }
            else if (SettingsManager.Instance.startOn == StartOnSettings.Countdown)
            {
                (UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView).ResetCountdown();
                (UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView).SetCountdownLabelVisibility(true);
            }

            HasLevelStarted = false;

            _levelTimer.Reset();
        }


        public void UseSecondChance()
        {
            _levelState = LevelState.WaitingToStart;

            _controls.DisableAndShowControls();

            _ball.SetBallVisible(true);
            _ball.FreezeBall(true);

            _usedSecondChance = true;

            _maze.ResetMazeOrientation();

            if (_lastRespawnableObstacle != null)
            {
                _ball.MoveBallToPosition(_lastRespawnableObstacle.transform.position + new Vector3(0, 0.5f, 0));
            }
        }

        /// <summary>
        /// Get the level after the current one. Overriden by child classes. Only used for default and daily levels
        /// </summary>
        /// <returns>The next level to load</returns>
        public virtual string GetNextLevel()
        {
            return "";
        }

        /// <summary>
        /// Return the number of stars the player got for the level. Overriden by child classes.
        /// </summary>
        /// <param name="levelId">The level's id</param>
        /// <returns>The number of stars the player got for the level</returns>
        public virtual int GetNumberOfStarsForLevel(string levelId, float? bestTime = null)
        {
            return 0;
        }


        /// <summary>
        /// Return the number of coins the player should earn for the given level and the number of stars gained. Overriden by child classes.
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="starsGained"></param>
        /// <returns></returns>
        public virtual int GetCoinsEarnedForLevel(int starsAlreadygained, int starsGained, string levelId = "")
        {
            return 0;
        }


        /// <summary>
        /// Player reached the target and won the level.
        /// Binded to the target's triggerAction, and so called when the ball enters a target's trigger.
        /// </summary>
        protected virtual void TargetReached()
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
        public void Lost()
        {
            if (_levelState == LevelState.Playing)
            {
                PauseLevel();

                if (_isSecondChanceEnabled && !_usedSecondChance)
                {
                    UIManager.Instance.Show(UIViewType.SecondChance);
                }
                else
                {
                    _levelState = LevelState.Lost;

                    UIManager.Instance.Show(UIViewType.LevelFailed);
                }
            }
        }


        /// <summary>
        /// Destroys all the maze's children and unbinds all actions (targets triggers...).
        /// </summary>
        private void ClearMaze()
        {
            LevelManager.levelToLoad = "";

            _lastRespawnableObstacle = null;

            MazeEvents.targetReached -= TargetReached;

            _maze.ClearMaze();
        }


        public void HandleBallCollision(Collision other)
        {
            GameObject obstacleRoot = GetObstacleGameObjectFromBallCollision(other.gameObject);
            Obstacle obstacle = _maze.obstacles[obstacleRoot];

            if (obstacle.canRespawnOn)
            {
                _lastRespawnableObstacle = obstacleRoot;
            }

            if (obstacle.canKill)
            {
                Lost();
            }
        }


        public void HandleBallTrigger(Collider other)
        {
            Obstacle obstacle = _maze.obstacles[GetObstacleGameObjectFromBallCollision(other.gameObject)];

            if (obstacle.obstacleType == ObstacleType.FlagTarget && _levelState == LevelState.Playing)
            {
                MazeEvents.targetReached?.Invoke();
            }
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
            if (gameObject.layer == 2)
            {
                return null;
            }

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
        /// Quit the level and reset everything.
        /// </summary>
        public void QuitLevel()
        {
            PauseLevel();

            if (UIManager.Instance.UIViews[UIViewType.Pause].isEnabled)
            {
                (UIManager.Instance.UIViews[UIViewType.Pause] as PauseView).ForceHide();
            }

            if (UIManager.Instance.UIViews[UIViewType.Skip].isEnabled)
            {
                (UIManager.Instance.UIViews[UIViewType.Skip] as SkipView).ForceHide();
            }

            if (UIManager.Instance.UIViews[UIViewType.LevelCompleted].isEnabled)
            {
                UIManager.Instance.UIViews[UIViewType.LevelCompleted].Hide();
            }

            if (UIManager.Instance.UIViews[UIViewType.SecondChance].isEnabled)
            {
                UIManager.Instance.UIViews[UIViewType.SecondChance].Hide();
            }

            if (UIManager.Instance.UIViews[UIViewType.LevelFailed].isEnabled)
            {
                UIManager.Instance.UIViews[UIViewType.LevelFailed].Hide();
            }


            _ball.SetBallVisible(false);
            _ball.FreezeBall(true);

            _maze.ResetMazeOrientation();

            ClearMaze();

            _levelTimer.Reset();
        }


        private void OnDestroy()
        {
            QuitLevel();
        }


        /// <summary>
        /// Reset the level manager when switching mode
        /// </summary>
        protected void ResetLevelManager()
        {
            // Reset properties
            _levelState = LevelState.Loading;
            _usedSecondChance = false;

            // Update the UI to match the selected mode
            LevelEvents.LevelModeUpdated?.Invoke(levelType);
        }
    }
}