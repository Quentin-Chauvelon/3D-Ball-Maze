using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityExtensionMethods;
using DG.Tweening;
using BallMaze.Events;


namespace BallMaze.UI
{
    public class SecondChanceView : ModalView
    {
        public override bool isCloseable => false;


        private const int RADIAL_PROGRESS_DURATION = 4;

        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.80f;

        // Visual Elements
        private RadialProgress _secondChanceRadialProgress;
        private Button _secondChanceButton;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private Button _tryAgainButton;

        private int PULSE_ANIMATION_INTERVAL = 700;
        private const bool IS_RADIAL_PROGRESS_ENABLED = false;


        public SecondChanceView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _secondChanceRadialProgress = _root.Q<RadialProgress>("second-chance__radial-progress");
            _secondChanceButton = _root.Q<Button>("second-chance__second-chance-button");
            _defaultLevelsListButton = _root.Q<Button>("second-chance__default-levels-list-button");
            _homeButton = _root.Q<Button>("second-chance__home-button");
            _tryAgainButton = _root.Q<Button>("second-chance__try-again-button");

            LevelEvents.LevelModeUpdated += (levelType) => { SwitchLevelTypeSource(levelType); };
        }


        protected override void RegisterButtonCallbacks()
        {
            _secondChanceButton.clicked += () =>
            {
                LevelManager.Instance.UseSecondChance();

                // Show the permanent view elements back
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.Playing);

                UIManager.Instance.Hide(UIViewType.SecondChance);
            };

            _defaultLevelsListButton.clicked += () =>
            {
                switch (LevelManager.Instance.levelType)
                {
                    case LevelType.Default:
                        UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
                        break;
                    case LevelType.DailyLevel:
                        UIManager.Instance.Show(UIViewType.DailyLevels);
                        break;
                    default:
                        UIManager.Instance.Show(UIViewType.MainMenu);
                        break;

                }

                UIManager.Instance.Hide(UIViewType.SecondChance);
            };

            _homeButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.MainMenu);
                UIManager.Instance.Hide(UIViewType.SecondChance);
            };

            _tryAgainButton.clicked += () =>
            {
                LevelManager.Instance.ResetLevel();

                // Show the permanent view elements back
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.Playing);

                UIManager.Instance.Hide(UIViewType.SecondChance);
            };
        }


        public override void Show()
        {
            base.Show();

            (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.SecondChance);

            // Tween the modal view from the top to the center of the screen
            UIUtitlities.TweenModalViewFromTop(_root, UI_HEIGHT_PERCENTAGE + 0.05f);

#pragma warning disable CS0162 // Unreachable code detected
            if (IS_RADIAL_PROGRESS_ENABLED)
            {
                StartRadialProgress();
            }
            else
            {
                // Slow the pulse down if the radial progress is disabled, otherwise it will be too fast and annoying
                PULSE_ANIMATION_INTERVAL = 1800;

                // Set the progress to 0 so that the radial progress is not visible
                _secondChanceRadialProgress.progress = 0;
            }
#pragma warning restore CS0162 // Unreachable code detected

            StartPulseAnimation();
        }


        public override async void Hide()
        {
            // Tween the modal view back to the top of the screen
            await UIUtitlities.TweenModalViewToTopAndWait(_root, UI_HEIGHT_PERCENTAGE + 0.05f);

            base.Hide();
        }


        /// <summary>
        /// Start the pulse animation for the second chance button
        /// </summary>
        /// <returns></returns>
        private async void StartPulseAnimation()
        {
            while (isEnabled)
            {
                await UniTask.Delay(700);
                await DOTween.To(() => 1f, x => _secondChanceRadialProgress.style.scale = new StyleScale(new Vector2(x, x)), 1.1f, 0.1f).SetEase(Ease.OutQuint).AsyncWaitForCompletion();
                await DOTween.To(() => 1.1f, x => _secondChanceRadialProgress.style.scale = new StyleScale(new Vector2(x, x)), 1f, 0.4f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
            }
        }


        /// <summary>
        /// Decrease the radial progress every frame (acts as a timer) and restart the level once it reaches 0
        /// </summary>
        private async void StartRadialProgress()
        {
            // Multiply by 50 instead of 60 because FixedUpdate is called 50 times per second
            float step = 100f / (RADIAL_PROGRESS_DURATION * 50);

            // Decrease the progress every frame while the view is visible
            while (isEnabled)
            {
                _secondChanceRadialProgress.progress -= step;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (_secondChanceRadialProgress.progress <= 0)
                {
                    LevelManager.Instance.UseSecondChance();
                    UIManager.Instance.Hide(UIViewType.SecondChance);

                    break;
                }
            }

            // Reset the progress
            _secondChanceRadialProgress.progress = 100;
        }


        /// <summary>
        /// Change the UI layout based on the level type the user is playing
        /// </summary>
        private void SwitchLevelTypeSource(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    _defaultLevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    break;
                case LevelType.DailyLevel:
                    _defaultLevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    break;
                default:
                    break;
            }
        }
    }
}