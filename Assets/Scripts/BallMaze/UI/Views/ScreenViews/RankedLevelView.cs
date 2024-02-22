using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityExtensionMethods;
using System.Threading.Tasks;


namespace BallMaze.UI
{
    public class RankedLevelView : ScreenView
    {
        // Visual Elements
        private Label _rankedLevelResetsInLabel;
        private Label _leaderboardLoadingLabel;
        private ScrollView _leaderboardContainerScrollView;
        private VisualElement _leaderboardUserEntry;
        private VisualElement _levelImage;
        private Label _rankedLevelBestTime;
        private Button _playRankedLevelButton;

        public RankedLevelView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _rankedLevelResetsInLabel = _root.Q<Label>("ranked-level__resets-in-label");
            _leaderboardLoadingLabel = _root.Q<Label>("ranked-level__leaderboard-loading-label");
            _leaderboardContainerScrollView = _root.Q<ScrollView>("ranked-level__leaderboard-entries-container-scroll-view");
            _leaderboardUserEntry = _root.Q<VisualElement>("ranked-level__user-entry");
            _levelImage = _root.Q<VisualElement>("ranked-level__level-image");
            _rankedLevelBestTime = _root.Q<Label>("ranked-level__best-time");
            _playRankedLevelButton = _root.Q<Button>("ranked-level__play-level-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _playRankedLevelButton.clicked += () =>
            {
                UIManager.Instance.Hide(UIViewType.RankedLevel);
            };
        }


        public override void Show()
        {
            base.Show();

            // Start the timer
            StartResetsInTimer();
        }


        /// <summary>
        /// Populate the leaderboard
        /// </summary>
        private void PopulateLeaderboard()
        {
            // Hide the loading text
            _leaderboardLoadingLabel.style.display = DisplayStyle.None;

            // Show the leaderboard
            _leaderboardContainerScrollView.style.display = DisplayStyle.Flex;
        }



        /// <summary>
        /// Clear the leaderboard and show the loading text
        /// </summary>
        private void ResetLeaderboard()
        {
            // Clear the leaderboard
            _leaderboardContainerScrollView.Clear();

            // Show the loading text
            _leaderboardLoadingLabel.style.display = DisplayStyle.Flex;
        }


        /// <summary>
        /// Update the player's rank and time in the leaderboard
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="time"></param>
        private void UpdatePlayerRankAndTime(int rank, TimeSpan time)
        {
            _leaderboardUserEntry.Q<Label>("leaderboard-entry-template__rank").text = rank.ToString();
            _leaderboardUserEntry.Q<Label>("leaderboard-entry-template__time").text = time.ToString(@"ss\:ff\s");
        }


        /// <summary>
        /// Update the level's image
        /// </summary>
        /// <param name="levelImage"></param>
        private void UpdateRankedLevel(Texture2D levelImage)
        {
            _levelImage.style.backgroundImage = levelImage;
        }


        /// <summary>
        /// Update the user's best time
        /// </summary>
        /// <param name="time"></param>
        private void UpdateBestTime(TimeSpan time)
        {
            _rankedLevelBestTime.text = time.ToString(@"ss\:ff\s");
        }


        /// <summary>
        /// Starts the timer and update the "Resets in" label every second until the view is hidden
        /// </summary>
        private async void StartResetsInTimer()
        {

            DateTime endOfWeek = DateTime.UtcNow.EndOfWeek();

            while (isEnabled)
            {
                // Calculate the time left until midnight utc
                TimeSpan timeLeft = endOfWeek - DateTime.UtcNow;

                _rankedLevelResetsInLabel.text = $"Resets in: {timeLeft.ToString(@"dd\:hh")}";
                await Task.Delay(1000);
            }
        }
    }
}