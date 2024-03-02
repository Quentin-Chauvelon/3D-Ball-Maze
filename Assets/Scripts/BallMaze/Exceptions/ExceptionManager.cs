using System;
using BallMaze.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace BallMaze
{
    /// <summary>
    /// Possible actions to display to the player when an exception occurs
    /// </summary>
    public enum ExceptionActionType
    {
        Resume,
        RestartGame,
        BackToMainMenu,
        BackToLevels
    }


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

        // Boolean to check if an exception is currently being displayed
        public static bool isError = false;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }


        /// <summary>
        /// Displays the given exception message
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="action">Defines what buttons to display to the user. Default: BackToMainMenu</param>
        public static void ShowExceptionMessage(Exception e, string message, ExceptionActionType action = ExceptionActionType.BackToMainMenu)
        {
            isError = true;
            Debug.Log(message);

            UIManager.Instance.UIViews[UIViewType.Exception].Show();
        }



        /// <summary>
        /// Displays the exception message from the given localization table and key
        /// </summary>
        /// <param name="table">The localization table to use</param>
        /// <param name="key">The localization key of the message</param>
        /// <param name="action">Defines what buttons to display to the user. Default: BackToMainMenu</param>
        public static void ShowExceptionMessage(Exception e, string table, string key, ExceptionActionType action = ExceptionActionType.BackToMainMenu)
        {
            ShowExceptionMessage(e, LocalizationSettings.StringDatabase.GetLocalizedString(table, key), action);
        }


        /// <summary>
        /// Executes the action that should be executed based on the exception once the button is clicked
        /// </summary>
        public void ActionButtonClicked()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Sends the current error to the support with the given additional information
        /// </summary>
        /// <param name="additionalInformation"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SendErrorToSupport(string additionalInformation)
        {
            throw new NotImplementedException();
        }
    }
}