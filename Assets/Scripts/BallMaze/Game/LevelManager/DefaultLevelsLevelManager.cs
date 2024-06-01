using System.Collections.Generic;
using System.IO;
using BallMaze.Events;
using BallMaze.UI;
using UnityEngine;

namespace BallMaze
{
    public class DefaultLevelsLevelManager : LevelManagerBase
    {
        public override LevelType levelType => LevelType.Default;

        public override string LEVELS_PATH => Path.Combine(Application.persistentDataPath, "levels");

        protected override bool _isSecondChanceEnabled => true;

        public const string DEFAULT_LEVELS_FILE_NAME = "defaultLevels.json";
        public const string DEFAULT_LEVELS_SELECTION_FILE_NAME = "defaultLevelsSelection.json";

        private static string[] _defaultLevelsIds = null;
        private static Dictionary<string, float[]> _defaultLevelsStarsTimes = new Dictionary<string, float[]>();


        public DefaultLevelsLevelManager()
        {
            ResetLevelManager();

            // If the levels folder doesn't exist, create it
            if (!Directory.Exists(LEVELS_PATH))
            {
                Directory.CreateDirectory(LEVELS_PATH);
            }
        }


        public static void LoadDefaultLevelsIds(LevelsSelection levelsSelection)
        {
            if (_defaultLevelsIds != null)
            {
                _defaultLevelsIds = null;
            }

            if (_defaultLevelsStarsTimes != null)
            {
                _defaultLevelsStarsTimes.Clear();
            }

            _defaultLevelsIds = new string[levelsSelection.numberOfLevels];

            for (int i = 0; i < levelsSelection.numberOfLevels; i++)
            {
                _defaultLevelsIds[i] = levelsSelection.levels[i].id;

                // Add the level's stars times to the dictionary if they are not already present, otherwise simply update the times
                if (!_defaultLevelsStarsTimes.ContainsKey(levelsSelection.levels[i].id))
                {
                    _defaultLevelsStarsTimes.Add(levelsSelection.levels[i].id, levelsSelection.levels[i].times);
                }
                else
                {
                    _defaultLevelsStarsTimes[levelsSelection.levels[i].id] = levelsSelection.levels[i].times;

                }
            }
        }


        public override string GetNextLevel()
        {
            // Don't call the base method since it have any implementation

            if (_defaultLevelsIds == null || _defaultLevelsIds.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < _defaultLevelsIds.Length; i++)
            {
                // Find the current level
                if (_defaultLevelsIds[i] == _currentLevelId)
                {
                    // If the current level is the last level, return null
                    if (i == _defaultLevelsIds.Length - 1)
                    {
                        return null;
                    }

                    // Return the next level
                    return _defaultLevelsIds[i + 1];
                }
            }

            return null;
        }


        public override int GetNumberOfStarsForLevel(string levelId, float? bestTime = null)
        {
            bestTime ??= PlayerManager.Instance.LevelDataManager.GetDefaultLevelBestTime(levelId);

            if (bestTime.Value > 0f)
            {
                float[] starsTimes = _defaultLevelsStarsTimes[levelId];

                for (int i = 1; i <= starsTimes.Length; i++)
                {
                    // In the file, the times are sorted in ascending order, meaning the last star is the first element.
                    // Find the best star the player has, and return its index
                    if (bestTime <= starsTimes[3 - i])
                    {
                        return 4 - i;
                    }
                }
            }

            return 0;
        }


        protected override void TargetReached()
        {
            base.TargetReached();

            float bestTime = PlayerManager.Instance.LevelDataManager.GetDefaultLevelBestTime(_currentLevelId);

            // Update the player's best time for the current level
            if (bestTime == 0f || _levelTimer.GetTime() < bestTime)
            {
                // Save the time of the current level
                PlayerManager.Instance.LevelDataManager.SetDefaultLevelTime(_currentLevelId, _levelTimer.GetTime());
            }

            string nextLevelId = GetNextLevel();

            if (nextLevelId != null)
            {
                // Unlock the next level
                if (!PlayerManager.Instance.LevelDataManager.IsDefaultLevelUnlocked(nextLevelId))
                {
                    PlayerManager.Instance.LevelDataManager.UnlockDefaultLevel(nextLevelId);
                }
            }
        }
    }
}