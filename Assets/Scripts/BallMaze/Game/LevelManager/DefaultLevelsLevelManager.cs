using System;
using System.Collections.Generic;
using System.IO;
using BallMaze.Events;
using BallMaze.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityExtensionMethods;

namespace BallMaze
{
    public class LevelInformation
    {
        public string id;
        public string name;
        public float[] times;


        public LevelInformation(string id, string name, float[] times)
        {
            this.id = id;
            this.name = name;
            this.times = times;
        }
    }


    public class DefaultLevelsLevelManager : LevelManagerBase
    {
        public override LevelType levelType => LevelType.Default;

        public override string LEVELS_PATH => Path.Combine(Application.persistentDataPath, "levels");

        protected override bool _isSecondChanceEnabled => true;

        public const string DEFAULT_LEVELS_FILE_NAME = "defaultLevels.json";
        public const string DEFAULT_LEVELS_SELECTION_FILE_NAME = "defaultLevelsSelection.json";

        private static string[] _defaultLevelsIds = null;
        private static Dictionary<string, LevelInformation> _defaultLevelsInformation = new Dictionary<string, LevelInformation>();


        public DefaultLevelsLevelManager()
        {
            ResetLevelManager();

            // If the levels folder doesn't exist, create it
            if (!Directory.Exists(LEVELS_PATH))
            {
                Directory.CreateDirectory(LEVELS_PATH);
            }
        }


        /// <summary>
        /// Load information about each level when deserializing the default levels file.
        /// This contains the id, name, times... of each level.
        /// </summary>
        /// <param name="levelsSelection"></param>
        public static void LoadDefaultLevelsInformation(LevelsSelection levelsSelection)
        {
            // If the levels information has already been loaded, remove the previous information
            if (_defaultLevelsIds != null)
            {
                _defaultLevelsIds = null;
            }

            if (_defaultLevelsInformation.Count > 0)
            {
                _defaultLevelsInformation.Clear();
            }

            // Create an array based on the nubmer of levels to load
            _defaultLevelsIds = new string[levelsSelection.numberOfLevels];

            for (int i = 0; i < levelsSelection.numberOfLevels; i++)
            {
                LevelSelection level = levelsSelection.levels[i];

                _defaultLevelsIds[i] = level.id;

                LevelInformation levelInformation = new LevelInformation(
                    level.id,
                    level.name,
                    level.times
                );

                if (_defaultLevelsInformation.ContainsKey(level.id))
                {
                    _defaultLevelsInformation[level.id] = levelInformation;
                }
                else
                {
                    _defaultLevelsInformation.Add(level.id, levelInformation);
                }
            }
        }


        public override void LoadLevel(string levelId)
        {
            base.LoadLevel(levelId);

            float bestTime = PlayerManager.Instance.LevelDataManager.GetDefaultLevelBestTime(_currentLevelId);

            if (GameManager.DEBUG)
            {
                Debug.Log($"Loaded level {_currentLevelId}. Best time: {bestTime}");
            }

            // Update the UI to match the level information
            LevelEvents.LevelNameUpdated?.Invoke(_defaultLevelsInformation[_currentLevelId].name);
            LevelEvents.BestTimeUpdated?.Invoke(bestTime);

            int numberOfStars = GetNumberOfStarsForLevel(_currentLevelId, bestTime);

            LevelEvents.NextStarTimeUpdated?.Invoke(numberOfStars < 3
                ? GetNextStarTime(_currentLevelId, 3 - numberOfStars)
                : null
            );
        }


