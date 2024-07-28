using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
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
            _dailyLevelsDataManager = new DailyLevelsDataManager();
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

            // If the player has played today's daily levels, load the times and unlock the corresponding levels
            if (data.lastDailyLevelPlayedDay == GameManager.Instance.GetUtcNowTime().DayOfYear)
            {
                foreach (KeyValuePair<string, decimal> entry in data.dailyLevelsTimes)
                {
                    _dailyLevelsDataManager.dailyLevelsUnlocked.Add(entry.Key);
                    PlayerEvents.DailyLevelUnlocked?.Invoke(entry.Key);

                    // If the time is 0, that means the player hasn't completed that level, so we stop to not load the time nor unlock the next level
                    if (entry.Value == 0m)
                    {
                        break;
                    }

                    // Load the time for that level
                    _dailyLevelsDataManager.dailyLevelsTimes.Add(entry.Key, entry.Value);
                }
            }
        }


        public void SaveData(PlayerData data)
        {
            data.coins = _coinsManager.Coins;

            data.defaultLevelsUnlocked = _defaultLevelsDataManager.defaultLevelsUnlocked;
            data.defaultLevelsTimes = _defaultLevelsDataManager.defaultLevelsTimes;

            data.dailyLevelsTimes = _dailyLevelsDataManager.dailyLevelsTimes;
        }
    }
}