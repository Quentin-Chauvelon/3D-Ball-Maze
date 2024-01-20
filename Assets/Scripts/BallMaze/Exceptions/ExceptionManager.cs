using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace BallMaze
{
    public class ExceptionManager : MonoBehaviour
    {
        // Singleton pattern
        private static ExceptionManager _instance;
        public static ExceptionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("ExceptionManager is null!");
                }
                return _instance;
            }
        }


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }


        /// <summary>
        /// Displays the given exception message
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="action">Defines what buttons to display to the user (by default go back to the main menu)</param>
        public void ShowExceptionMessage(string message, ExceptionAction action = ExceptionAction.BackToMainMenu)
        {
            Debug.Log(message);
            // TODO: UI to display message
            // TODO: add buttons according to the given action
        }


        /// <summary>
        /// Possible actions to display to the player when an exception occurs
        /// </summary>
        public enum ExceptionAction
        {
            BackToMainMenu,
            BackToLevels
        }
    }
}