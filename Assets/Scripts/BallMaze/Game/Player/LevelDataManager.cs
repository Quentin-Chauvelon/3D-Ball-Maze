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

        // Number of decimal places to save the time to
        private int _timeSavePrecision = 3;


        public LevelDataManager()
        {
            defaultLevelsUnlocked = new List<string>();
            defaultLevelsTimes = new Dictionary<string, decimal>();
        }


        /// <summary>
        /// Unlock the default level matching the given id
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Return if the default level matching the given id is unlocked
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool IsDefaultLevelUnlocked(string level)
        {
            return defaultLevelsUnlocked.Contains(level);
        }


        /// <summary>
        /// Set the best time for the default level matching the given id.
        /// If force is set so true, the time will be saved even if it's lower than the current best time. Otherwise, it won't be saved. Defaults to false
        /// </summary>
        /// <param name="level"></param>
        /// <param name="time"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public async void SetDefaultLevelTime(string level, float time, bool force = false)
        {
            decimal timeDecimal = Math.Round((decimal)time, _timeSavePrecision);

            if (defaultLevelsTimes.ContainsKey(level))
            {
                // Only save the time if the player lowered their best time or if force is set to true
                if (defaultLevelsTimes[level] != 0m && timeDecimal > defaultLevelsTimes[level] && !force)
                {
                    return;
                }

                defaultLevelsTimes[level] = timeDecimal;

                LevelEvents.DefaultLevelBestTimeUpdated?.Invoke(level, time);
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


        /// <summary>
        /// Return the best time for the default level matching the given id
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
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