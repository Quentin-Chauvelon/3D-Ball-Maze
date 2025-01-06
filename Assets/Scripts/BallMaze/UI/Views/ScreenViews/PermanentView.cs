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
        private VisualElement _trophiesContainer;
        private Label _trophiesLabel;
        private VisualElement _coinsBackground;
        private Label _coinsLabel;
        private MainImageButton _skipButton;
        private MainImageButton _pauseButton;
        private MainImageButton _settingsButton;

        public const float UI_HEIGHT_PERCENTAGE = 0.15f;


        public PermanentView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _navigationContainer = _root.Q<VisualElement>("permanent__navigation-container");
            _backButton = _root.Q<Button>("permanent__back-button");
            _trophiesContainer = _root.Q<VisualElement>("permanent__trophies-container");
            _trophiesLabel = _root.Q<Label>("permanent__trophies-label");
            _coinsBackground = _root.Q<VisualElement>("permanent__coins-background");
            _coinsLabel = _root.Q<Label>("permanent__coins-value-label");
            _skipButton = _root.Q<MainImageButton>("permanent__skip-button");
            _pauseButton = _root.Q<MainImageButton>("permanent__pause-button");
            _settingsButton = _root.Q<MainImageButton>("permanent__settings-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            // Go back to the previous screen view
            _backButton.clickable.clicked += () => { UIManager.Instance.Back(); };

            // Open the settings modal view
            _settingsButton.Button.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.Settings); };

            _pauseButton.Button.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.Pause); };

            _skipButton.Button.clickable.clicked += () =>
            {
                // If the player is playing default levels, check if the next level is already unlocked.
                // If it's the case, load the next level, otherwise show the skip modal view
                if (LevelManager.Instance.levelType == LevelType.Default)
                {
                    string nextLevel = LevelManager.Instance.GetNextLevel();

                    if (!String.IsNullOrEmpty(nextLevel) && PlayerManager.Instance.DefaultLevelsDataManager.IsLevelUnlocked(nextLevel))
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
            return new Vector2(_coinsBackground.worldBound.xMin, _coinsBackground.worldBound.yMin);
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
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    _settingsButton.style.display = DisplayStyle.Flex;
                    break;

                case UIViewType.Playing:
                    _backButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.Flex;
                    _settingsButton.style.display = DisplayStyle.Flex;

                    // Show the skip button only for default
                    _skipButton.style.display = LevelManager.Instance.levelType == LevelType.Default
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;

                    break;

                case UIViewType.LevelCompleted:
                    _backButton.style.display = DisplayStyle.None;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    _settingsButton.style.display = DisplayStyle.None;

                    break;

                case UIViewType.SecondChance:
                    _backButton.style.display = DisplayStyle.None;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    _settingsButton.style.display = DisplayStyle.None;

                    break;

                case UIViewType.LevelFailed:
                    _backButton.style.display = DisplayStyle.None;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    _settingsButton.style.display = DisplayStyle.None;

                    break;

                default:
                    _backButton.style.display = DisplayStyle.Flex;
                    _skipButton.style.display = DisplayStyle.None;
                    _pauseButton.style.display = DisplayStyle.None;
                    _settingsButton.style.display = DisplayStyle.Flex;
                    break;
            }
        }
    }
}