using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace BallMaze
{
    public class ExceptionManager : MonoBehaviour
    {
        // Boolean to check if an exception is currently being displayed
        public static bool isError = false;


        /// <summary>
        /// Displays the given exception message
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="action">Defines what buttons to display to the user (by default go back to the main menu)</param>
        public static void ShowExceptionMessage(string message, ExceptionAction action = ExceptionAction.BackToMainMenu)
        {
            isError = true;
            Debug.Log(message);
            // TODO: UI to display message
            // TODO: add buttons according to the given action, when buttons are clicked, set isError to false
        }


        /// <summary>
        /// Possible actions to display to the player when an exception occurs
        /// </summary>
        public enum ExceptionAction
        {
            Resume,
            RestartGame,
            BackToMainMenu,
            BackToLevels
        }
    }
}