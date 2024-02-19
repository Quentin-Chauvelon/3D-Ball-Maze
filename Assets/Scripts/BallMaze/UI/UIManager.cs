using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public enum UIViewType
    {
        Unknown,
        Permanent,
        Background,
        ModalBackground,
        MainMenu,
        Playing,
        ModeSelection,
        DefaultLevelSelection,
        DailyLevels,
        RankedLevel,
        Settings,
        DailyReward,
        LevelQuitConfirmation,
        GameQuitConfirmation,
        SecondChance,
        LevelFailed,
        LevelCompleted,
        Skip,
        Pause,
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

        // Stack to keep track of the navigation history (screens views are always at the bottom of the stack and modal views stack on top of it)
        private Stack<UIView> _navigationHistory = new Stack<UIView>();

        // Dictionary that associates all UIs with their name
        private Dictionary<UIViewType, UIView> _uiViews = new Dictionary<UIViewType, UIView>();
        // Public getter for the dictionary (read-only as we want other classes to be able to call methods on the UIs but not to modify the dictionary itself)
        public Dictionary<UIViewType, UIView> UIViews
        {
            get
            {
                return _uiViews;
            }
        }

        private bool _isModalOpened = false;


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
            // Special views
            _uiViews.Add(UIViewType.Permanent, new PermanentView(root.Q<VisualElement>("permanent")));
            _uiViews.Add(UIViewType.Background, new BackgroundView(root.Q<VisualElement>("background")));
            _uiViews.Add(UIViewType.ModalBackground, new ModalBackgroundView(root.Q<VisualElement>("modal-background")));

            // Screen views
            _uiViews.Add(UIViewType.MainMenu, new MainMenuView(root.Q<VisualElement>("main-menu")));
            _uiViews.Add(UIViewType.Playing, new PlayingView(root.Q<VisualElement>("playing")));
            _uiViews.Add(UIViewType.ModeSelection, new ModeSelectionView(root.Q<VisualElement>("mode-selection")));
            _uiViews.Add(UIViewType.DefaultLevelSelection, new DefaultLevelSelectionView(root.Q<VisualElement>("level-selection")));
            _uiViews.Add(UIViewType.DailyLevels, new DailyLevelsView(root.Q<VisualElement>("daily-levels")));
            _uiViews.Add(UIViewType.RankedLevel, new RankedLevelView(root.Q<VisualElement>("ranked-level")));

            // Closeable modal views
            _uiViews.Add(UIViewType.Settings, new SettingsView(root.Q<VisualElement>("settings")));
            _uiViews.Add(UIViewType.DailyReward, new DailyRewardView(root.Q<VisualElement>("daily-reward")));
            _uiViews.Add(UIViewType.LevelQuitConfirmation, new LevelQuitConfirmationView(root.Q<VisualElement>("level-quit-confirmation")));
            _uiViews.Add(UIViewType.GameQuitConfirmation, new GameQuitConfirmationView(root.Q<VisualElement>("game-quit-confirmation")));

            // Uncloseable modal views
            _uiViews.Add(UIViewType.NoInternet, new NoInternetView(root.Q<VisualElement>("no-internet")));
            _uiViews.Add(UIViewType.SecondChance, new SecondChanceView(root.Q<VisualElement>("second-chance-container")));
            _uiViews.Add(UIViewType.LevelFailed, new LevelFailedView(root.Q<VisualElement>("level-failed-container")));
            _uiViews.Add(UIViewType.LevelCompleted, new LevelCompletedView(root.Q<VisualElement>("level-completed-container")));
            _uiViews.Add(UIViewType.Skip, new SkipView(root.Q<VisualElement>("skip-container")));
            _uiViews.Add(UIViewType.Pause, new PauseView(root.Q<VisualElement>("pause-container")));

            // Show the UIs that should be shown at the start
            // For the permanent UI, don't call the UIManager.Show() method because it would add the permanent UI to the navigation history
            // and it shouldn't be the case as it should always be visible
            // For the other UIs, call the UIManager.Show() method to add them to the navigation history
            _uiViews[UIViewType.Permanent].Show();
            _uiViews[UIViewType.Background].Show();
            Show(_uiViews[UIViewType.MainMenu]);
        }


        /// <summary>
        /// Opens a new screen view and hides all other UIs.
        /// </summary>
        /// <param name="screenView">The component of the screen view to open</param>
        private void ShowScreenView(ScreenView screenView)
        {
            // Get the type of the UI view if it hasn't been passed given
            UIViewType uiView = GetUIViewType(screenView);

            if (uiView != UIViewType.Unknown)
            {
                // Update the game state based on the current UI view
                GameManager.Instance.UpdateGameState(uiView);

                // Update the visible elements of the permanent view
                (_uiViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(uiView);
            }

            // Hide all UIs
            HideScreenView();

            // If we are back at the main menu, clear the navigation history
            if (screenView == _uiViews[UIViewType.MainMenu])
            {
                _navigationHistory.Clear();
            }

            // If the screen view is not already in the navigation history, add it to the navigation history
            if (!_navigationHistory.Contains(screenView))
            {
                _navigationHistory.Push(screenView);
            }

            screenView.Show();
        }


        /// <summary>
        /// Hides all UIs.
        /// </summary>
        private void HideScreenView()
        {
            foreach (UIView uiView in _navigationHistory)
            {
                uiView.Hide();
            }

            // Remove the invisible button behind the modal view
            NoModalsOpened();
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
        private void Show(UIView uiView)
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
        public void Show(UIViewType uiView)
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
        private void Hide(UIView uiView)
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
        public void Hide(UIViewType uiView)
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
            // If the navigation history is empty, show the main menu
            if (_navigationHistory.Count == 0 || _navigationHistory.Peek() == null)
            {
                Show(UIViewType.MainMenu);
                return;
            }

            // if the top UI is an uncloseable modal view, don't go back
            if (_navigationHistory.Peek() is ModalView && !(_navigationHistory.Peek() as ModalView).isCloseable)
            {
                return;
            }

            // If the top UI is a screen view, hide it and show the previous screen view, otherwise hide the top UI
            if (!_navigationHistory.Peek().isModal)
            {
                _navigationHistory.Pop().Hide();

                if (_navigationHistory.Count > 0)
                {
                    ShowScreenView(_navigationHistory.Peek() as ScreenView);
                }
                else
                {
                    Show(UIViewType.MainMenu);
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
            _uiViews[UIViewType.ModalBackground].Show();
        }


        /// <summary>
        /// Called when the last modal view is hidden.
        /// Removes the invisible button behind the modal view.
        /// </summary>
        private void NoModalsOpened()
        {
            _isModalOpened = false;
            _uiViews[UIViewType.ModalBackground].Hide();
        }


        /// <summary>
        /// Returns the type from the UIViewType enum corresponding to the given UI view
        /// </summary>
        /// <param name="uiView"></param>
        /// <returns></returns>
        private UIViewType GetUIViewType(UIView uiView)
        {
            foreach (KeyValuePair<UIViewType, UIView> item in _uiViews)
            {
                if (item.Value == uiView)
                {
                    return item.Key;
                }
            }

            return UIViewType.Unknown;
        }
    }
}