using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using BallMaze.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class DailyLevelsView : ScreenView
    {
        public bool AreDailyLevelsLoaded = false;

        // Visual Elements
        private Label _dailyLevelsTitle;
        private VisualElement _dailyLevelsButtonsContainer;
        private VisualElement _dailyLevelsStreakContainer;
        private Dictionary<DailyLevelDifficulty, Action> _dailyLevelsButtonClickAction;
        private VisualElement[] _dailyLevelsStreakDays = new VisualElement[7];
        private Label _dailyLevelsUpdatesInLabel;

        // Adressable handle to load the default level selection template
        private AsyncOperationHandle<VisualTreeAsset> _defaultLevelSelectionTemplateHandle;
        private VisualTreeAsset _levelSelectionTemplate;

        // The difficulty of the level to save the thumbnail texture for. If 0, no texture is saved
        private int _saveThumbnailToTexture = 0;


        public DailyLevelsView(VisualElement root) : base(root)
        {
            _dailyLevelsButtonClickAction = new Dictionary<DailyLevelDifficulty, Action>();

            _defaultLevelSelectionTemplateHandle = Addressables.LoadAssetAsync<VisualTreeAsset>("DefaultLevelSelectionTemplate");
            _defaultLevelSelectionTemplateHandle.Completed += DefaultLevelSelectionTemplateHandleCompleted;
        }


        protected override void SetVisualElements()
        {
            _dailyLevelsTitle = _root.Q<Label>("daily-levels__title");

            _dailyLevelsButtonsContainer = _root.Q<VisualElement>("daily-levels__levels-container");
            _dailyLevelsStreakContainer = _root.Q<VisualElement>("daily-levels__streak-container");

            for (int i = 0; i < 7; i++)
            {
                _dailyLevelsStreakDays[i] = _root.Q<VisualElement>($"daily-levels__streak-day-{i + 1}");
            }

            _dailyLevelsUpdatesInLabel = _root.Q<Label>("daily-levels__updates-in-label");

            PlayerEvents.DailyLevelUnlocked += UnlockLevel;
            LevelEvents.DailyLevelBestTimeUpdated += SetLevelTime;
        }


        public override async void Show()
        {
            base.Show();

            _dailyLevelsTitle.text = $"DAILY LEVELS - {GameManager.Instance.GetUtcNowTime().ToString("dd/MM/yyyy")}";

            StartUpdatesInTimer();

            // If the daily levels aren't loaded, show the loading screen
            if (!AreDailyLevelsLoaded)
            {
                LoadingScreen.LoadingScreenUIView.InitializeLoadingScreen(LoadingIndicatorType.CircularAnimation);

                TimeoutException timeoutException = await UniTask.WaitUntil(() => AreDailyLevelsLoaded).Timeout(20);

                LoadingScreen.LoadingScreenUIView.Hide();

                if (timeoutException != null)
                {
                    UIManager.Instance.Show(UIViewType.MainMenu);

                    ExceptionManager.ShowExceptionMessage(timeoutException, "ExceptionMessagesTable", "DailyLevelsDownloadingGenericError", ExceptionActionType.BackToMainMenu);
                }
            }
        }


        public void PopulateStreakRewards(int[] dailyLevelsStreakRewards)
        {
            ResetStreak();

            for (int i = 1; i <= 7; i++)
            {
                if (i != 7)
                {
                    _dailyLevelsStreakContainer.Q<VisualElement>($"daily-levels__streak-day-{i}").Q<Label>("daily-levels__streak-day-number").text = dailyLevelsStreakRewards[i - 1].ToString();
                }
            }

            UpdateStreak(DailyLevelsLevelManager.DailyLevelsStreak);
        }


        /// <summary>
        /// Populates the daily levels view with the levels of the day
        /// </summary>
        /// <param name="textures"></param>
        public void PopulateLevels(Level[] dailyLevels)
        {
            // Start by emptying the daily levels view
            ResetLevels();

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
                dailyLevelTemplateClone.name = $"daily-levels__level-{(int)difficulty - 1}";
                dailyLevelTemplateClone.AddToClassList("daily-levels__level-button");
                _dailyLevelsButtonsContainer.Add(dailyLevelTemplateClone);

                string id = dailyLevels[(int)difficulty - 1].id;

                Action dailyLevelClickedHandler = () => { DailyLevelClicked(id); };
                _dailyLevelsButtonClickAction[difficulty] = dailyLevelClickedHandler;

                Button dailyLevelCloneButton = dailyLevelTemplateClone.Q<Button>("default-level-selection-template__container-button");
                dailyLevelCloneButton.clicked += dailyLevelClickedHandler;


                dailyLevelCloneButton.Q<Label>("default-level-selection-template__level-id").text = id;

                dailyLevelTemplateClone.AddToClassList("level-selection-item");

                if (PlayerManager.Instance.DailyLevelsDataManager.IsLevelUnlocked(id) || difficulty == DailyLevelDifficulty.VeryEasy)
                {
                    dailyLevelTemplateClone.RemoveFromClassList("level-selection-item-locked");
                    dailyLevelTemplateClone.AddToClassList("level-selection-item-unlocked");

                    float bestTime = PlayerManager.Instance.DailyLevelsDataManager.GetLevelBestTime(id);

                    if (!bestTime.AlmostEquals(0))
                    {
                        dailyLevelCloneButton.Q<Label>("default-level-selection-template__time-label").text = $"{bestTime.ToString("00.00")}s";

                        for (int i = 0; i < 3; i++)
                        {
                            if (bestTime < dailyLevels[(int)difficulty - 1].times[i])
                            {
                                dailyLevelCloneButton.Q<VisualElement>($"default-level-selection-template__star-{3 - i}-image").AddToClassList("star-active");
                            }
                        }
                    }
                }
                else
                {
                    dailyLevelTemplateClone.RemoveFromClassList("level-selection-item-unlocked");
                    dailyLevelTemplateClone.AddToClassList("level-selection-item-locked");
                }
            }

            if (_dailyLevelsButtonsContainer.childCount == 5)
            {
                AreDailyLevelsLoaded = true;
            }

            LoadThumbnails(dailyLevels);
        }


        private async void LoadThumbnails(Level[] dailyLevels)
        {
            // Wait for the level manager and the maze to be initialized (obstacles, materials...)
            await UniTask.WhenAll(
                UniTask.WaitUntil(() => LevelManager.Initialized),
                UniTask.WaitUntil(() => Maze.ObstaclesBundleLoaded)
            );

            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            Camera.onPostRender += OnPostRenderCallback;

            Camera camera = GameObject.Find("DailyLevelsThumbnailCamera").GetComponent<Camera>();
            camera.targetTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);

            for (int i = 0; i < dailyLevels.Length; i++)
            {
                Level level = dailyLevels[i];

                Vector3 offset = new Vector3(0, 0, 100);

                LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).Maze.BuildMaze(LevelType.DailyLevel, level, offset);

                Maze.RenderAllObstacles(LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).Maze.obstaclesList, LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).Maze.obstacles, LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).Maze.obstaclesTypesMap, "DailyLevelsThumbnailMaze");

                camera.transform.position = level.cameraPosition + offset;
                camera.transform.rotation = level.cameraRotation;

                _saveThumbnailToTexture = i + 1;

                await UniTask.WaitUntil(() => _saveThumbnailToTexture == 0);

                LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).Maze.ClearMaze(GameObject.Find("DailyLevelsThumbnailMaze"));
            }

            RenderTexture renderTexture = camera.activeTexture;

            camera.targetTexture = null;
            RenderTexture.active = null;
            renderTexture.Release();
            GameObject.Destroy(renderTexture);

            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            Camera.onPostRender -= OnPostRenderCallback;
        }


        private void OnEndCameraRendering(ScriptableRenderContext _, Camera camera)
        {
            OnPostRenderCallback(camera);
        }


        private void OnPostRenderCallback(Camera camera)
        {
            if (_saveThumbnailToTexture != 0 && camera.targetDisplay == 7)
            {
                RenderTexture renderTexture = camera.activeTexture;
                camera.Render();

                Texture2D texture = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.ARGB32, false);
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
                texture.Apply();

                _dailyLevelsButtonsContainer.ElementAt(_saveThumbnailToTexture - 1).Q<Button>("default-level-selection-template__container-button").style.backgroundImage = texture;

                _saveThumbnailToTexture = 0;
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
                    if (difficulty == DailyLevelDifficulty.Unknown)
                    {
                        continue;
                    }

                    _dailyLevelsButtonsContainer.ElementAt((int)difficulty - 1).Q<Button>("default-level-selection-template__container-button").clicked -= _dailyLevelsButtonClickAction[difficulty];
                }
            }

            // Empty the action dictionary and the scroll view content
            _dailyLevelsButtonClickAction.Clear();
            _dailyLevelsButtonsContainer.Clear();

            AreDailyLevelsLoaded = false;
        }


        private void DailyLevelClicked(string levelId)
        {
            // If the level is locked, return
            if (!PlayerManager.Instance.DailyLevelsDataManager.IsLevelUnlocked(levelId))
            {
                return;
            }

            UIManager.Instance.Show(UIViewType.Playing);

            LevelEvents.LevelModeUpdated?.Invoke(LevelType.DailyLevel);

            LevelManager.Instance.LoadLevel(levelId);
        }


        /// <summary>
        /// Get the visual element of the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public VisualElement GetLevelFromId(string levelId)
        {
            foreach (VisualElement child in _dailyLevelsButtonsContainer.Children())
            {
                if (child.Q<Label>("default-level-selection-template__level-id").text == levelId)
                {
                    return child;
                }
            }

            return null;
        }


        /// <summary>
        /// Removes the lock icon for the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        public void UnlockLevel(string levelId)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.RemoveFromClassList("level-selection-item-locked");
                level.AddToClassList("level-selection-item-unlocked");
            }
        }


        /// <summary>
        /// Update the time and stars of the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="time"></param>
        public void SetLevelTime(string levelId, float time)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.Q<Label>("default-level-selection-template__time-label").text = $"{time.ToString("00.00")}s";
            }

            // Set the level's stars
            Debug.Log($"number of stars: {LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).GetNumberOfStarsForLevel(levelId)}");
            for (int i = 3; i > 3 - LevelManager.GetLevelManagerModeInstance(LevelType.DailyLevel).GetNumberOfStarsForLevel(levelId); i--)
            {
                level.Q<VisualElement>($"default-level-selection-template__star-{i}-image").AddToClassList("star-active");
            }
        }


        /// <summary>
        /// Update the streak days visual elements from day 1 to the given streak day
        /// </summary>
        /// <param name="streak"></param>
        public void UpdateStreak(int streak)
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
        public void ResetStreak()
        {
            foreach (VisualElement streakDay in _dailyLevelsStreakDays)
            {
                streakDay.RemoveFromClassList("daily-levels__streak-day-active");
                streakDay.AddToClassList("daily-levels__streak-day-inactive");
            }
        }


        public VisualElement GetStreakElement()
        {
            return _dailyLevelsStreakContainer;
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
                _dailyLevelsButtonsContainer.Q<VisualElement>($"daily-levels__level-{i}").RemoveFromClassList("level-locked");
            }
        }


        /// <summary>
        /// Resets the levels to their initial state
        /// </summary>
        public void ResetLevels()
        {
            // Only reset the levels if they are loaded.
            // Furthermore, since this method is called at midnight utc, the UI might not be displayed if the user is not in the daily levels view
            // And so, we don't want to display the error if the UI is not displayed
            // (eg: the player might be offline playing another mode and so daily levels might not be loaded but it is then normal and no error should be displayed)
            // try
            // {
            //     if (isEnabled && !AreDailyLevelsLoaded)
            //     {
            //         throw new CouldNotLoadLevelException("No daily levels loaded");
            //     }
            // }
            // catch (Exception e)
            // {
            //     DisplayDefaultDailyLevelsLoadingException(e);
            //     return;
            // }
            if (!AreDailyLevelsLoaded)
            {
                return;
            }

            // Lock the levels, hide the stars and time
            for (int i = 0; i < 5; i++)
            {
                VisualElement dailyLevelButton = _dailyLevelsButtonsContainer.Q<VisualElement>($"daily-levels__level-{i}");

                // Lock the levels except the first one
                if (i != 0)
                {
                    dailyLevelButton.RemoveFromClassList("level-selection-item-unlocked");
                    dailyLevelButton.AddToClassList("level-selection-item-locked");
                }

                // Hide the stars and time
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-1-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-2-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<VisualElement>("default-level-selection-template__star-3-image").RemoveFromClassList("star-active");
                dailyLevelButton.Q<Label>("default-level-selection-template__time-label").text = "";
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

            VisualElement dailyLevelButton = _dailyLevelsButtonsContainer.Q<VisualElement>($"daily-levels__level-{(int)difficulty + 1}");

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
            DateTime midnightUtc = GameManager.Instance.GetUtcNowTime().MidnightUtc().AddMinutes(1);

            while (isEnabled)
            {
                // Calculate the time left until midnight utc
                TimeSpan timeLeft = midnightUtc - GameManager.Instance.GetUtcNowTime();

                _dailyLevelsUpdatesInLabel.text = $"Updates in: {timeLeft.ToString(@"hh\:mm\:ss")}";
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }


        private async void StartSpinningCircleLoadingAnimation(VisualElement element)
        {
            while (element.style.display == DisplayStyle.Flex)
            {
                await DOTween.To(() => 0, x => element.transform.rotation = Quaternion.Euler(0, 0, x), 360, 2f).SetEase(Ease.Linear).AsyncWaitForCompletion();
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