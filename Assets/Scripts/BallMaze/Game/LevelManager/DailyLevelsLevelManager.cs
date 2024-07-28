using System.IO;
using BallMaze;
using BallMaze.UI;
using UnityEngine;

public class DailyLevelsLevelManager : LevelManagerBase
{
    public override LevelType levelType => LevelType.DailyLevel;

    public override string LEVELS_PATH => Path.Combine(Application.persistentDataPath, "dailyLevels");

    protected override bool _isSecondChanceEnabled => true;

    private static Level[] _dailyLevels;


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


    /// <summary>
    /// Populate the daily levels with the levels fetched from the server and update the UI
    /// </summary>
    /// <param name="dailyLevels"></param>
    public static void PopulateDailyLevels(Level[] dailyLevels)
    {
        _dailyLevels = dailyLevels;

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
            float[] starsTimes = _dailyLevels[(int)difficulty - 1].times;

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


    public static void ResetDailyLevels()
    {
        _dailyLevels = null;
    }
}