        public override string GetNextLevel()
        {
            // Don't call the base method since it doesn't have any implementation

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
                float[] starsTimes = _defaultLevelsInformation[levelId].times;

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


        public override int GetCoinsEarnedForLevel(int starsAlreadygained, int starsGained, string levelId = "")
        {
            int coinsEarned = 0;

            for (int i = starsAlreadygained; i < starsGained; i++)
            {
                coinsEarned += 10 * (i + 1);
            }

            return coinsEarned;
        }


        /// <summary>
        /// Get the time the player needs to get the next star
        /// </summary>
        /// <param name="levelId">The id of the level</param>
        /// <param name="starIndex">The index of the star</param>
        /// <returns>The time the player needs to get the next star</returns>
        private float GetNextStarTime(string levelId, int starIndex)
        {
            float[] starsTimes = _defaultLevelsInformation[levelId].times;

            if (starIndex <= 3)
            {
                return starsTimes[3 - starIndex];
            }

            return 0f;
        }


        protected override async void TargetReached()
        {
            base.TargetReached();

            float time = _levelTimer.GetTime();
            float bestTime = PlayerManager.Instance.LevelDataManager.GetDefaultLevelBestTime(_currentLevelId);

            bool newBestTime = bestTime == 0f || time < bestTime;

            if (GameManager.DEBUG)
            {
                Debug.Log($"Completed level {_currentLevelId} in {time}. Previous PB: {bestTime}. New best? {newBestTime}");
            }

            // The number of stars the player already had before completing the level
            int numberOfStarsAlreadyGained = GetNumberOfStarsForLevel(_currentLevelId, bestTime);

            // Update the player's best time for the current level
            if (newBestTime)
            {
                // Save the time of the current level
                PlayerManager.Instance.LevelDataManager.SetDefaultLevelTime(_currentLevelId, time);
            }

            string nextLevelId = GetNextLevel();

            if (nextLevelId != null)
            {
                // Unlock the next level if it hasn't already been unlocked
                if (!PlayerManager.Instance.LevelDataManager.IsDefaultLevelUnlocked(nextLevelId))
                {
                    PlayerManager.Instance.LevelDataManager.UnlockDefaultLevel(nextLevelId);
                }
            }

            LevelCompletedView levelCompletedView = UIManager.Instance.UIViews[UIViewType.LevelCompleted] as LevelCompletedView;

            // The number of stars the player now has after completing the level
            int numberOfStars = GetNumberOfStarsForLevel(_currentLevelId, newBestTime ? time : bestTime);

            string levelCompletedSecondText;

            if (GameManager.DEBUG)
            {
                Debug.Log($"Player had {numberOfStarsAlreadyGained} stars. Now has {numberOfStars}");
            }

            // Update the second text depending on the amount of stars and the time the player got
            if (numberOfStars == 3)
            {
                // Update the player's best time for the current level
                if (newBestTime)
                {
                    levelCompletedSecondText = $"NEW BEST TIME: {time.ToString("00.00")}s";
                }
                else
                {
                    levelCompletedSecondText = $"BEST TIME: {bestTime.ToString("00.00")}s";
                }

                LevelEvents.NextStarTimeUpdated?.Invoke(null);
            }
            else
            {
                float nextStarTime = GetNextStarTime(_currentLevelId, 3 - numberOfStars);

                levelCompletedSecondText = $"NEXT STAR: {nextStarTime.ToString("00.00")}s";

                LevelEvents.NextStarTimeUpdated?.Invoke(nextStarTime);
            }

            // The number of stars the player has gained for completing the level
            int numberOfStarsGained = numberOfStars - numberOfStarsAlreadyGained;

            int coinsEarned = GetCoinsEarnedForLevel(numberOfStarsAlreadyGained, numberOfStars, _currentLevelId);

            // Update the amount of coins the player has
            if (coinsEarned > 0)
            {
                PlayerManager.Instance.CoinsManager.UpdateCoins(coinsEarned, LevelCompletedView.GetLevelCompletedAnimationDuration(numberOfStarsGained));
            }

            levelCompletedView.DisplayNewBestTimeFrame(newBestTime);

            await UniTask.Delay(TimeSpan.FromMilliseconds(TARGET_REACHED_UI_DELAY));

            (UIManager.Instance.UIViews[UIViewType.LevelCompleted] as LevelCompletedView).SetNextLevelButtonVisibility(nextLevelId != null);

            UIManager.Instance.Show(UIViewType.LevelCompleted);

            levelCompletedView.UpdateTime(_levelTimer.GetTime(), numberOfStarsAlreadyGained, numberOfStars, levelCompletedSecondText);
        }
    }
}