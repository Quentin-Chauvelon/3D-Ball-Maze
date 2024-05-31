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

            _defaultLevelsIds = new string[levelsSelection.numberOfLevels];

            for (int i = 0; i < levelsSelection.numberOfLevels; i++)
            {
                _defaultLevelsIds[i] = levelsSelection.levels[i].id;
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