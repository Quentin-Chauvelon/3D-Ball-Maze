using System.Collections.Generic;
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


    public static void ResetDailyLevels()
    {
        _dailyLevels = null;
    }
}
