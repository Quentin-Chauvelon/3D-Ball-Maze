using System;
using System.Net.NetworkInformation;


namespace BallMaze.Events
{
    public static class LevelEvents
    {
        // Called when the name of the level is updated.
        // Usually happens when the player switches level
        public static Action<string> LevelNameUpdated;

        // Called when the difficulty of the daily level is updated.
        // Usually happens when the player switches level
        public static Action<DailyLevelDifficulty> DailyLevelDifficultyUpdated;

        // Called whenever the best time has been updated.
        // Can happen when the player sets a new best time or simply when switching level
        public static Action<float> BestTimeUpdated;

        // Called when the player's best time for a level is updated.
        // This can happen when the player completes a level with a new best time
        // or simply when the player switches to a different level to update the UI.
        public static Action<string, float> DefaultLevelBestTimeUpdated;

        // Called when the player's best time for a daily level is updated.
        // This can happen when the player completes a level with a new best time
        // or simply when the player switches to a different level to update the UI.
        public static Action<string, float> DailyLevelBestTimeUpdated;

        // Called when the next star time is updated.
        // Usually happens when the player completes a level with a new best time
        public static Action<float?> NextStarTimeUpdated;
    }
}