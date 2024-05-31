using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using UnityEngine;


namespace BallMaze
{
    public class LevelDataManager
    {
        public List<string> defaultLevelsUnlocked;
        public Dictionary<string, decimal> defaultLevelsTimes;

        private int _timeSavePrecision = 3;


        public LevelDataManager()
        {
            defaultLevelsUnlocked = new List<string>();
            defaultLevelsTimes = new Dictionary<string, decimal>();
        }


        public async void UnlockDefaultLevel(string level)
        {
            if (!IsDefaultLevelUnlocked(level))
            {
                defaultLevelsUnlocked.Add(level);

                // If Cloud Save is enabled, save to the cloud
                if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
                {
                    await DataPersistenceManager.Instance.cloudDataHandler.Save(CloudSaveKey.defaultLevelsUnlocked, defaultLevelsUnlocked);
                }

                PlayerEvents.DefaultLevelUnlocked?.Invoke(level);
            }
        }


        public bool IsDefaultLevelUnlocked(string level)
        {
            return defaultLevelsUnlocked.Contains(level);
        }


        public async void SetDefaultLevelTime(string level, float time)
        {
            decimal timeDecimal = Math.Round((decimal)time, _timeSavePrecision);

            if (defaultLevelsTimes.ContainsKey(level))
            {
                // Only save the time if the player lowered their best time
                if (timeDecimal > defaultLevelsTimes[level])
                {
                    return;
                }

                defaultLevelsTimes[level] = timeDecimal;

                PlayerEvents.DefaultLevelBestTimeUpdated?.Invoke(level, time);
            }
            else
            {
                defaultLevelsTimes.Add(level, timeDecimal);
            }


            // If Cloud Save is enabled, save to the cloud
            if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
            {
                await DataPersistenceManager.Instance.cloudDataHandler.Save(CloudSaveKey.defaultLevelsTimes, defaultLevelsTimes);
            }
        }


        public float GetDefaultLevelBestTime(string level)
        {
            if (defaultLevelsTimes.ContainsKey(level))
            {
                return (float)defaultLevelsTimes[level];
            }
            else
            {
                return 0f;
            }
        }
    }
}