using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.UI
{
    public class UIManager : MonoBehaviour
    {
        // Singleton pattern
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("UIManager is null!");
                }
                return _instance;
            }
        }

        // Stack to keep track of the navigation history (a mainUI is always at the bottom of the stack and popups stack on top of it)
        private static Stack<UIControl> _navigationHistory = new Stack<UIControl>();

        // Dictionary that associates all UIs with their name
        private static Dictionary<string, UIControl> _uiControls = new Dictionary<string, UIControl>();

        /// <summary>
        /// Registers a new UI control.
        /// </summary>
        /// <param name="ui"></param>
        public static void RegisterUIControl(UIControl ui)
        {
            if (ui != null && !_uiControls.ContainsKey(ui.name))
            {
                _uiControls.Add(ui.name, ui);
            }

        }


        /// <summary>
        /// Unregisters a UI control.
        /// </summary>
        /// <param name="ui"></param>
        public static void UnregisterUIControl(UIControl ui)
        {
            if (ui != null)
            {
                _uiControls.Remove(ui.name);
            }

        }


        /// <summary>
        /// Opens a new main UI and closes all other UIs.
        /// </summary>
        /// <param name="mainUI">The component of the main UI to open</param>
        public static void OpenMainUI(MainUIControl mainUI)
        {
            CloseMainUI();

            mainUI.Show();
        }


        /// <summary>
        /// Opens a new main UI and closes all other UIs.
        /// </summary>
        /// <param name="name">The name of the main UI to open</param>
        public static void OpenMainUI(string name)
        {
            if (_uiControls.ContainsKey(name))
            {
                OpenMainUI(_uiControls[name] as MainUIControl);
            }
        }


        /// <summary>
        /// Closes all UIs.
        /// </summary>
        public static void CloseMainUI()
        {
            foreach (UIControl ui in _navigationHistory)
            {
                ui.Hide();
            }

            // Remove the invisible button behind the popup
            PopUpUIControl.NoPopUpsOpened();

            _navigationHistory.Clear();
        }


        /// <summary>
        /// Opens a new popup and adds it to the navigation history.
        /// </summary>
        /// <param name="popUp">The component of the pop-up to open</param>
        public static void OpenPopUp(PopUpUIControl popUp)
        {
            // If the popup is the first popup to be opened, add the invisible button behind the popup
            if (_navigationHistory.Count == 1)
            {
                PopUpUIControl.PopUpOpened();
            }

            _navigationHistory.Push(popUp);
            popUp.Show();
        }


        /// <summary>
        /// Opens a new popup and adds it to the navigation history.
        /// </summary>
        /// <param name="name">The name of the pop-up to open</param>
        public static void OpenPopUp(string name)
        {
            if (_uiControls.ContainsKey(name))
            {
                OpenPopUp(_uiControls[name] as PopUpUIControl);
            }
        }


        /// <summary>
        /// Closes the top popup and removes it from the navigation history.
        /// </summary>
        /// <param name="popUp">The component of the pop-up to close</param>
        public static void ClosePopUp(PopUpUIControl popUp)
        {
            // If the popup the player is trying to close is not the top popup, close all popups on top of it
            while (_navigationHistory.Peek() != popUp)
            {
                _navigationHistory.Pop().Hide();
            }

            _navigationHistory.Pop();
            popUp.Hide();

            if (_navigationHistory.Count == 1)
            {
                // Remove the invisible button behind the popup
                PopUpUIControl.NoPopUpsOpened();
            }
        }


        /// <summary>
        /// Closes the top popup and removes it from the navigation history.
        /// </summary>
        /// <param name="name">The name of the pop-up to close</param>
        public static void ClosePopUp(string name)
        {
            if (_uiControls.ContainsKey(name))
            {
                ClosePopUp(_uiControls[name] as PopUpUIControl);
            }
        }


        /// <summary>
        /// Closes the top UI to go back to the previous opened UI.
        /// </summary>
        public static void Back()
        {
            if (_navigationHistory.Count >= 1)
            {
                // If the top UI is a popup, close it
                if (_navigationHistory.Peek() is PopUpUIControl)
                {
                    ClosePopUp(_navigationHistory.Peek() as PopUpUIControl);
                }
                // If the top UI is a main UI, close all UIs
                else if (_navigationHistory.Peek() is MainUIControl)
                {
                    CloseMainUI();
                }
            }
        }
    }
}