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


        // Visual Elements
        private Button _closeButton;
        private VisualElement _dailyRewardDaysButtonsContainer;
        private Button _dailyRewardDayButton;
        private Action _dailyRewardDayClickAction;
        private Label _dailyRewardInfoLabel;
        private Button _claimRewardButton;

        // Not displayed. It is only used to store the images that will be used when populating the rewards.
        // This allows us to avoid using adressables for thoses images
        private VisualElement _imagesContainer;


        public DailyRewardView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _dailyRewardDaysButtonsContainer = _root.Q<VisualElement>("daily-reward__rewards-container");

            _dailyRewardDayButton = _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{DailyReward.DailyRewardStreak + 1}-reward").Q<Button>("daily-reward-day-template__button-container");

            // Change the text for each reward to match the day
            for (int i = 1; i < 8; i++)
            {
                _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{i}-reward").Q<Label>("daily-reward-day-template__status-label").text = $"DAY {i.ToString()}";
            }

            _dailyRewardInfoLabel = _root.Q<Label>("daily-reward__info-label");
            _claimRewardButton = _root.Q<Button>("daily-reward__claim-reward-button");
            _imagesContainer = _root.Q<VisualElement>("daily-reward__images-container");
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clicked += () => { UIManager.Instance.Hide(UIViewType.DailyReward); };

            _claimRewardButton.clicked += () => { DailyRewardDayClicked(DailyReward.DailyRewardStreak + 1); };
        }


        public override void Show()
        {
            base.Show();

            if (DailyReward.Collected)
            {
                StartNextRewardTimer();
            }

            // Reset the text on show, which will be overriden by the timer if needed
            _dailyRewardInfoLabel.text = $"CLAIM YOUR DAILY REWARD!";
        }


        public void PopulateDailyRewards(object[] rewards, int streak)
        {
            if (streak == 0)
            {
                ResetDailyRewardDays();
            }

            for (short i = 1; i <= 7; i++)
            {
                VisualElement dailyRewardDay = GetDailyRewardButtonFromDay(i);

                RewardType rewardType = RewardsManager.GetRewardType(rewards[i - 1]);

                // Update the image of the reward
                dailyRewardDay.Q<VisualElement>("daily-reward-day-template__primary-reward-image").style.backgroundImage = GetDailyRewardImageForReward(rewardType);

                // Update the label of the reward
                switch (rewardType)
                {
                    case RewardType.Coins:
                        dailyRewardDay.Q<Label>("daily-reward-day-template__reward-value-label").text = RewardsManager.GetCoinReward(rewards[i - 1]).amount.ToString();
                        break;
                    case RewardType.Skin:
                        dailyRewardDay.Q<Label>("daily-reward-day-template__reward-value-label").text = SkinManager.GetSkinRarityName(RewardsManager.GetSkinReward(rewards[i - 1]).rarity);
                        break;
                    default:
                        dailyRewardDay.Q<Label>("daily-reward-day-template__reward-value-label").text = "???";
                        break;
                }
            }

            for (short i = 1; i <= streak; i++)
            {
                VisualElement dailyRewardDay = GetDailyRewardButtonFromDay(i);
                dailyRewardDay.AddToClassList("collected");
                dailyRewardDay.Q<Label>("daily-reward-day-template__status-label").text = "COLLECTED";
            }

            // Bind the buttons
            if (!DailyReward.Collected)
            {
                UnbindDailyRewardDaysButtonsClicks();
                BindDailyRewardDayButtonClick();
            }
        }


        /// <summary>
        /// Bind the daily reward day button click.
        /// </summary>
        public void BindDailyRewardDayButtonClick()
        {
            _dailyRewardInfoLabel.text = "CLAIM YOUR DAILY REWARD!";

            Action dailyRewardDayClickedHandler = () => DailyRewardDayClicked(DailyReward.DailyRewardStreak + 1);
            _dailyRewardDayClickAction = dailyRewardDayClickedHandler;

            _dailyRewardDayButton = GetDailyRewardButtonFromDay(DailyReward.DailyRewardStreak + 1).Q<Button>("daily-reward-day-template__button-container");
            _dailyRewardDayButton.clicked += dailyRewardDayClickedHandler;
            _dailyRewardDayButton.AddToClassList("button-active");
        }


        /// <summary>
        /// Unbind the daily reward day button click.
        /// </summary>
        public void UnbindDailyRewardDaysButtonsClicks()
        {
            _dailyRewardDayButton.clicked -= _dailyRewardDayClickAction;
            _dailyRewardDayButton.RemoveFromClassList("button-active");
        }


        /// <summary>
        /// Reset the daily reward days to their initial state.
        /// </summary>
        public void ResetDailyRewardDays()
        {
            for (int i = 1; i < 8; i++)
            {
                VisualElement dailyRewardDay = GetDailyRewardButtonFromDay(i);
                dailyRewardDay.RemoveFromClassList("collected");
                dailyRewardDay.Q<Label>("daily-reward-day-template__status-label").text = $"DAY {i.ToString()}";
            }
        }


        /// <summary>
        /// Called when a daily reward day is clicked.
        /// </summary>
        /// <param name="day"></param>
        private void DailyRewardDayClicked(int day)
        {
            if (GameManager.DEBUG)
            {
                Debug.Log($"Trying to collect daily reward for day {day}");
            }

            if (DailyReward.CollectDailyReward(day))
            {
                // Rewards don't need to be clicked anymore so we can unbind the click events
                UnbindDailyRewardDaysButtonsClicks();

                for (short i = 1; i <= day; i++)
                {
                    VisualElement dailyRewardDay = GetDailyRewardButtonFromDay(i);
                    dailyRewardDay.AddToClassList("collected");
                    dailyRewardDay.Q<Label>("daily-reward-day-template__status-label").text = "COLLECTED";
                }

                StartNextRewardTimer();
            }
        }


        public StyleBackground GetDailyRewardImageForReward(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Coins:
                    return _imagesContainer.Q<VisualElement>("daily-reward__coin-image").resolvedStyle.backgroundImage;
                case RewardType.Skin:
                    return _imagesContainer.Q<VisualElement>("daily-reward__skin-image").resolvedStyle.backgroundImage;
                default:
                    return _imagesContainer.Q<VisualElement>("daily-reward__unknowned-image").resolvedStyle.backgroundImage;
            }
        }


        public VisualElement GetDailyRewardButtonFromDay(int day)
        {
            return _dailyRewardDaysButtonsContainer.Q<VisualElement>($"daily-reward__day-{day}-reward");
        }


        /// <summary>
        /// Starts the timer to update every second the time left before the next day and so, the next reward.
        /// </summary>
        private async void StartNextRewardTimer()
        {
            DateTime midnightUtc = DateTime.UtcNow.MidnightUtc().AddMinutes(1);

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