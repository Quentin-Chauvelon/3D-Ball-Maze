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

        private LevelDataManager _levelDataManager;
        public LevelDataManager LevelDataManager
        {
            get { return _levelDataManager; }
        }


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _coinsManager = new CoinsManager();
            _levelDataManager = new LevelDataManager();
        }



        public void LoadData(PlayerData data)
        {
            _coinsManager.SetCoins(data.coins);

            _levelDataManager.defaultLevelsUnlocked = data.defaultLevelsUnlocked;
            _levelDataManager.defaultLevelsTimes = data.defaultLevelsTimes;

            // If there are no levels unlocked, add the first one
            if (_levelDataManager.defaultLevelsUnlocked.Count == 0)
            {
                _levelDataManager.defaultLevelsUnlocked.Add("1");
            }
        }


        public void SaveData(PlayerData data)
        {
            data.coins = _coinsManager.Coins;

            data.defaultLevelsUnlocked = _levelDataManager.defaultLevelsUnlocked;
            data.defaultLevelsTimes = _levelDataManager.defaultLevelsTimes;
        }
    }
}