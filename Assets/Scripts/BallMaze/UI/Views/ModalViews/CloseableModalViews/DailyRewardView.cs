using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SearchService;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class DailyRewardView : ModalView
    {
        public override bool isCloseable => true;


        private short _rewardDayToCollect = 1;

        // Visual Elements
        private Button _closeButton;
        private VisualElement _dailyRewardDaysButtonsContainer;
        private Button _dailyRewardDayButton;
        private Action _dailyRewardDayClickAction;
        private Label _dailyRewardInfoLabel;
        private Button _claimRewardButton;



        public DailyRewardView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _dailyRewardDaysButtonsContainer = _root.Q<VisualElement>("daily-reward__rewards-container");

            _dailyRewardDayButton = _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{_rewardDayToCollect}-reward").Q<Button>("daily-reward-day-template__button-container");

            // Change the text for each reward to match the day
            for (int i = 1; i < 8; i++)
            {
                _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{i}-reward").Q<Label>("daily-reward-day-template__status-label").text = $"DAY {i.ToString()}";
            }

            _dailyRewardInfoLabel = _root.Q<Label>("daily-reward__info-label");
            _claimRewardButton = _root.Q<Button>("daily-reward__claim-reward-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clicked += () => { UIManager.Instance.Hide(UIViewType.DailyReward); };

            BindDailyRewardDayButtonClick();

            _claimRewardButton.clicked += () => { DailyRewardDayClicked(_rewardDayToCollect); };
        }


        public override void Show()
        {
            base.Show();

            // TODO: check if the reward has already been collected before showing the timer
            if (true)
            {
                StartNextRewardTimer();
            }
        }


        /// <summary>
        /// Bind the daily reward day button click.
        /// </summary>
        private void BindDailyRewardDayButtonClick()
        {
            _dailyRewardInfoLabel.text = "CLAIM YOUR DAILY REWARD!";

            Action dailyRewardDayClickedHandler = () => DailyRewardDayClicked(_rewardDayToCollect);
            _dailyRewardDayClickAction = dailyRewardDayClickedHandler;

            _dailyRewardDayButton = _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{_rewardDayToCollect}-reward").Q<Button>("daily-reward-day-template__button-container");
            _dailyRewardDayButton.clicked += dailyRewardDayClickedHandler;
            _dailyRewardDayButton.AddToClassList("button-active");
        }


        /// <summary>
        /// Unbind the daily reward day button click.
        /// </summary>
        private void UnbindDailyRewardDaysButtonsClicks()
        {
            _dailyRewardDayButton.clicked -= _dailyRewardDayClickAction;
            _dailyRewardDayButton.RemoveFromClassList("button-active");
        }


        /// <summary>
        /// Reset the daily reward days to their initial state.
        /// </summary>
        private void ResetDailyRewardDays()
        {
            for (int i = 1; i < 8; i++)
            {
                VisualElement dailyRewardDay = _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{i}-reward");
                dailyRewardDay.RemoveFromClassList("collected");
                dailyRewardDay.Q<Label>("daily-reward-day-template__status-label").text = $"DAY {i.ToString()}";
            }
        }


        /// <summary>
        /// Called when a daily reward day is clicked.
        /// </summary>
        /// <param name="day"></param>
        private void DailyRewardDayClicked(short day)
        {
            // TODO: replace with the actual logic (calling a method from DailyRewardManager or something similar)
            if (true)
            {
                // Rewards don't need to be clicked anymore so we can unbind the click events
                UnbindDailyRewardDaysButtonsClicks();

                for (short i = 1; i <= day; i++)
                {
                    VisualElement dailyRewardDay = _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{i}-reward");
                    dailyRewardDay.AddToClassList("collected");
                    dailyRewardDay.Q<Label>("daily-reward-day-template__status-label").text = "COLLECTED";
                }

                StartNextRewardTimer();
            }
        }


        /// <summary>
        /// Starts the timer to update every second the time left before the next day and so, the next reward.
        /// </summary>
        private async void StartNextRewardTimer()
        {
            DateTime midnightUtc = DateTime.UtcNow.MidnightUtc();

            while (isEnabled)
            {
                // Calculate the time left until midnight utc
                TimeSpan timeLeft = midnightUtc - DateTime.UtcNow;

                _dailyRewardInfoLabel.text = $"COME BACK IN {timeLeft.ToString(@"hh\h\ mm\m")}";
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}