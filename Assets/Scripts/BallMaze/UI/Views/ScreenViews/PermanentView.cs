using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class PermanentView : ScreenView
    {
        // Visual Elements
        private VisualElement _navigationContainer;
        private Button _backButton;
        private Button _homeButton;
        private VisualElement _trophiesContainer;
        private Label _trophiesLabel;
        private VisualElement _coinsContainer;
        private Label _coinsLabel;
        private Button _moreCoinsButton;
        private Button _skipButton;
        private Button _pauseButton;
        private Button _settingsButton;


        public PermanentView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _navigationContainer = _root.Q<VisualElement>("permanent__navigation-container");
            _backButton = _root.Q<Button>("permanent__back-button");
            _homeButton = _root.Q<Button>("permanent__home-button");
            _trophiesContainer = _root.Q<VisualElement>("permanent__trophies-container");
            _trophiesLabel = _root.Q<Label>("permanent__trophies-label");
            _coinsContainer = _root.Q<VisualElement>("permanent__coins-container");
            _coinsLabel = _root.Q<Label>("permanent__coins-value-label");
            _moreCoinsButton = _root.Q<Button>("permanent__more-coins-button");
            _skipButton = _root.Q<Button>("permanent__skip-button");
            _pauseButton = _root.Q<Button>("permanent__pause-button");
            _settingsButton = _root.Q<Button>("permanent__settings-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            // Go back to the previous screen view
            _backButton.clickable.clicked += () => { UIManager.Instance.Back(); };

            // Go to the main menu
            _homeButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.MainMenu); };

            // Open the settings modal view
            _settingsButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.Settings); };

            _pauseButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.Pause); };

            _skipButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.Skip); };

            // Update the coins label when the player's coins are updated
            PlayerEvents.CoinsUpdated += (coins) => { _coinsLabel.text = coins.ToString(); };
        }


        public override void Show()
        {
            base.Show();
        }


        /// <summary>
        /// Updates the visible elements based on the screen view currently shown
        /// </summary>
        public void UpdateVisibleElements(UIViewType uiView)
        {
            switch (uiView)
            {
                case UIViewType.MainMenu:
                    _backButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.None;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    break;
                case UIViewType.Playing:
                    _backButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.Flex;

                    // Show the skip button only for default and daily levels
                    _skipButton.style.display = LevelManager.Instance.levelType == LevelType.Default || LevelManager.Instance.levelType == LevelType.DailyLevel
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;

                    break;
                default:
                    _backButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.Flex;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    break;
            }
        }
    }
}