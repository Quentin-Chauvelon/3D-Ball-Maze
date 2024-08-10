using System;
using BallMaze.Events;
using UnityEngine;


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
        public static string levelToLoad = "";

        [SerializeField]
        private Config _config;

        // Singleton pattern
        // Instance is of type LevelManagerBase because it can be any of the LevelManagerBase subclasses
        // And so, the LevelManager (which inherits from MonoBehaviour) has to be separated in two classes:
        // One for the MonoBehaviour and the other one for the abstract class which handles the logic
        private static LevelManagerBase _instance;
        public static LevelManagerBase Instance
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

        public static bool Initialized = false;

        private static DefaultLevelsLevelManager _defaultLevelsLevelManager;
        private static DailyLevelsLevelManager _dailyLevelsLevelManager;

        private void Awake()
        {
            _instance = new DefaultLevelsLevelManager();

            DontDestroyOnLoad(gameObject);
        }


        void Start()
        {
            _defaultLevelsLevelManager = new DefaultLevelsLevelManager();
            _dailyLevelsLevelManager = new DailyLevelsLevelManager();

            SwitchMode(LevelType.Default);

            _defaultLevelsLevelManager.Start();
            _dailyLevelsLevelManager.Start();

            _instance = _defaultLevelsLevelManager;

            if (_config.setLevelToLoad)
            {
                levelToLoad = _config.levelToLoad;
            }

            LevelEvents.LevelModeUpdated += (levelType) => { SwitchMode(levelType); };

            Initialized = true;
        }


        void Update()
        {
            _instance.Update();
        }


        public static void SwitchMode(LevelType newLevelType)
        {
            // If the given mode is already loaded, return
            if (_instance != null && _instance.levelType == newLevelType)
            {
                return;
            }

            switch (newLevelType)
            {
                case LevelType.Default:
                    _instance = _defaultLevelsLevelManager;
                    break;
                case LevelType.DailyLevel:
                    _instance = _dailyLevelsLevelManager;
                    break;
            }
        }


        public static LevelManagerBase GetLevelManagerModeInstance(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    return _defaultLevelsLevelManager;
                case LevelType.DailyLevel:
                    return _dailyLevelsLevelManager;
            }

            return null;
        }
    }
}