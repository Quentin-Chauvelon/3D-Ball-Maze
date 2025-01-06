using System;
using System.Collections.Generic;
using BallMaze.Events;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI {
    public class ModeSelectionView : ScreenView
    {
        private ScrollView _modeSelectionContainerScrollView;
        private Button _defaultLevelsButton;
        private Button _dailylevelsButton;
        private Button _rankedButton;
        private Button _eventButton;


        public ModeSelectionView(VisualElement root) : base(root)
        {

        }

        protected override void SetVisualElements()
        {
            _modeSelectionContainerScrollView = _root.Q<ScrollView>("mode-selection__modes-container-scroll-view");
            _defaultLevelsButton = _root.Q<Button>("mode-selection__default-levels-button");
            _dailylevelsButton = _root.Q<Button>("mode-selection__daily-levels-button");
            _rankedButton = _root.Q<Button>("mode-selection__ranked-button");
            _eventButton = _root.Q<Button>("mode-selection__event-button");
        }

        protected override void RegisterButtonCallbacks()
        {
            _defaultLevelsButton.clickable.clicked += () =>
            {
                LevelEvents.LevelModeUpdated?.Invoke(LevelType.Default);

                // We are playing classic, so disable the event UI
                (UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).SetEventModeSelected(false);
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
            };

            _dailylevelsButton.clickable.clicked += () =>
            {
                LevelEvents.LevelModeUpdated?.Invoke(LevelType.DailyLevel);

                UIManager.Instance.Show(UIViewType.DailyLevels);
            };

            _rankedButton.clickable.clicked += () =>
            {
                LevelEvents.LevelModeUpdated?.Invoke(LevelType.RankedLevel);

                UIManager.Instance.Show(UIViewType.RankedLevel);
            };

            _eventButton.clickable.clicked += () =>
            {
                LevelEvents.LevelModeUpdated?.Invoke(LevelType.Default);

                // We are playing event, so enable the event UI
                (UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).SetEventModeSelected(true);
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
            };
        }
    }
}