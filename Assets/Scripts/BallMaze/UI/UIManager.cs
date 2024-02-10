using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public enum UIViews
    {
        MainMenu,
        DefaultLevelSelection,
        Permanent,
        ModalBackground,
        Settings,
        NoInternet
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
            _uiViews.Add(UIViews.MainMenu, new MainMenuView(root.Q<VisualElement>("main-menu")));
            _uiViews.Add(UIViews.DefaultLevelSelection, new DefaultLevelSelectionView(root.Q<VisualElement>("level-selection")));
            _uiViews.Add(UIViews.NoInternet, new NoInternetView(root.Q<VisualElement>("no-internet")));

            // Show the UIs that should be shown at the start
            // For the permanent UI, don't call the UIManager.Show() method because it would add the permanent UI to the navigation history
            // and it shouldn't be the case as it should always be visible
            // For the other UIs, call the UIManager.Show() method to add them to the navigation history
            _uiViews[UIViews.Permanent].Show();
            Show(_uiViews[UIViews.MainMenu]);
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
            // If the modal view is not closeable, don't add it to the navigation history
            if (!modalView.isCloseable)
            {
                modalView.Show();
                return;
            }

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
            // If the modal view is not closeable, don't try to remove it from the navigation history and simply hide it.
            // Not closeable doesn't mean that it can't be hidden, it just means that it can't be closed by the user
            if (!modalView.isCloseable)
            {
                modalView.Hide();
                return;
            }

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
            // If the navigation history is empty, show the previous screen view
            if (_navigationHistory.Count == 0 || _navigationHistory.Peek() == null)
            {
                ShowScreenView(_previousScreenView);
                return;
            }

            // if the top UI is a uncloseable modal view, don't go back
            if (_navigationHistory.Peek() is ModalView && !(_navigationHistory.Peek() as ModalView).isCloseable)
            {
                return;
            }

            // If the top UI is a screen view, hide it and show the previous screen view, otherwise hide the top UI
            if (!_navigationHistory.Peek().isModal)
            {
                // We have to keep a reference to the previous screen view because calling Hide() will change the value of _previousScreenView (it will be _currentScreenView)
                ScreenView previousScreenView = _previousScreenView;

                Hide(_navigationHistory.Peek());

                if (previousScreenView != null)
                {
                    ShowScreenView(previousScreenView);
                }
            }
            else
            {
                Hide(_navigationHistory.Peek());
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
        /// Called when the last modal view is hidden.
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


        /// <summary>
        /// Populate the default level selection view with the given levels selection
        /// </summary>
        /// <param name="levelsSelection"></param>
        public void PopulateLevelSelectionView(LevelsSelection levelsSelection)
        {
                (_uiViews[UIViews.DefaultLevelSelection] as DefaultLevelSelectionView).PopulateLevelSelectionView(levelsSelection);
            }


        /// <summary>
        /// Checks if the default level selection view contains the levels
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultLevelSelectionViewLoaded()
        {
            return (_uiViews[UIViews.DefaultLevelSelection] as DefaultLevelSelectionView).IsDefaultLevelSelectionViewLoaded();
        }


        /// <summary>
        /// Displays the no internet UI based on the given internet availability.
        /// If a callback method is passed, it will be called when the player goes back online.
        /// </summary>
        /// <param name="internetAvailable">True if the player is online and the UI should be hidden, false otherwise</param>
        /// <param name="callback">The callback method to call when the player goes back online</param>
        public void DisplayNoInternetUI(bool internetAvailable, Action callback = null)
        {
            (_uiViews[UIViews.NoInternet] as NoInternetView).DisplayNoInternetUI(internetAvailable, callback);
        }
    }
}