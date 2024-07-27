using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    public class PlayerManager : MonoBehaviour, IDataPersistence
    {
        // Singleton pattern
        private static PlayerManager _instance;
        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("PlayerManager is null!");
                }
                return _instance;
            }
        }

        private CoinsManager _coinsManager;
        public CoinsManager CoinsManager
        {
            get { return _coinsManager; }
        }

        private DefaultLevelsDataManager _defaultLevelsDataManager;
        public DefaultLevelsDataManager DefaultLevelsDataManager
        {
            get { return _defaultLevelsDataManager; }
        }

        private DailyLevelsDataManager _dailyLevelsDataManager;
        public DailyLevelsDataManager DailyLevelsDataManager
        {
            get { return _dailyLevelsDataManager; }
        }


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _coinsManager = new CoinsManager();

            _defaultLevelsDataManager = new DefaultLevelsDataManager();
        }



        public void LoadData(PlayerData data)
        {
            _coinsManager.SetCoins(data.coins);

            _defaultLevelsDataManager.defaultLevelsUnlocked = data.defaultLevelsUnlocked;
            _defaultLevelsDataManager.defaultLevelsTimes = data.defaultLevelsTimes;

            // If there are no levels unlocked, add the first one
            if (_defaultLevelsDataManager.defaultLevelsUnlocked.Count == 0)
            {
                _defaultLevelsDataManager.defaultLevelsUnlocked.Add("1");
            }
        }


        public void SaveData(PlayerData data)
        {
            data.coins = _coinsManager.Coins;

            data.defaultLevelsUnlocked = _defaultLevelsDataManager.defaultLevelsUnlocked;
            data.defaultLevelsTimes = _defaultLevelsDataManager.defaultLevelsTimes;
        }
    }
}