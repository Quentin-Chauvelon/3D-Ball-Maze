using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEditor.PackageManager.Requests;
using System.Linq;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class DailyLevelsView : ScreenView
    {
        private short _dailyLevelsCompletedToday;
        private short _dailyLevelsStreak;
        private bool _areDailyLevelsLoaded;

        // Visual Elements
        private VisualElement _dailyLevelsButtonsContainerScrollView;
        private Dictionary<DailyLevelDifficulty, Action> _dailyLevelsButtonClickAction;
        private VisualElement[] _dailyLevelsStreakDays = new VisualElement[7];
        private Label _dailyLevelsUpdatesInLabel;


        public DailyLevelsView(VisualElement root) : base(root)
        {
            _dailyLevelsCompletedToday = 0;
            _dailyLevelsStreak = 0;

            _dailyLevelsButtonClickAction = new Dictionary<DailyLevelDifficulty, Action>();
        }


        protected override void SetVisualElements()
        {
            _dailyLevelsButtonsContainerScrollView = _root.Q<VisualElement>("daily-levels__levels-container");

            for (int i = 0; i < 7; i++)
            {
                _dailyLevelsStreakDays[i] = _root.Q<VisualElement>($"daily-levels__streak-day-{i + 1}");
            }

            _dailyLevelsUpdatesInLabel = _root.Q<Label>("daily-levels__updates-in-label");
        }


        public override void Show()
        {
            base.Show();

            StartUpdatesInTimer();
        }


        /// <summary>
        /// Populates the daily levels view with the levels of the day
        /// </summary>
        /// <param name="textures"></param>
        private void PopulateLevels(Texture[] textures)
        {
            // Start by emptying the daily levels view
            EmptyDailyLevelsView();

            VisualTreeAsset levelSelectionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Templates/DefaultLevelSelectionTemplate.uxml");

            foreach (DailyLevelDifficulty difficulty in Enum.GetValues(typeof(DailyLevelDifficulty)))
            {
                VisualElement dailyLevelTemplateClone = levelSelectionTemplate.CloneTree();
                dailyLevelTemplateClone.name = $"daily-levels__level-{(int)difficulty + 1}";
                dailyLevelTemplateClone.AddToClassList("daily-levels__level-button");
                _dailyLevelsButtonsContainerScrollView.Add(dailyLevelTemplateClone);

                // Lock all the levels except the first one
                if (difficulty != DailyLevelDifficulty.VeryEasy)
                {
                    dailyLevelTemplateClone.AddToClassList("level-locked");
                }

                Action dailyLevelClickedHandler = () => { DailyLevelClicked(difficulty); };
                _dailyLevelsButtonClickAction[difficulty] = dailyLevelClickedHandler;

                Button dailyLevelCloneButton = dailyLevelTemplateClone.Q<Button>("default-level-selection-template__container-button");
                dailyLevelCloneButton.style.backgroundImage = (StyleBackground)textures[(int)difficulty];
                dailyLevelCloneButton.clicked += dailyLevelClickedHandler;
            }

            if (_dailyLevelsButtonsContainerScrollView.childCount == 5)
            {
                _areDailyLevelsLoaded = true;
            }
        }


        /// <summary>
        /// Empties the daily levels view by removing all the content of the scroll view all clicks events
        /// </summary>
        private void EmptyDailyLevelsView()
        {
            if (_areDailyLevelsLoaded)
            {
                // Unsubscribe from the click event of each daily level button
                foreach (DailyLevelDifficulty difficulty in Enum.GetValues(typeof(DailyLevelDifficulty)))
                {
                    _dailyLevelsButtonsContainerScrollView.ElementAt((int)difficulty).Q<Button>("default-level-selection-template__container-button").clicked -= _dailyLevelsButtonClickAction[difficulty];
                }
            }

            // Empty the action dictionary and the scroll view content
            _dailyLevelsButtonClickAction.Clear();
            _dailyLevelsButtonsContainerScrollView.Clear();

            _areDailyLevelsLoaded = false;
        }


        private void DailyLevelClicked(DailyLevelDifficulty difficulty)
        {
            Debug.Log($"Daily level {difficulty} clicked");
            throw new NotImplementedException("Implement DailyLevelClicked method to start the daily level with the selected difficulty");
        }


        /// <summary>
        /// Update the streak days visual elements from day 1 to the given streak day
        /// </summary>
        /// <param name="streak"></param>
        private void UpdateStreak(short streak)
        {
            for (int i = 0; i < streak; i++)
            {
                _dailyLevelsStreakDays[i].RemoveFromClassList("daily-levels__streak-day-inactive");
                _dailyLevelsStreakDays[i].AddToClassList("daily-levels__streak-day-active");
            }
        }


        /// <summary>
        /// Resets the streak days visual elements to their initial state
        /// </summary>
        private void ResetStreak()
        {
            foreach (VisualElement streakDay in _dailyLevelsStreakDays)
            {
                streakDay.RemoveFromClassList("daily-levels__streak-day-active");
                streakDay.AddToClassList("daily-levels__streak-day-inactive");
            }
        }


        /// <summary>
        /// Unlocks all the levels up to the given difficulty
        /// </summary>
        /// <param name="difficultyToUnlockTo"></param>
        private void UnlockLevels(DailyLevelDifficulty difficultyToUnlockTo)
        {
            // Only reset the levels if they are loaded.
            // Furthermore, since this method is called at midnight utc, the UI might not be displayed if the user is not in the daily levels view
            // And so, we don't want to display the error if the UI is not displayed
            // (eg: the player might be offline playing another mode and so daily levels might not be loaded but it is then normal and no error should be displayed)
            if (isEnabled && !_areDailyLevelsLoaded)
            {
                DisplayDefaultDailyLevelsLoadingException();
                return;
            }

            for (int i = 1; i <= (int)difficultyToUnlockTo + 1; i++)
            {
                _dailyLevelsButtonsContainerScrollView.Q<VisualElement>($"daily-levels__level-{i}").RemoveFromClassList("level-locked");
            }
        }


        /// <summary>
        /// Resets the levels to their initial state
        /// </summary>
        private void ResetLevels()
        {
            // Only reset the levels if they are loaded.
            // Furthermore, since this method is called at midnight utc, the UI might not be displayed if the user is not in the daily levels view
            // And so, we don't want to display the error if the UI is not displayed
            // (eg: the player might be offline playing another mode and so daily levels might not be loaded but it is then normal and no error should be displayed)
            if (isEnabled && !_areDailyLevelsLoaded)
            {
                DisplayDefaultDailyLevelsLoadingException();
                return;
            }

            // Lock the levels, hide the stars and time
            for (int i = 1; i < 6; i++)
            {
                VisualElement dailyLevelButton = _dailyLevelsButtonsContainerScrollView.Q<VisualElement>($"daily-levels__level-{i}");

                // Lock the levels except the first one
                if (i != 1)
                {
                    dailyLevelButton.AddToClassList("level-locked");
                }

                // Hide the stars and time
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-1-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-2-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-3-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<Label>("default-level-selection-template__time-label").style.display = DisplayStyle.None;
            }

            EmptyDailyLevelsView();
        }


        /// <summary>
        /// Update the time and stars of the level with the given difficulty
        /// </summary>
        /// <param name="difficulty">The difficulty the user was playing</param>
        /// <param name="starsGained">The number of stars the user got with their time</param>
        /// <param name="time">The time the user got in milliseconds</param>
        private void UpdateLevelBestTime(DailyLevelDifficulty difficulty, short starsGained, TimeSpan time)
        {
            // Only reset the levels if they are loaded.
            // Furthermore, since this method is called at midnight utc, the UI might not be displayed if the user is not in the daily levels view
            // And so, we don't want to display the error if the UI is not displayed
            // (eg: the player might be offline playing another mode and so daily levels might not be loaded but it is then normal and no error should be displayed)
            if (isEnabled && !_areDailyLevelsLoaded)
            {
                DisplayDefaultDailyLevelsLoadingException();
                return;
            }

            VisualElement dailyLevelButton = _dailyLevelsButtonsContainerScrollView.Q<VisualElement>($"daily-levels__level-{(int)difficulty + 1}");

            // Show the time
            dailyLevelButton.Q<Label>("default-level-selection-template__time-label").text = time.ToString(@"ss\:ff\s");
            dailyLevelButton.Q<Label>("default-level-selection-template__time-label").style.display = DisplayStyle.Flex;

            // Show the stars
            for (int i = 1; i <= starsGained; i++)
            {
                dailyLevelButton.Q<VisualElement>($"default-level-selection-template__star-{i}-image").AddToClassList("star-active");
            }
        }


        /// <summary>
        /// Start the timer and update the "Updates in" label every second until the view is hidden
        /// </summary>
        private async void StartUpdatesInTimer()
        {
            DateTime midnightUtc = DateTime.UtcNow.MidnightUtc();

            while (isEnabled)
            {
                // Calculate the time left until midnight utc
                TimeSpan timeLeft = midnightUtc - DateTime.UtcNow;

                _dailyLevelsUpdatesInLabel.text = $"Updates in: {timeLeft.ToString(@"hh\:mm\:ss")}";
                await Task.Delay(1000);
            }
        }


        /// <summary>
        /// Displays the exception UI with the default daily levels loading error message.
        /// </summary>
        private void DisplayDefaultDailyLevelsLoadingException()
        {
            Debug.Log("No daily levels loaded");
            ExceptionManager.ShowExceptionMessage(LocalizationSettings.StringDatabase.GetLocalizedString("ExceptionMessagesTable", "DailyLevelsLoadingCheckInternetGenericError"), ExceptionManager.ExceptionAction.BackToMainMenu);
        }
    }
}