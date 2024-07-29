using System;
using System.IO;
using BallMaze;
using BallMaze.Events;
using BallMaze.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DailyLevelsLevelManager : LevelManagerBase
{
    public override LevelType levelType => LevelType.DailyLevel;

    public override string LEVELS_PATH
     => Path.Combine(Application.persistentDataPath, "dailyLevels");

    protected override bool _isSecondChanceEnabled => true;

    private static Level[] _dailyLevels;

    // The day the player last completed a daily level. Used when saving locally to a file
    public static int LastDailyLevelsPlayedDay;


    public DailyLevelsLevelManager()
    {
        ResetLevelManager();
    }


    public static DailyLevelDifficulty GetDailyLevelDifficultyFromId(string levelId)
    {
        switch (levelId)
        {
            case "DailyLevelVeryEasy":
                return DailyLevelDifficulty.VeryEasy;
            case "DailyLevelEasy":
                return DailyLevelDifficulty.Easy;
            case "DailyLevelMedium":
                return DailyLevelDifficulty.Medium;
            case "DailyLevelHard":
                return DailyLevelDifficulty.Hard;
            case "DailyLevelExtreme":
                return DailyLevelDifficulty.Extreme;
            default:
                return DailyLevelDifficulty.Unknown;
        }
    }


    public static string GetDailyLevelIdFromDifficulty(DailyLevelDifficulty difficulty)
    {
        switch (difficulty)
        {
            case DailyLevelDifficulty.VeryEasy:
                return "DailyLevelVeryEasy";
            case DailyLevelDifficulty.Easy:
                return "DailyLevelEasy";
            case DailyLevelDifficulty.Medium:
                return "DailyLevelMedium";
            case DailyLevelDifficulty.Hard:
                return "DailyLevelHard";
            case DailyLevelDifficulty.Extreme:
                return "DailyLevelExtreme";
            default:
                return null;
        }
    }


    public override void LoadLevel(string levelId)
    {
        base.LoadLevel(levelId);

        float bestTime = PlayerManager.Instance.DailyLevelsDataManager.GetLevelBestTime(_currentLevelId);

        if (GameManager.DEBUG)
        {
            Debug.Log($"Loaded level {_currentLevelId}. Best time: {bestTime}");
        }

        DailyLevelDifficulty difficulty = GetDailyLevelDifficultyFromId(_currentLevelId);

        // Update the UI to match the level information
        LevelEvents.LevelNameUpdated?.Invoke(GetLevelFromDifficulty(difficulty).name);
        LevelEvents.BestTimeUpdated?.Invoke(bestTime);

        int numberOfStars = GetNumberOfStarsForLevel(_currentLevelId, bestTime);

        LevelEvents.NextStarTimeUpdated?.Invoke(numberOfStars < 3
            ? GetNextStarTime(difficulty, 3 - numberOfStars)
            : null
        );
    }


    /// <summary>
    /// Populate the daily levels with the levels fetched from the server and update the UI
    /// </summary>
    /// <param name="dailyLevels"></param>
    public static void PopulateDailyLevels(Level[] dailyLevels)
    {
        _dailyLevels = dailyLevels;

        // Reset the player's times and unlocked levels if it's a new day (00:01)
        if (GameManager.Instance.GetUtcNowTime().Hour == 0 && GameManager.Instance.GetUtcNowTime().Minute == 1)
        {
            PlayerManager.Instance.DailyLevelsDataManager.dailyLevelsTimes.Clear();
            PlayerManager.Instance.DailyLevelsDataManager.dailyLevelsUnlocked.Clear();
            PlayerManager.Instance.DailyLevelsDataManager.dailyLevelsUnlocked.Add("DailyLevelVeryEasy");
        }

        if (GameManager.DEBUG)
        {
            Debug.Log("Daily levels populated");
        }

        (UIManager.Instance.UIViews[UIViewType.DailyLevels] as DailyLevelsView).PopulateLevels(dailyLevels);
    }


    public override int GetNumberOfStarsForLevel(string levelId, float? bestTime = null)
    {
        bestTime ??= PlayerManager.Instance.DailyLevelsDataManager.GetLevelBestTime(levelId);
        DailyLevelDifficulty difficulty = GetDailyLevelDifficultyFromId(levelId);

        if (bestTime.Value > 0f && (int)difficulty - 1 < _dailyLevels.Length)
        {
            float[] starsTimes = GetLevelFromDifficulty(difficulty).times;

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
        if (string.IsNullOrEmpty(levelId))
        {
            levelId = _currentLevelId;
        }

        int coinsEarned = 0;
        int increment = ((int)GetDailyLevelDifficultyFromId(levelId) - 1) * 5;

        for (int i = starsAlreadygained; i < starsGained; i++)
        {
            coinsEarned += 10 + increment + 10 * i;
        }

        return coinsEarned;
    }


    /// <summary>
    /// Get the time the player needs to get the next star
    /// </summary>
    /// <param name="levelId">The id of the level</param>
    /// <param name="starIndex">The index of the star</param>
    /// <returns>The time the player needs to get the next star</returns>
    private float GetNextStarTime(DailyLevelDifficulty difficulty, int starIndex)
    {
        float[] starsTimes = GetLevelFromDifficulty(difficulty).times;

        if (starIndex <= 3)
        {
            return starsTimes[3 - starIndex];
        }

        return 0f;
    }


    public static Level GetLevelFromDifficulty(DailyLevelDifficulty difficulty)
    {
        return _dailyLevels[(int)difficulty - 1];
    }


    public override string GetNextLevel()
    {
        DailyLevelDifficulty difficulty = GetDailyLevelDifficultyFromId(_currentLevelId);
        DailyLevelDifficulty nextDifficulty = difficulty + 1;

        if (nextDifficulty <= DailyLevelDifficulty.Extreme)
        {
            return GetLevelFromDifficulty(nextDifficulty).id;
        }

        return null;
    }


    protected override async void TargetReached()
    {
        base.TargetReached();

        float time = _levelTimer.GetTime();
        float bestTime = PlayerManager.Instance.DailyLevelsDataManager.GetLevelBestTime(_currentLevelId);
        DailyLevelDifficulty difficulty = GetDailyLevelDifficultyFromId(_currentLevelId);
        DailyLevelDifficulty nextDifficulty = difficulty + 1;

        bool newBestTime = bestTime == 0f || time < bestTime;

        if (GameManager.DEBUG)
        {
            Debug.Log($"Completed daily level {_currentLevelId} in {time}. Previous PB: {bestTime}. New best? {newBestTime}");
        }

        // The number of stars the player already had before completing the level
        int numberOfStarsAlreadyGained = GetNumberOfStarsForLevel(_currentLevelId, bestTime);

        // Update the player's best time for the current level
        if (newBestTime)
        {
            // Save the time of the current level
            PlayerManager.Instance.DailyLevelsDataManager.SetLevelTime(_currentLevelId, time);

            LevelEvents.DailyLevelBestTimeUpdated?.Invoke(_currentLevelId, time);

            LastDailyLevelsPlayedDay = GameManager.Instance.GetUtcNowTime().DayOfYear;
        }

        string nextLevelId = GetNextLevel();

        if (nextLevelId != null)
        {
            // Unlock the next level if it hasn't already been unlocked
            if (!PlayerManager.Instance.DailyLevelsDataManager.IsLevelUnlocked(nextLevelId))
            {
                PlayerManager.Instance.DailyLevelsDataManager.UnlockLevel(nextLevelId);
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
            float nextStarTime = GetNextStarTime(difficulty, 3 - numberOfStars);

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


    public static void ResetDailyLevels()
    {
        _dailyLevels = null;
    }
}
