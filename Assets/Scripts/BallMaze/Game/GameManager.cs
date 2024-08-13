using BallMaze.UI;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityExtensionMethods;
using Debug = UnityEngine.Debug;


namespace BallMaze
{
    /// <summary>
    /// The possible states of the game
    /// </summary>
    public enum GameState
    {
        Loading,
        MainMenu,
        ModeSelection,
        LevelSelection,
        DailyLevelsSelection,
        Playing
    }


    public class GameManager : MonoBehaviour
    {
        // Singleton pattern
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("GameManager is null!");
                }
                return _instance;
            }
        }

        public static bool DEBUG = true;

        private GameState _gameState;

        private DateTime _lastUnfocus;

        private int _lastFrameMinute;

        public DefaultLevelSelection defaultLevelSelection;

        // These two variables are used to test the editor and act as different platforms.
        // This is primarly used for features like Cloud Save where WebGL and mobile have different implementations
        [SerializeField] public bool EditorIsWebGL;
        [SerializeField] public bool EditorIsMobile;

        [SerializeField] public RemoteConfigFetchState RemoteConfigFetchState;

        [SerializeField] public bool MockNowTimeEnabled;
        [SerializeField] public string MockNowTime;

        public static bool isQuitting = false;


        public void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _gameState = GameState.Loading;

            _lastUnfocus = DateTime.UnixEpoch;

            defaultLevelSelection = new DefaultLevelSelection();
        }


        public void Initialize()
        {
            Debug.Log("Initializing game");
            GameObject.Find("DataPersistenceManager").GetComponent<DataPersistenceManager>().Initialize();
            defaultLevelSelection.Initialize();

            MockNowTime = DateTime.UtcNow.AddDays(1).Date.AddMinutes(1).ToString(); // Tomorrow at 00:01
        }


        /// <summary>
        /// Start the game once it has finished loading
        /// </summary>
        public void StartGame()
        {
            _gameState = GameState.MainMenu;

            RemoteConfigManager.Initialize();
            RemoteConfigManager.LoadRemoteConfig();

            UIManager.Instance.Initialize();
        }


        private void Update()
        {
            // When the player presses escape (or back button on Android)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // If we are playing, show the level quit confirmation
                if (_gameState == GameState.Playing)
                {
                    // If a modal is opened, we want to close it
                    if (UIManager.Instance.IsModalOpened)
                    {
                        UIManager.Instance.Back();
                    }
                    // If no modal is opened, show the quit confirmation
                    else
                    {
                        // Don't call UIManager.Show(), as it will add the view to the navigation history, instead, call Show() on the view itself
                        UIManager.Instance.UIViews[UIViewType.LevelQuitConfirmation].Show();
                    }
                }
                // If the user is on the main menu, show the game quit confirmation
                else if (_gameState == GameState.MainMenu)
                {
                    // If a modal is opened, we want to close it
                    if (UIManager.Instance.IsModalOpened)
                    {
                        UIManager.Instance.Back();
                    }
                    // If no modal is opened, show the quit confirmation
                    else
                    {
                        // Don't call UIManager.Show(), as it will add the view to the navigation history, instead, call Show() on the view itself
                        UIManager.Instance.UIViews[UIViewType.GameQuitConfirmation].Show();
                    }
                }
                // Otherwise, go back to the previous UI view in the navigation history
                else
                {
                    UIManager.Instance.Back();
                }
            }

            DateTime currentTime = GetUtcNowTime();

            // If a minute has passed
            if (currentTime.Minute != _lastFrameMinute)
            {
                Debug.Log("Current time: " + currentTime.ToString());
                _lastFrameMinute = currentTime.Minute;

                // If it's 23:50
                if (currentTime.Hour == 23 && _lastFrameMinute == 51)
                {
                    // If the player is playing the daily levels, move them back to the main menu
                    if (_gameState == GameState.Playing && LevelManager.Instance.levelType == LevelType.DailyLevel)
                    {
                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels will be updated in 10 minutes!");
                    }
                }
                // If it's 23:55
                else if (currentTime.Hour == 23 && _lastFrameMinute == 56)
                {
                    // If the player is playing the daily levels, move them back to the main menu
                    if (_gameState == GameState.Playing && LevelManager.Instance.levelType == LevelType.DailyLevel)
                    {
                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels will be updated in 5 minutes!");
                    }
                }
                // If it's 23:58
                else if (currentTime.Hour == 23 && _lastFrameMinute == 59)
                {
                    // If the player is playing the daily levels, move them back to the main menu
                    if (_gameState == GameState.Playing && LevelManager.Instance.levelType == LevelType.DailyLevel)
                    {
                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels will be updated in 2 minutes!");
                    }
                }
                // If it's 23:59
                else if (currentTime.Hour == 0 && _lastFrameMinute == 0)
                {
                    // If the player is playing the daily levels, move them back to the main menu
                    if (_gameState == GameState.Playing && LevelManager.Instance.levelType == LevelType.DailyLevel)
                    {
                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels will be updated in 1 minutes!");
                    }
                }
                // If it's 00:01
                else if (currentTime.Hour == 0 && _lastFrameMinute == 1)
                {
                    // Hide the daily reward button until the rewards are loaded from remote config
                    (UIManager.Instance.UIViews[UIViewType.MainMenu] as MainMenuView).SetDailyRewardsButtonVisibility(false);

                    // If the player didn't collect the reward yesterday or today or if they reached day 7 yesterday or before, reset their streak
                    if (DailyReward.LastDailyRewardClaimedDay < GetUtcNowTime().DayOfYear - 1 ||
                        (DailyReward.DailyRewardStreak == 7 && DailyReward.LastDailyRewardClaimedDay != GetUtcNowTime().DayOfYear))
                    {
                        DailyReward.DailyRewardStreak = 0;
                    }

                    DailyReward.Collected = false;

                    (UIManager.Instance.UIViews[UIViewType.DailyReward] as DailyRewardView).ResetDailyRewardDays();
                    (UIManager.Instance.UIViews[UIViewType.DailyReward] as DailyRewardView).UnbindDailyRewardDaysButtonsClicks();
                    (UIManager.Instance.UIViews[UIViewType.DailyReward] as DailyRewardView).BindDailyRewardDayButtonClick();

                    // If the player is playing the daily levels, move them back to the main menu
                    if (_gameState == GameState.Playing && LevelManager.Instance.levelType == LevelType.DailyLevel)
                    {
                        LevelManager.Instance.QuitLevel();

                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels have been updated. You have been redirected to the main menu!");
                        UIManager.Instance.Show(UIViewType.MainMenu);
                    }
                    else if (_gameState == GameState.DailyLevelsSelection)
                    {
                        (UIManager.Instance.UIViews[UIViewType.Notification] as NotificationView).Notify("Daily levels have been updated. You have been redirected to the main menu!");
                        UIManager.Instance.Show(UIViewType.MainMenu);
                    }

                    DailyLevelsLevelManager.LastDailyLevelCompleted = new KeyValuePair<int, DailyLevelDifficulty>(GetUtcNowTime().DayOfYear, DailyLevelDifficulty.Unknown);

                    // Reload the remote config to get the new daily levels
                    RemoteConfigManager.LoadRemoteConfig();
                }
            }
        }


        public DateTime GetUtcNowTime()
        {
            if (!MockNowTimeEnabled)
            {
                return DateTime.UtcNow;
            }
            else
            {
                DateTime now = DateTime.UtcNow;

                DateTime.TryParse(MockNowTime, out now);

                return now;
            }
        }


        /// <summary>
        /// Updates the game state based on the current UI view
        /// </summary>
        /// <param name="uiView"></param>
        public void UpdateGameState(UIViewType uiView)
        {
            Debug.Log("Current game state: " + uiView.ToString());

            switch (uiView)
            {
                case UIViewType.MainMenu:
                    _gameState = GameState.MainMenu;
                    break;
                case UIViewType.ModeSelection:
                    _gameState = GameState.ModeSelection;
                    break;
                case UIViewType.DefaultLevelSelection:
                    _gameState = GameState.LevelSelection;
                    break;
                case UIViewType.DailyLevels:
                    _gameState = GameState.DailyLevelsSelection;
                    break;
                case UIViewType.Playing:
                    _gameState = GameState.Playing;
                    break;
                default:
                    _gameState = GameState.Playing;
                    break;
            }

            if (_gameState == GameState.Playing)
            {
                UIManager.Instance.UIViews[UIViewType.Background].Hide();
            }
            else
            {
                UIManager.Instance.UIViews[UIViewType.Background].Show();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            // If the game is paused, save the time it was paused at, this will allow to know how much time the game was paused for
            if (pause)
            {
                _lastUnfocus = GameManager.Instance.GetUtcNowTime();

                // Save the game on Application.Pause() and not on Application.Quit().
                // This is done because Application.Quit() is not always called on mobile devices (especially older ones)
                DataPersistenceManager.Instance.SaveGame();
            }
            else
            {
                // If the game has been paused for less than 1 hour and the player is in the level selection screen, bring him back to the main menu
                // This way, when the player clicks on the level selection button, levels update and UI populate will be handled by the LevelSelection script
                if (_lastUnfocus.DateInTimeframe(3600))
                {
                    UIManager.Instance.Show(UIViewType.MainMenu);
                }
                // If the levels have not been populated yet, load them
                else if (!(UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).IsDefaultLevelSelectionViewLoaded())
                {
                    defaultLevelSelection.LoadDefaultLevelSelection();
                }
            }
        }


        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(0);

            LevelManager.Instance.QuitLevel();
        }


        private void OnApplicationQuit()
        {
            isQuitting = true;

            DataPersistenceManager.Instance.SaveGame();
        }


        /// <summary>
        /// Quits the game
        /// </summary>
        public void QuitGame()
        {
            DataPersistenceManager.Instance.SaveGame();

            Application.Quit();

            // Application.Quit() doesn't work in the editor, so we need to stop the editor manually
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


        /// <summary>
        /// Return the screen size in pixels
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetScreenSize()
        {
            return new Vector2(Screen.width, Screen.height);
        }
    }
}