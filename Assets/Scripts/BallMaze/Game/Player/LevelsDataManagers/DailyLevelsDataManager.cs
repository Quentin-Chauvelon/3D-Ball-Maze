using System;
using System.Collections.Generic;
using BallMaze.Events;


namespace BallMaze
{
    public class DailyLevelsDataManager : LevelDataManager
    {
        public List<string> dailyLevelsUnlocked;
        public Dictionary<string, decimal> dailyLevelsTimes;


        public DailyLevelsDataManager()
        {
            dailyLevelsUnlocked = new List<string>();
            dailyLevelsTimes = new Dictionary<string, decimal>();
        }


        /// <summary>
        /// Unlock the daily level matching the given id
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <returns></returns>
        public void UnlockLevel(string level)
        {
            base.UnlockLevel(level, dailyLevelsUnlocked, PlayerEvents.DailyLevelUnlocked);
        }


        /// <summary>
        /// Return if the daily level matching the given id is unlocked
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <returns></returns>
        public bool IsLevelUnlocked(string level)
        {
            return base.IsLevelUnlocked(level, dailyLevelsUnlocked);
        }


        /// <summary>
        /// Set the best time for the daily level matching the given id.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="time"></param>
        /// <param name="forceSave">If true, the time will be saved even if it's lower than the current best time. Otherwise, it won't be saved. Defaults to false</param>
        /// <returns></returns>
        public async void SetLevelTime(string level, float time, bool forceSave = false)
        {
            base.SetLevelTime(level, dailyLevelsTimes, time, cloudSaveKey: CloudSaveKey.dailyLevelsTimes, forceSave: forceSave);

            // If Cloud Save is enabled, save to the cloud
            if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
            {
                await DataPersistenceManager.Instance.cloudDataHandler.Save(CloudSaveKey.lastDailyLevelPlayedDay, DateTime.UtcNow.DayOfYear);
            }
        }


        /// <summary>
        /// Return the best time for the daily level matching the given id
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public float GetLevelBestTime(string level)
        {
            return base.GetLevelBestTime(level, dailyLevelsTimes);
        }
    }
}