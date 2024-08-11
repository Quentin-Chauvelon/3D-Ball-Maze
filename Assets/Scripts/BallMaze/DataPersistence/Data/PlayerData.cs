using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace BallMaze
{
    [Serializable]
    public class PlayerData
    {
        public long lastUpdated;

        public int coins;

        public List<string> defaultLevelsUnlocked;
        public Dictionary<string, decimal> defaultLevelsTimes;

        [JsonIgnore]
        public List<string> dailyLevelsUnlocked; // Not saved, filled at runtime based on the dailyLevelsTimes and lastDailyLevelPlayedDay
        public Dictionary<string, decimal> dailyLevelsTimes;
        public int lastDailyLevelPlayedDay; // Not saved on SaveData, it is saved everytime the player finishes a daily level
        public int dailyLevelStreak;
        // The highest daily level difficulty the player has completed on the last day they played.
        // This allows us to know when to increment or reset the streak based on the day and the difficulty.
        // The key represents the day number in the year
        public KeyValuePair<int, DailyLevelDifficulty> lastDailyLevelCompleted;

        public List<int> ownedSkins;


        public PlayerData()
        {
            coins = 0;
            defaultLevelsUnlocked = new List<string>();
            defaultLevelsTimes = new Dictionary<string, decimal>();
            dailyLevelsTimes = new Dictionary<string, decimal>();
            lastDailyLevelCompleted = new KeyValuePair<int, DailyLevelDifficulty>(GameManager.Instance.GetUtcNowTime().DayOfYear, DailyLevelDifficulty.Easy);
            ownedSkins = new List<int>();
        }
    }
}