using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using DG.Tweening;
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
        private VisualElement _coinsImage;
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
            _coinsImage = _root.Q<VisualElement>("permanent__coins-image");
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

            _skipButton.clickable.clicked += () =>
            {
                // If the player is playing default levels, check if the next level is already unlocked.
                // If it's the case, load the next level, otherwise show the skip modal view
                if (LevelManager.Instance.levelType == LevelType.Default)
                {
                    string nextLevel = LevelManager.Instance.GetNextLevel();

                    if (!String.IsNullOrEmpty(nextLevel) && PlayerManager.Instance.LevelDataManager.IsDefaultLevelUnlocked(nextLevel))
                    {
                        LevelManager.Instance.LoadLevel(nextLevel);

                        return;
                    }
                }

                UIManager.Instance.Show(UIViewType.Skip);
            };

            // Update the coins label when the player's coins are updated
            PlayerEvents.CoinsUpdated += (coins, increment) =>
            {
                _coinsLabel.text = coins.ToString();
                DOTween.To(() => coins - increment, x => _coinsLabel.text = x.ToString(), coins, 0.5f);
            };
        }


        public override void Show()
        {
            base.Show();
        }


        /// <summary>
        /// Returns the position of the coins image
        /// </summary>
        /// <returns>A vector 2 representing the position in pixels of the coin image from the top and left edges</returns>
        public Vector2 GetCoinsImagePosition()
        {
            return new Vector2(_coinsImage.worldBound.xMin, _coinsImage.worldBound.yMin);
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