using System;


namespace BallMaze.Events
{
    public static class PlayerEvents
    {
        public static Action<int> CoinsUpdated;

        public static Action<string> DefaultLevelUnlocked;

        // Called when the player's best time for a level is updated.
        // This can happen when the player completes a level with a new best time
        // or simply when the player switches to a different level to update the UI.
        public static Action<string, float> DefaultLevelBestTimeUpdated;
    }
}