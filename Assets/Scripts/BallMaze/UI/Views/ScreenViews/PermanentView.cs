using System.Collections;
using System.Collections.Generic;
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
            _coinsLabel = _root.Q<Label>("permanent__coins-label");
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
                    _skipButton.style.display = DisplayStyle.Flex;
                    _pauseButton.style.display = DisplayStyle.Flex;
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