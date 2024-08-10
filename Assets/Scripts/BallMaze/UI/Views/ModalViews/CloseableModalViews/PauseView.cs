using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class PauseView : ModalView
    {
        public override bool isCloseable => true;


        private bool _musicEnabled;
        private bool _sfxEnabled;

        // Visual Elements
        private Button _closeButton;
        private VisualElement _aspectRatioContainer;
        private Button _musicButton;
        private Button _sfxButton;
        private Label _levelNameLabel;
        private Label _levelDifficultyLabel;
        private Label _bestTimeLabel;
        private Label _nextStarLabel;
        private VisualElement _leaderboardTop3EntriesContainer;
        private VisualElement _leaderboardUserEntry;
        private Button _defaultLevelsListButton;
        private Button _dailyLevelsListButton;
        private Button _homeButton;
        private Button _resumeButton;
        private Button _tryAgainButton;

        // Set to false when the UI is shown and is set to true whenever the player interacts with the UI (eg: try again, reset...).
        // This is needed because once the UI is hidden, we need to run the default behavior (ie: resume the level)
        // if and only if the player hasn't interacted with the UI. Otherwise, the player will be stuck and unable to play
        private bool _hasInteractedBeforeHide = false;


        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.8f;


        public PauseView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _musicEnabled = true;
            _sfxEnabled = true;

            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _aspectRatioContainer = _root.Q<VisualElement>("pause__aspect-ratio-container");
            _musicButton = _root.Q<Button>("pause__music-button");
            _sfxButton = _root.Q<Button>("pause__sfx-button");
            _levelNameLabel = _root.Q<Label>("pause__level-name-label");
            _levelDifficultyLabel = _root.Q<Label>("pause__level-difficulty-label");
            _bestTimeLabel = _root.Q<Label>("pause__best-time-label");
            _nextStarLabel = _root.Q<Label>("pause__next-star-label");
            _leaderboardTop3EntriesContainer = _root.Q<VisualElement>("pause__leaderboard-top-3-entries-container");
            _leaderboardUserEntry = _root.Q<VisualElement>("pause__leaderboard-user-entry");
            _defaultLevelsListButton = _root.Q<Button>("pause__default-levels-list-button");
            _dailyLevelsListButton = _root.Q<Button>("pause__daily-levels-list-button");
            _homeButton = _root.Q<Button>("pause__home-button");
            _resumeButton = _root.Q<Button>("pause__resume-button");
            _tryAgainButton = _root.Q<Button>("pause__try-again-button");

            LevelEvents.LevelModeUpdated += (levelType) => { SwitchLevelTypeSource(levelType); };

            LevelEvents.LevelNameUpdated += (levelName) => { _levelNameLabel.text = levelName; };

            LevelEvents.DailyLevelDifficultyUpdated += (difficulty) => { SetLevelDifficulty(difficulty); };

            LevelEvents.DefaultLevelBestTimeUpdated += (_, time) => { _bestTimeLabel.text = $"BEST TIME: {time.ToString("00.00")}s"; };

            LevelEvents.DailyLevelBestTimeUpdated += (_, time) => { _bestTimeLabel.text = $"BEST TIME: {time.ToString("00.00")}s"; };

            LevelEvents.BestTimeUpdated += (bestTime) => { _bestTimeLabel.text = $"BEST TIME: {bestTime.ToString("00.00")}s"; };

            LevelEvents.NextStarTimeUpdated += (nextStarTime) => { SetNextStarTime(nextStarTime); };
        }



        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clicked += () =>
            {
                UIManager.Instance.Hide(UIViewType.Pause);
            };

            _musicButton.clicked += () =>
            {
                _musicEnabled = !_musicEnabled;
                SetMusicButtonState(_musicEnabled);
            };

            _sfxButton.clicked += () =>
            {
                _sfxEnabled = !_sfxEnabled;
                SetSFXButtonState(_sfxEnabled);
            };


            _defaultLevelsListButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.QuitLevel();
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
            };


            _dailyLevelsListButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.QuitLevel();
                UIManager.Instance.Show(UIViewType.DailyLevels);
            };

            _homeButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                UIManager.Instance.Show(UIViewType.MainMenu);
            };

            _resumeButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.ResumeLevel();
                UIManager.Instance.Hide(UIViewType.Pause);
            };

            _tryAgainButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.ResetLevel();
                UIManager.Instance.Hide(UIViewType.Pause);
            };
        }


        public override void Show()
        {
            base.Show();

            // Tween the modal view from the top to the center of the screen
            UIUtitlities.TweenModalViewFromTop(_root, UI_HEIGHT_PERCENTAGE);

            LevelManager.Instance.PauseLevel();

            _hasInteractedBeforeHide = false;
        }


        public override async void Hide()
        {
            if (!_hasInteractedBeforeHide)
            {
                if (LevelManager.Instance.HasLevelStarted)
                {
                    LevelManager.Instance.ResumeLevel();
                }
                else
                {
                    LevelManager.Instance.LevelState = LevelState.WaitingToStart;
                }
            }

            // Tween the modal view back to the top of the screen
            await UIUtitlities.TweenModalViewToTopAndWait(_root, UI_HEIGHT_PERCENTAGE);

            base.Hide();
        }


        public void ForceHide()
        {
            _hasInteractedBeforeHide = true;
            Hide();
        }


        /// <summary>
        /// Toggle the state of the music button
        /// </summary>
        /// <param name="enabled"></param>
        private void SetMusicButtonState(bool enabled)
        {
            if (enabled)
            {
                _musicButton.RemoveFromClassList("audio-controls-button-inactive");
                _musicButton.AddToClassList("audio-controls-button-active");
            }
            else
            {
                _musicButton.RemoveFromClassList("audio-controls-button-active");
                _musicButton.AddToClassList("audio-controls-button-inactive");
            }
        }


        /// <summary>
        /// Toggle the state of the SFX button
        /// </summary>
        /// <param name="enabled"></param>
        private void SetSFXButtonState(bool enabled)
        {
            if (enabled)
            {
                _sfxButton.RemoveFromClassList("audio-controls-button-inactive");
                _sfxButton.AddToClassList("audio-controls-button-active");
            }
            else
            {
                _sfxButton.RemoveFromClassList("audio-controls-button-active");
                _sfxButton.AddToClassList("audio-controls-button-inactive");
            }
        }


        public void SetLevelDifficulty(DailyLevelDifficulty difficulty = DailyLevelDifficulty.Unknown)
        {
            (string name, Color color) = difficulty.GetDifficultyInfo();

            _levelDifficultyLabel.text = name;
            _levelDifficultyLabel.style.color = color;
        }


        private void SetNextStarTime(float? nextStarTime)
        {
            if (nextStarTime == null)
            {
                _nextStarLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _nextStarLabel.style.display = DisplayStyle.Flex;
                _nextStarLabel.text = $"NEXT STAR: {nextStarTime?.ToString("00.00")}s";
            }
        }


        /// <summary>
        /// Populate the top 3 leaderboard
        /// </summary>
        private void PopulateTop3Leaderboard()
        {
            //TODO: populate the top 3 leaderboard
        }


        /// <summary>
        ///  Clear the top 3 leaderboard
        /// </summary>
        private void ResetTop3Leaderboard()
        {
            //TODO: clear the top 3 leaderboard
        }


        /// <summary>
        /// Update the player's rank and time in the leaderboard
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="time"></param>
        private void UpdatePlayerRankAndTime(int rank, float time)
        {
            _leaderboardUserEntry.Q<Label>("leaderboard-entry-template__rank").text = rank.ToString();
            _leaderboardUserEntry.Q<Label>("leaderboard-entry-template__time").text = $"{time.ToString("00.00")}s";
        }


        /// <summary>
        /// Change the UI layout based on the level type the user is playing
        /// </summary>
        public void SwitchLevelTypeSource(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.RemoveFromClassList("daily-levels");
                    _aspectRatioContainer.AddToClassList("default-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.Flex;
                    _dailyLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.None;
                    _levelDifficultyLabel.style.display = DisplayStyle.None;
                    break;
                case LevelType.DailyLevel:
                    _aspectRatioContainer.RemoveFromClassList("default-levels");
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.AddToClassList("daily-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _dailyLevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    _levelDifficultyLabel.style.display = DisplayStyle.Flex;
                    break;
                case LevelType.RankedLevel:
                    _aspectRatioContainer.RemoveFromClassList("default-levels");
                    _aspectRatioContainer.RemoveFromClassList("daily-levels");
                    _aspectRatioContainer.AddToClassList("ranked-level");
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _dailyLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
        }
    }
}