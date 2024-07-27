using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class DailyLevelsView : ScreenView
    {
        private short _dailyLevelsCompletedToday;
        private short _dailyLevelsStreak;

        public bool AreDailyLevelsLoaded = false;

        // Visual Elements
        private VisualElement _dailyLevelsButtonsContainerScrollView;
        private Dictionary<DailyLevelDifficulty, Action> _dailyLevelsButtonClickAction;
        private VisualElement[] _dailyLevelsStreakDays = new VisualElement[7];
        private Label _dailyLevelsUpdatesInLabel;

        // Adressable handle to load the default level selection template
        private AsyncOperationHandle<VisualTreeAsset> _defaultLevelSelectionTemplateHandle;
        private VisualTreeAsset _levelSelectionTemplate;


        public DailyLevelsView(VisualElement root) : base(root)
        {
            _dailyLevelsCompletedToday = 0;
            _dailyLevelsStreak = 0;

            _dailyLevelsButtonClickAction = new Dictionary<DailyLevelDifficulty, Action>();

            _defaultLevelSelectionTemplateHandle = Addressables.LoadAssetAsync<VisualTreeAsset>("DefaultLevelSelectionTemplate");
            _defaultLevelSelectionTemplateHandle.Completed += DefaultLevelSelectionTemplateHandleCompleted;
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
        public void PopulateLevels(Level[] dailyLevels)
        {
            // Start by emptying the daily levels view
            EmptyDailyLevelsView();

#if UNITY_EDITOR
            VisualTreeAsset levelSelectionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Templates/DefaultLevelSelectionTemplate.uxml");
#else
            VisualTreeAsset levelSelectionTemplate = _levelSelectionTemplate;
#endif

            foreach (DailyLevelDifficulty difficulty in Enum.GetValues(typeof(DailyLevelDifficulty)))
            {
                if (difficulty == DailyLevelDifficulty.Unknown)
                {
                    continue;
                }

                VisualElement dailyLevelTemplateClone = levelSelectionTemplate.CloneTree();
                dailyLevelTemplateClone.name = $"daily-levels__level-{(int)difficulty + 1}";
                dailyLevelTemplateClone.AddToClassList("daily-levels__level-button");
                _dailyLevelsButtonsContainerScrollView.Add(dailyLevelTemplateClone);

                Action dailyLevelClickedHandler = () => { DailyLevelClicked(difficulty); };
                _dailyLevelsButtonClickAction[difficulty] = dailyLevelClickedHandler;

                Button dailyLevelCloneButton = dailyLevelTemplateClone.Q<Button>("default-level-selection-template__container-button");
                dailyLevelCloneButton.clicked += dailyLevelClickedHandler;

                // Lock all the levels except the first one
                if (difficulty != DailyLevelDifficulty.VeryEasy)
                {
                    dailyLevelTemplateClone.AddToClassList("level-locked");
                    dailyLevelCloneButton.text = "Locked";
                }
            }

            if (_dailyLevelsButtonsContainerScrollView.childCount == 5)
            {
                AreDailyLevelsLoaded = true;
            }
        }


        /// <summary>
        /// Empties the daily levels view by removing all the content of the scroll view all clicks events
        /// </summary>
        private void EmptyDailyLevelsView()
        {
            if (AreDailyLevelsLoaded)
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

            AreDailyLevelsLoaded = false;
        }


        private void DailyLevelClicked(DailyLevelDifficulty difficulty)
        {
            LevelManager.SwitchMode(LevelType.DailyLevel);

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
            try
            {
                if (isEnabled && !AreDailyLevelsLoaded)
                {
                    throw new CouldNotLoadLevelException("No daily levels loaded");
                }
            }
            catch (Exception e)
            {
                DisplayDefaultDailyLevelsLoadingException(e);
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
            try
            {
                if (isEnabled && !AreDailyLevelsLoaded)
                {
                    throw new CouldNotLoadLevelException("No daily levels loaded");
                }
            }
            catch (Exception e)
            {
                DisplayDefaultDailyLevelsLoadingException(e);
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
            try
            {
                if (isEnabled && !AreDailyLevelsLoaded)
                {
                    throw new CouldNotLoadLevelException("No daily levels loaded");
                }
            }
            catch (Exception e)
            {
                DisplayDefaultDailyLevelsLoadingException(e);
                return;
            }

            VisualElement dailyLevelButton = _dailyLevelsButtonsContainerScrollView.Q<VisualElement>($"daily-levels__level-{(int)difficulty + 1}");

            // Show the time
            dailyLevelButton.Q<Label>("default-level-selection-template__time-label").text = $"{time.ToString("00.00")}s";
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
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }


        /// <summary>
        /// Displays the exception UI with the default daily levels loading error message.
        /// </summary>
        private void DisplayDefaultDailyLevelsLoadingException(Exception e)
        {
            ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "DailyLevelsLoadingCheckInternetGenericError", ExceptionActionType.BackToMainMenu);
        }


        private void DefaultLevelSelectionTemplateHandleCompleted(AsyncOperationHandle<VisualTreeAsset> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _levelSelectionTemplate = operation.Result;
            }
            else
            {
                ExceptionManager.ShowExceptionMessage(operation.OperationException, $"Couldn't load default level selection template: {operation.OperationException}");
            }
            _defaultLevelSelectionTemplateHandle.Completed -= DefaultLevelSelectionTemplateHandleCompleted;
        }

        public override void Dispose()
        {
            base.Dispose();

            Addressables.Release(_defaultLevelSelectionTemplateHandle);
        }
    }
}