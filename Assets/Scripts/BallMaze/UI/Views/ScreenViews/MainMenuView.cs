using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class MainMenuView : ScreenView
    {
        // Visual Elements
        private Button _profileButton;
        private Button _shopButton;
        private Button _skinsButton;
        private Button _dailyRewardButton;
        private Button _playButton;
        private Label _bottomInfoLabel;


        public MainMenuView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _profileButton = _root.Q<Button>("main-menu__profile-button");
            _shopButton = _root.Q<Button>("main-menu__shop-button");
            _skinsButton = _root.Q<Button>("main-menu__skins-button");
            _dailyRewardButton = _root.Q<Button>("main-menu__daily-reward-button");
            _playButton = _root.Q<Button>("main-menu__play-button");
            _bottomInfoLabel = _root.Q<Label>("main-menu__bottom-info");

            SetDailyRewardsButtonVisibility(false);
        }


        protected override void RegisterButtonCallbacks()
        {
            // Open mode selection when play is clicked
            _playButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.ModeSelection); };

            _dailyRewardButton.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.DailyReward); };

            _skinsButton.clicked += () => { UIManager.Instance.Show(UIViewType.Skins); };
        }


        public void SetDailyRewardsButtonVisibility(bool visible)
        {
            _dailyRewardButton.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}