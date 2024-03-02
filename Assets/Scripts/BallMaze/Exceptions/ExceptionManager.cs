using System;
using System.Collections.Generic;
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


    /// <summary>
    /// Class that represents an action that will be shown to the user when an exception occurs
    /// </summary>
    public class ExceptionAction
    {
        public string name;
        public int priority;
        public Action clickCallback;
    }


    /// <summary>
    /// Class taht represents an exception object containing all the information about the exception
    /// </summary>
    public class ExceptionObject
    {
        public string friendlyMessage;
        public string message;
        public string stackTrace;
        public ExceptionAction action;
        public bool sentToSupport;
        public DateTime date;
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

        private ExceptionObject _currentException;

        private Dictionary<ExceptionActionType, ExceptionAction> _actions = new Dictionary<ExceptionActionType, ExceptionAction>
        {
            {
                ExceptionActionType.Resume,
                new ExceptionAction {
                    name = "Resume",
                    priority = 1,
                    clickCallback = () => {  }
                }
            },
            {
                ExceptionActionType.RestartGame,
                new ExceptionAction {
                    name = "Restart Game",
                    priority = 100,
                    clickCallback = () => { throw new NotImplementedException(); }
                }
            },
            {
                ExceptionActionType.BackToMainMenu,
                new ExceptionAction {
                    name = "Back to Main Menu",
                    priority = 80,
                    clickCallback = () => { UIManager.Instance.UIViews[UIViewType.MainMenu].Show(); }
                }
            },
            {
                ExceptionActionType.BackToLevels,
                new ExceptionAction {
                    name = "Back to Levels",
                    priority = 50,
                    clickCallback = () => { UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection].Show(); }
                }
            }
        };


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

            // If the new exception has a lower priority than the current one, don't display it
            if (Instance._currentException != null && (Instance._actions[action].priority <= Instance._currentException.action.priority))
            {
                return;
            }

            Instance._currentException = new ExceptionObject
            {
                friendlyMessage = string.IsNullOrEmpty(message) ? e.Message : message,
                message = message,
                stackTrace = e.StackTrace,
                action = Instance._actions[action],
                sentToSupport = false,
                date = DateTime.Now
            };

            (UIManager.Instance.UIViews[UIViewType.Exception] as ExceptionView).UpdateException(Instance._currentException);
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
            isError = false;

            _currentException.action.clickCallback();
        }


        /// <summary>
        /// Sends the current error to the support with the given additional information
        /// </summary>
        /// <param name="additionalInformation"></param>
        public void SendErrorToSupport(string additionalInformation)
        {
            // If the exception has already been sent to the support, don't send it again
            if (_currentException.sentToSupport)
            {
                return;
            }

            _currentException.sentToSupport = true;

            throw new NotImplementedException("SendErrorToSupport via email");
        }
    }
}