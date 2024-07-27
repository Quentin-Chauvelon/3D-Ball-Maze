using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using UnityEngine;


namespace BallMaze
{
    public abstract class LevelDataManager
    {
        // Number of decimal places to save the time to
        protected int _timeSavePrecision = 3;


        /// <summary>
        /// Unlock the default level matching the given id
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <param name="unlockedLevels">A list of the unlocked level ids for the mode</param>
        /// <param name="unlockLevelAction">The action to invoke with the level id to update UIs...</param>
        /// <param name="cloudSaveKey">The key under which to save the unlocked levels list on the cloud. If the list should NOT be saved, use CloudSaveKey.unknowned</param>
        /// <returns></returns>
        public async void UnlockLevel(string level, List<string> unlockedLevels, Action<string> unlockLevelAction = null, CloudSaveKey cloudSaveKey = CloudSaveKey.unknowned)
        {
            if (!IsLevelUnlocked(level, unlockedLevels))
            {
                unlockedLevels.Add(level);

                // If Cloud Save is enabled, save to the cloud
                if (cloudSaveKey != CloudSaveKey.unknowned && DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
                {
                    await DataPersistenceManager.Instance.cloudDataHandler.Save(cloudSaveKey, unlockedLevels);
                }

                if (unlockLevelAction != null)
                {
                    unlockLevelAction?.Invoke(level);
                }
            }
        }


        /// <summary>
        /// Return if the default level matching the given id is unlocked
        /// </summary>
        /// <param name="level"></param>
        /// /// <param name="unlockedLevels">A list of the unlocked level ids for the mode</param>
        /// <returns></returns>
        public bool IsLevelUnlocked(string level, List<string> unlockedLevels)
        {
            return unlockedLevels.Contains(level);
        }


        /// <summary>
        /// Set the best time for the default level matching the given id.
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <param name="levelsTimes">A list of the best time for each level of the mode</param>
        /// <param name="time">The time the player got</param>
        /// /// <param name="setLevelTimeAction">The action to invoke with the level id and time to update UIs...</param>
        /// <param name="cloudSaveKey">The key under which to save the levels best time on the cloud. If the list should NOT be saved, use CloudSaveKey.unknowned</param>
        /// <param name="forceSave">If true, the time will be saved even if it's lower than the current best time. Otherwise, it won't be saved. Defaults to false</param>
        /// <returns></returns>
        public async void SetLevelTime(string level, Dictionary<string, decimal> levelsTimes, float time, Action<string, float> setLevelTimeAction = null, CloudSaveKey cloudSaveKey = CloudSaveKey.unknowned, bool forceSave = false)
        {
            decimal timeDecimal = Math.Round((decimal)time, _timeSavePrecision);

            if (levelsTimes.ContainsKey(level))
            {
                // Only save the time if the player lowered their best time or if force is set to true
                if (levelsTimes[level] != 0m && timeDecimal > levelsTimes[level] && !forceSave)
                {
                    return;
                }

                levelsTimes[level] = timeDecimal;

                if (setLevelTimeAction != null)
                {
                    setLevelTimeAction?.Invoke(level, time);
                }
            }
            else
            {
                levelsTimes.Add(level, timeDecimal);
            }

            // If Cloud Save is enabled, save to the cloud
            if (cloudSaveKey != CloudSaveKey.unknowned && DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
            {
                await DataPersistenceManager.Instance.cloudDataHandler.Save(cloudSaveKey, levelsTimes);
            }
        }


        /// <summary>
        /// Return the best time for the default level matching the given id
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <param name="levelsTimes">A list of the best time for each level of the mode</param>
        /// <returns></returns>
        public float GetLevelBestTime(string level, Dictionary<string, decimal> levelsTimes)
        {
            if (levelsTimes.ContainsKey(level))
            {
                return (float)levelsTimes[level];
            }
            else
            {
                return 0f;
            }
        }
    }
}