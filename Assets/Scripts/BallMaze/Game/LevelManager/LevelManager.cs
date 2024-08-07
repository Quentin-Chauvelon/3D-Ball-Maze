using System;
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

        private void Awake()
        {
            SwitchMode(LevelType.Default);
            DontDestroyOnLoad(gameObject);
        }


        void Start()
        {
            _instance.Start();

            if (_config.setLevelToLoad)
            {
                levelToLoad = _config.levelToLoad;
            }
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
                    _instance = new DefaultLevelsLevelManager();
                    break;
                    // case LevelType.DailyLevel:
                    //     _instance = new DailyLevelsLevelManager();
                    //     break;
            }
        }
    }
}