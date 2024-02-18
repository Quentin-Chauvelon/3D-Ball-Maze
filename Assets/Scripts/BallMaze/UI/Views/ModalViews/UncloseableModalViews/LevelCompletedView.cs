using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class LevelCompletedView : ModalView
    {
        public override bool isCloseable => false;

        private VisualElement[] _stars;

        // Visual Elements
        private Label _coinsEarntLabel;
        private Button _doubleCoinsAdButton;
        private Button _doubleCoinsIAPButton;
        private Label _timeLabel;
        private Label _secondTimeInfoLabel;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private Button _nextLevelButton;
        private Button _tryAgainButton;


        public LevelCompletedView(VisualElement root) : base(root)
        {
            _stars = new VisualElement[3];
        }


        protected override void SetVisualElements()
        {
            _stars[0] = _root.Q<VisualElement>("level-completed__star-1");
            _stars[1] = _root.Q<VisualElement>("level-completed__star-2");
            _stars[2] = _root.Q<VisualElement>("level-completed__star-3");
            _coinsEarntLabel = _root.Q<Label>("level-completed__coins-earnt-label");
            _doubleCoinsAdButton = _root.Q<Button>("level-completed__double-coins-ad-button");
            _doubleCoinsIAPButton = _root.Q<Button>("level-completed__double-coins-iap-button");
            _timeLabel = _root.Q<Label>("level-completed__time-label");
            _secondTimeInfoLabel = _root.Q<Label>("level-completed__second-time-info-label");
            _defaultLevelsListButton = _root.Q<Button>("level-completed__default-levels-list-button");
            _homeButton = _root.Q<Button>("level-completed__home-button");
            _nextLevelButton = _root.Q<Button>("level-completed__next-level-button");
            _tryAgainButton = _root.Q<Button>("level-completed__try-again-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _doubleCoinsAdButton.clicked += () =>
            {
                // TODO: show ad and double the coins earnt (give + update label) if the ad was watched
            };

            _doubleCoinsIAPButton.clicked += () =>
            {
                // TODO: show the double coins permanently IAP UI, and if bought, double the coins + update Ui (doubleds)
            };

            _defaultLevelsListButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
                Hide();
            };

            _homeButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.MainMenu);
                Hide();
            };

            _nextLevelButton.clicked += () =>
            {
                // TODO: go to next level
                Hide();
            };

            _tryAgainButton.clicked += () =>
            {
                // TODO: restart the level
                Hide();
            };
        }


        /// <summary>
        /// Update the amount of coins the user has
        /// </summary>
        /// <param name="coins"></param>
        private void UpdateCoinsAmount(int coins)
        {
            _coinsEarntLabel.text = coins.ToString();
        }


        /// <summary>
        /// Update the time labels
        /// </summary>
        /// <param name="time">The time the user got in the level</param>
        /// <param name="starsGained">The amount of stars the user got in the level</param>
        /// <param name="secondTimeInfo">A string which depends on the context. For instance, if the player hasn't got all the stars
        /// it will be "NEXT STAR: XX:XXs, if the player got all the stars, it will be "BEST TIME: XX:XXs" or "NEW BEST TIME: XX:XXs"...
        /// </param>
        private void UpdateTime(TimeSpan time, short starsGained, string secondTimeInfo)
        {
            // Hide the stars
            _stars[0].RemoveFromClassList("star-active");
            _stars[1].RemoveFromClassList("star-active");
            _stars[2].RemoveFromClassList("star-active");

            // Show the stars the user got
            for (int i = 0; i < starsGained; i++)
            {
                _stars[i].AddToClassList("star-active");
            }

            _timeLabel.text = $"TIME: {time.ToString(@"ss\:ff\s")}";
            _secondTimeInfoLabel.text = secondTimeInfo;
        }
    }
}