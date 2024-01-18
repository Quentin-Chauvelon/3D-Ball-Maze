using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public enum UIViews
    {
        Permanent,
        ModalBackground,
        Settings
    }



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

        // The main UI document contains all the other documents (disabled) so that they can all be retrieved using root.Q<>() easily)
        // This is easier to manage than having the documents attached to different game objects and getting references to all of them
        private UIDocument _mainUIDocument;

        // Stack to keep track of the navigation history (a screen view is always at the bottom of the stack and modal views stack on top of it)
        private Stack<UIView> _navigationHistory = new Stack<UIView>();

        // Dictionary that associates all UIs with their name
        private Dictionary<UIViews, UIView> _uiViews = new Dictionary<UIViews, UIView>();

        private bool _isModalOpened = false;

        private ScreenView _previousScreenView;
        private ScreenView _currentScreenView;

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _mainUIDocument = GetComponent<UIDocument>();

            SetupViews();
        }


        private void OnDestroy()
        {
            foreach (UIView uiView in _uiViews.Values)
            {
                uiView.Dispose();
            }
        }


        private void SetupViews()
        {
            VisualElement root = _mainUIDocument.rootVisualElement;

            // Add all UIs to the uiViews dictionary
            _uiViews.Add(UIViews.Permanent, new PermanentView(root.Q<VisualElement>("permanent")));
            _uiViews.Add(UIViews.ModalBackground, new ModalBackgroundView(root.Q<VisualElement>("modal-background")));
            _uiViews.Add(UIViews.Settings, new SettingsView(root.Q<VisualElement>("settings")));

            // Show the UIs that should be shown at the start
            _uiViews[UIViews.Permanent].Show();
        }


        /// <summary>
        /// Opens a new screen view and hides all other UIs.
        /// </summary>
        /// <param name="screenView">The component of the screen view to open</param>
        private void ShowScreenView(ScreenView screenView)
        {
            HideScreenView();

            _currentScreenView = screenView;

            _navigationHistory.Push(screenView);
            screenView.Show();
        }


        /// <summary>
        /// Hides all UIs.
        /// </summary>
        private void HideScreenView()
        {
            _previousScreenView = _currentScreenView;

            foreach (UIView ui in _navigationHistory)
            {
                ui.Hide();
            }

            // Remove the invisible button behind the modal view
            NoModalsOpened();

            _navigationHistory.Clear();
        }


        /// <summary>
        /// Opens a new modal view and adds it to the navigation history.
        /// </summary>
        /// <param name="modalView">The component of the modal view to open</param>
        private void ShowModalView(ModalView modalView)
        {
            // If the modal view is the first modal view to be opened, add the invisible button behind the modal view
            if (!_isModalOpened)
            {
                ModalOpened();
            }

            _navigationHistory.Push(modalView);
            modalView.Show();
        }


        /// <summary>
        /// Hides the top modal view and removes it from the navigation history.
        /// </summary>
        /// <param name="modalView">The component of the modal view to hide</param>
        private void HideModalView(ModalView modalView)
        {
            // If the modal view the player is trying to hide is not the top modal view, hide all modal views on top of it
            while (_navigationHistory.Count > 0 && _navigationHistory.Peek() as ModalView != modalView)
            {
                _navigationHistory.Pop().Hide();
            }

            _navigationHistory.Pop();
            modalView.Hide();

            // If the stack is empty or if the top UI is not a modal view, remove the invisible button behind the modal view
            if (_navigationHistory.Count == 0 || !(_navigationHistory.Peek() is ModalView))
            {
                // Remove the invisible button behind the modal view
                NoModalsOpened();
            }
        }


        /// <summary>
        /// Opens the given uiView
        /// </summary>
        /// <param name="uiView">The view to open</param>
        public void Show(UIView uiView)
        {
            if (!uiView.isModal)
            {
                ShowScreenView(uiView as ScreenView);
            }
            else
            {
                ShowModalView(uiView as ModalView);
            }
        }


        /// <summary>
        /// Opens the given uiView
        /// </summary>
        /// <param name="uiView">The name of the view to open</param>
        public void Show(UIViews uiView)
        {
            if (_uiViews.ContainsKey(uiView))
            {
                Show(_uiViews[uiView]);
            }
        }


        /// <summary>
        /// Closes the given uiView
        /// </summary>
        /// <param name="uiView">The view to hide</param>
        public void Hide(UIView uiView)
        {
            if (!uiView.isModal)
            {
                HideScreenView();
            }
            else
            {
                HideModalView(uiView as ModalView);
            }
        }

        /// <summary>
        /// Closes the given uiView
        /// </summary>
        /// <param name="uiView">The name of the view to hide</param>
        public void Hide(UIViews uiView)
        {
            if (_uiViews.ContainsKey(uiView))
            {
                Hide(_uiViews[uiView]);
            }
        }


        /// <summary>
        /// Hides the top UI to go back to the previous opened UI.
        /// </summary>
        public void Back()
        {
            if (_navigationHistory.Count > 0)
            {
                Hide(_navigationHistory.Peek());
            }
            else
            {
                ShowScreenView(_previousScreenView);
            }
        }


        /// <summary>
        /// Called when the first modal view is opened.
        /// Adds an invisible button behind the modal view to detect clicks outside of the modal view (to hide the more easily).
        /// The same button is used for all modal views and the last modal view to be opened is the one that will be hided when the button is clicked.
        /// </summary>
        private void ModalOpened()
        {
            _isModalOpened = true;
            _uiViews[UIViews.ModalBackground].Show();
        }


        /// <summary>
        /// Called when the last modal view is hided.
        /// Removes the invisible button behind the modal view.
        /// </summary>
        private void NoModalsOpened()
        {
            _isModalOpened = false;
            _uiViews[UIViews.ModalBackground].Hide();
        }


        public void UpdateSettings()
        {
            (_uiViews[UIViews.Settings] as SettingsView).Update();
        }
    }
}