using BallMaze.Events;
using BallMaze.Obstacles;
using BallMaze.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;

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

        private GameObject _lastRespawnableObstacle;


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
                    // If the player has start on touch enabled, is not over a UI element and clicks or touches the screen, start the game
                    if (SettingsManager.Instance.startOn == StartOnSettings.Touch &&
                        !EventSystem.current.currentSelectedGameObject &&
                        (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
                    {
                        StartLevel();
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
        public void LoadLevel(string levelId)
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

            _ball.MoveBallToPosition(_lastRespawnableObstacle.transform.position + new Vector3(0, 0.5f, 0));
        }

        /// <summary>
        /// Get the level after the current one. Overriden by child classes. Only used for default and daily levels
        /// </summary>
        /// <returns>The next level to load</returns>
        public virtual string GetNextLevel() { return ""; }


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

                UIManager.Instance.Show(UIViewType.LevelCompleted);
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

            if (obstacle.obstacleType == ObstacleType.FlagTarget)
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
            ((PauseView)UIManager.Instance.UIViews[UIViewType.Pause]).SwitchLevelTypeSource(levelType);
            ((LevelFailedView)UIManager.Instance.UIViews[UIViewType.LevelFailed]).SwitchLevelTypeSource(levelType);
        }
    }
}