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
        public List<string> dailyLevelsUnlocked; // Not saved, filled at runtime based on the dailyLevelsTimes and lastDailyLevelPlayedDate
        public Dictionary<string, decimal> dailyLevelsTimes;
        public int lastDailyLevelPlayedDay; // Not saved on SaveData, it is saved everytime the player finishes a daily level

        public List<int> ownedSkins;


        public PlayerData()
        {
            coins = 0;
            defaultLevelsUnlocked = new List<string>();
            defaultLevelsTimes = new Dictionary<string, decimal>();
            dailyLevelsTimes = new Dictionary<string, decimal>();
            ownedSkins = new List<int>();
        }
    }
}