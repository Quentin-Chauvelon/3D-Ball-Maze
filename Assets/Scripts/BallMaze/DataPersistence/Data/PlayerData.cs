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
        public Dictionary<string, int> defaultLevelsTimes;
        public int[] dailyLevelsTimes;
        public List<int> ownedSkins;


        public PlayerData()
        {
            coins = 0;
            defaultLevelsTimes = new Dictionary<string, int>();
            dailyLevelsTimes = new int[5];
            ownedSkins = new List<int>();
        }
    }
}