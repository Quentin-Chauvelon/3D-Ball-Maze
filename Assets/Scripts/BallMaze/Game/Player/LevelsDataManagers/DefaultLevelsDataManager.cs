using System.Collections.Generic;
using BallMaze.Events;


namespace BallMaze
{
    public class DefaultLevelsDataManager : LevelDataManager
    {
        public List<string> defaultLevelsUnlocked;
        public Dictionary<string, decimal> defaultLevelsTimes;


        public DefaultLevelsDataManager()
        {
            defaultLevelsUnlocked = new List<string>();
            defaultLevelsTimes = new Dictionary<string, decimal>();
        }


        /// <summary>
        /// Unlock the default level matching the given id
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <returns></returns>
        public void UnlockLevel(string level)
        {
            base.UnlockLevel(level, defaultLevelsUnlocked, PlayerEvents.DefaultLevelUnlocked, CloudSaveKey.defaultLevelsUnlocked);
        }


        /// <summary>
        /// Return if the default level matching the given id is unlocked
        /// </summary>
        /// <param name="level">The id of the level</param>
        /// <returns></returns>
        public bool IsLevelUnlocked(string level)
        {
            return base.IsLevelUnlocked(level, defaultLevelsUnlocked);
        }


        /// <summary>
        /// Set the best time for the default level matching the given id.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="time"></param>
        /// <param name="forceSave">If true, the time will be saved even if it's lower than the current best time. Otherwise, it won't be saved. Defaults to false</param>
        /// <returns></returns>
        public void SetLevelTime(string level, float time, bool forceSave = false)
        {
            base.SetLevelTime(level, defaultLevelsTimes, time, LevelEvents.DefaultLevelBestTimeUpdated, CloudSaveKey.defaultLevelsTimes, forceSave);
        }


        /// <summary>
        /// Return the best time for the default level matching the given id
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public float GetLevelBestTime(string level)
        {
            return base.GetLevelBestTime(level, defaultLevelsTimes);
        }
    }
}