using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class MainMenuView : ScreenView
    {
        // Visual Elements
        private MainTextButton _profileButton;
        private MainImageButton _shopButton;
        private MainImageButton _skinsButton;
        private MainImageButton _dailyRewardButton;
        private MainTextButton _playButton;


        public MainMenuView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _profileButton = _root.Q<MainTextButton>("main-menu__profile-button");
            _shopButton = _root.Q<MainImageButton>("main-menu__shop-button");
            _skinsButton = _root.Q<MainImageButton>("main-menu__skins-button");
            _dailyRewardButton = _root.Q<MainImageButton>("main-menu__daily-reward-button");
            _playButton = _root.Q<MainTextButton>("main-menu__play-button");

            SetDailyRewardsButtonVisibility(false);
        }


        protected override void RegisterButtonCallbacks()
        {
            // Open mode selection when play is clicked
            _playButton.Button.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.ModeSelection); };

            _dailyRewardButton.Button.clickable.clicked += () => { UIManager.Instance.Show(UIViewType.DailyReward); };

            _skinsButton.Button.clicked += () => { UIManager.Instance.Show(UIViewType.Skins); };
        }


        public void SetDailyRewardsButtonVisibility(bool visible)
        {
            _dailyRewardButton.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}