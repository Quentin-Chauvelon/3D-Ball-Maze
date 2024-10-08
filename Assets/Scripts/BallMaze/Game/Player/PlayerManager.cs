using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using BallMaze.UI;
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

        private SkinManager _skinManager;
        public SkinManager SkinManager
        {
            get { return _skinManager; }
        }

        public bool Initialized = false;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _coinsManager = new CoinsManager();

            _defaultLevelsDataManager = new DefaultLevelsDataManager();
            _dailyLevelsDataManager = new DailyLevelsDataManager();
            _skinManager = new SkinManager();
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

            _dailyLevelsDataManager.dailyLevelsUnlocked.Add("DailyLevelVeryEasy");

            // If the player has played today's daily levels, load the times and unlock the corresponding levels
            if (data.lastDailyLevelPlayedDay == GameManager.Instance.GetUtcNowTime().DayOfYear)
            {
                foreach (KeyValuePair<string, decimal> entry in data.dailyLevelsTimes)
                {
                    // If the time is 0, that means the player hasn't completed that level, so we stop to not load the time nor unlock the next level
                    if (entry.Value == 0m)
                    {
                        break;
                    }

                    // If level id is not the last level, unlock the next one
                    if (entry.Key != "DailyLevelVeryExtreme")
                    {
                        string nextLevelId = DailyLevelsLevelManager.GetDailyLevelIdFromDifficulty(DailyLevelsLevelManager.GetDailyLevelDifficultyFromId(entry.Key) + 1);

                        _dailyLevelsDataManager.dailyLevelsUnlocked.Add(nextLevelId);
                        PlayerEvents.DailyLevelUnlocked?.Invoke(nextLevelId);
                    }

                    // Load the time for that level
                    _dailyLevelsDataManager.dailyLevelsTimes.Add(entry.Key, entry.Value);
                }
            }

            DailyLevelsLevelManager.LastDailyLevelsPlayedDay = data.lastDailyLevelPlayedDay;
            DailyLevelsLevelManager.DailyLevelsStreak = data.dailyLevelStreak;

            // If the player hasn't played today's daily levels, reset the highest daily level difficulty completed. Otherwise, load it
            if (data.lastDailyLevelCompleted.Key <= GameManager.Instance.GetUtcNowTime().DayOfYear - 1)
            {
                // Using unknown since it has a value of 0
                DailyLevelsLevelManager.LastDailyLevelCompleted = new KeyValuePair<int, DailyLevelDifficulty>(GameManager.Instance.GetUtcNowTime().DayOfYear - 1, DailyLevelDifficulty.Unknown);
            }
            else
            {
                DailyLevelsLevelManager.LastDailyLevelCompleted = data.lastDailyLevelCompleted;
            }

            // If the last completed daily level dates from before yesterday or if it was yesterday but the player didn't complete the extreme level
            // or if they reached a streak of 7 days, reset the streak
            if (data.lastDailyLevelCompleted.Key < GameManager.Instance.GetUtcNowTime().DayOfYear - 1 ||
                (data.lastDailyLevelCompleted.Key == GameManager.Instance.GetUtcNowTime().DayOfYear - 1 && data.lastDailyLevelCompleted.Value != DailyLevelDifficulty.Extreme) ||
                (data.lastDailyLevelCompleted.Key == GameManager.Instance.GetUtcNowTime().DayOfYear - 1 && data.dailyLevelStreak == 7))
            {
                (UIManager.Instance.UIViews[UIViewType.DailyLevels] as DailyLevelsView).ResetStreak();
                DailyLevelsLevelManager.DailyLevelsStreak = 0;
            }
            // Otherwise, update the UI to match the current streak
            else
            {
                (UIManager.Instance.UIViews[UIViewType.DailyLevels] as DailyLevelsView).UpdateStreak((int)data.lastDailyLevelCompleted.Value);
            }

            DailyReward.DailyRewardStreak = data.dailyRewardStreak;
            DailyReward.LastDailyRewardClaimedDay = data.lastDailyRewardClaimedDay;

            // If the player didn't collect the reward yesterday or today or if they reached day 7 yesterday or before, reset their streak
            if (data.lastDailyRewardClaimedDay < GameManager.Instance.GetUtcNowTime().DayOfYear - 1 ||
                (data.dailyRewardStreak == 7 && data.lastDailyRewardClaimedDay != GameManager.Instance.GetUtcNowTime().DayOfYear))
            {
                DailyReward.DailyRewardStreak = 0;
            }

            // If the player collected the reward today, mark it as collected
            if (data.lastDailyRewardClaimedDay == GameManager.Instance.GetUtcNowTime().DayOfYear)
            {
                DailyReward.Collected = true;
            }

            // If for some reasons, the player has no unlocked skins, add the default one
            if (data.unlockedSkins.Count == 0)
            {
                data.unlockedSkins.Add(0);
            }

            SkinManager.SetUnlockedSkins(data.unlockedSkins);
            SkinManager.EquippedSkin = data.equippedSkin;

            Initialized = true;
        }


        public void SaveData(PlayerData data)
        {
            data.coins = _coinsManager.Coins;

            data.defaultLevelsUnlocked = _defaultLevelsDataManager.defaultLevelsUnlocked;
            data.defaultLevelsTimes = _defaultLevelsDataManager.defaultLevelsTimes;

            data.dailyLevelsTimes = _dailyLevelsDataManager.dailyLevelsTimes;

            data.lastDailyLevelPlayedDay = DailyLevelsLevelManager.LastDailyLevelsPlayedDay;
            data.dailyLevelStreak = DailyLevelsLevelManager.DailyLevelsStreak;
            data.lastDailyLevelCompleted = DailyLevelsLevelManager.LastDailyLevelCompleted;

            data.dailyRewardStreak = DailyReward.DailyRewardStreak;
            data.lastDailyRewardClaimedDay = DailyReward.LastDailyRewardClaimedDay;

            data.unlockedSkins = SkinManager.GetUnlockedSkins();
            data.equippedSkin = SkinManager.EquippedSkin;
        }
    }
}