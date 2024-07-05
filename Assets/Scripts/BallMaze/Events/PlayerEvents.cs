using System;


namespace BallMaze.Events
{
    public static class PlayerEvents
    {
        // Called when the player's coins are updated.
        // The first parameter is the amount of coins the player has
        // and the second parameter is the new increment from the previous amount of coins (ie. the amount of coins the player just gained or lost)
        public static Action<int, int> CoinsUpdated;

        public static Action<string> DefaultLevelUnlocked;

        // Called when the player's best time for a level is updated.
        // This can happen when the player completes a level with a new best time
        // or simply when the player switches to a different level to update the UI.
        public static Action<string, float> DefaultLevelBestTimeUpdated;
    }
}