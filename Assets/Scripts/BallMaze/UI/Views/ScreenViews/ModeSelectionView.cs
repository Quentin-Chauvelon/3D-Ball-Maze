using System;
using System.Collections.Generic;
using BallMaze.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI {
    public class ModeSelectionView : ScreenView
    {
        private ScrollView _modeSelectionContainerScrollView;
        private Button _defaultLevelsButton;
        private Button _dailyLevelsButton;
        private Label _dailyLevelsTimer;
        private Button _rankedButton;
        private Label _rankedTimer;
        private Button _eventButton;
        private Label _eventTimer;


        public ModeSelectionView(VisualElement root) : base(root)
        {

        }

        protected override void SetVisualElements()
        {
            _modeSelectionContainerScrollView = _root.Q<ScrollView>("mode-selection__modes-container-scroll-view");
            _defaultLevelsButton = _root.Q<Button>("mode-selection__default-levels-button");
            _dailyLevelsButton = _root.Q<Button>("mode-selection__daily-levels-button");
            _dailyLevelsTimer = _dailyLevelsButton.Q<Label>("mode-selection__levels-button-subtext");
            _rankedButton = _root.Q<Button>("mode-selection__ranked-button");
            _rankedTimer = _rankedButton.Q<Label>("mode-selection__levels-button-subtext");
            _eventButton = _root.Q<Button>("mode-selection__event-button");
            _eventTimer = _eventButton.Q<Label>("mode-selection__levels-button-subtext");
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

            _dailyLevelsButton.clickable.clicked += () =>
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

        public override void Show()
        {
            base.Show();

            StartTimers();
        }


        private async void StartTimers()
        {
            DateTime midnightUtc = GameManager.Instance.GetUtcNowTime().MidnightUtc().AddMinutes(1);
            DateTime endOfWeekUtc = GameManager.Instance.GetUtcNowTime().EndOfWeek().AddMinutes(1);

            while (isEnabled)
            {
                // Calculate the time left until midnight utc
                TimeSpan dailyTimeLeft = midnightUtc - GameManager.Instance.GetUtcNowTime();
                TimeSpan weeklyTimeLeft = endOfWeekUtc - GameManager.Instance.GetUtcNowTime();

                _dailyLevelsTimer.text = $"{dailyTimeLeft.Hours}h {dailyTimeLeft.Minutes}m {dailyTimeLeft.Seconds}s";
                _rankedTimer.text = $"{weeklyTimeLeft.Days}d {weeklyTimeLeft.Hours}h {weeklyTimeLeft.Minutes}m {weeklyTimeLeft.Seconds}s";
                _eventTimer.text = $"{weeklyTimeLeft.Days}d {weeklyTimeLeft.Hours}h {weeklyTimeLeft.Minutes}m {weeklyTimeLeft.Seconds}s";

                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}