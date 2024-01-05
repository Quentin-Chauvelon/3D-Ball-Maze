using System;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Interface to be implemented by all obstacles that can be triggered (targets, kill parts...).
    /// </summary>
    public interface ITrigerrable
    {
        /// <summary>
        /// Observer pattern:
        /// Action to invoke when the trigger is entered to notify the observers (LevelManager).
        /// </summary>
        Action triggerAction { get; set; }

        /// <summary>
        /// Method to be called when the trigger is entered.
        /// Shouldn't handle any of the game's logic as it is handled by the observers.
        /// Only notifies the observers and eventually plays some animation, sound...
        /// </summary>
        void TriggerEntered();
    }
}