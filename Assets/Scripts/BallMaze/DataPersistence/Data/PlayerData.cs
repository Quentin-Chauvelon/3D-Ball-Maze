using System;
using System.Collections;
using System.Collections.Generic;
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
        public int[] dailyLevelsTimes;
        public List<int> ownedSkins;


        public PlayerData()
        {
            coins = 0;
            defaultLevelsUnlocked = new List<string>() { "1" };
            defaultLevelsTimes = new Dictionary<string, decimal>();
            dailyLevelsTimes = new int[5];
            ownedSkins = new List<int>();
        }
    }
}