using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;


namespace BallMaze.UI
{
    public class LevelFailedView : ModalView
    {
        public override bool isCloseable => false;

        // Visual Elements
        private VisualElement _aspectRatioContainer;
        private Label _restartWhereYouFailedLabel;
        private Button _restartCoinsButton;
        private Button _restartAdButton;
        private Button _unlimitedRestartsIAPButton;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private RadialProgress _tryAgainRadialProgress;
        private Button _tryAgainButton;

        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.8f;

        private const int RADIAL_PROGRESS_DURATION = 4;

        private const bool IS_RADIAL_PROGRESS_ENABLED = true;


        public LevelFailedView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _aspectRatioContainer = _root.Q<VisualElement>("level-failed__aspect-ratio-container");
            _restartWhereYouFailedLabel = _root.Q<Label>("level-failed__level-failed-label");
            _restartCoinsButton = _root.Q<Button>("level-failed__restart-where-you-failed-coins-button");
            _restartAdButton = _root.Q<Button>("level-failed__restart-where-you-failed-ad-button");
            _unlimitedRestartsIAPButton = _root.Q<Button>("level-failed__unlimited-restarts-iap-button");
            _defaultLevelsListButton = _root.Q<Button>("level-failed__default-levels-list-button");
            _homeButton = _root.Q<Button>("level-failed__home-button");
            _tryAgainRadialProgress = _root.Q<RadialProgress>("level-failed__try-again-radial-progress");
            _tryAgainButton = _root.Q<Button>("level-failed__try-again-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _restartCoinsButton.clicked += () =>
            {
                // TODO: pay coins to restart level (second chance), check if has enough coins and hide the UI + restart level if it's the case
            };

            _restartAdButton.clicked += () =>
            {
                // TODO: show ad and restart level if the ad was watched (second chance) (callback function?)
            };

            _unlimitedRestartsIAPButton.clicked += () =>
            {
                // TODO: show the unlimited restarts IAP UI, and if bought, show the second chance UI?
            };

            _defaultLevelsListButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
                UIManager.Instance.Hide(UIViewType.LevelFailed);
            };

            _homeButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.MainMenu);
                UIManager.Instance.Hide(UIViewType.LevelFailed);
            };

            _tryAgainButton.clicked += () =>
            {
                LevelManager.Instance.ResetLevel();

                // Show the permanent view elements back
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.Playing);

                UIManager.Instance.Hide(UIViewType.LevelFailed);
            };
        }



        public override void Show()
        {
            base.Show();

            (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.LevelFailed);

            // Tween the modal view from the top to the center of the screen
            UIUtitlities.TweenModalViewFromTop(_root, UI_HEIGHT_PERCENTAGE + 0.05f);

#pragma warning disable CS0162 // Unreachable code detected
            if (IS_RADIAL_PROGRESS_ENABLED)
            {
                StartRadialProgress();
            }
            else
            {
                // Set the progress to 0 so that the radial progress is not visible
                _tryAgainRadialProgress.progress = 0;
            }
#pragma warning restore CS0162 // Unreachable code detected
        }



        public override async void Hide()
        {
            // Tween the modal view back to the top of the screen
            await UIUtitlities.TweenModalViewToTopAndWait(_root, UI_HEIGHT_PERCENTAGE + 0.05f);

            base.Hide();
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
                _tryAgainRadialProgress.progress -= step;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (_tryAgainRadialProgress.progress <= 0)
                {
                    LevelManager.Instance.ResetLevel();
                    UIManager.Instance.Hide(UIViewType.LevelFailed);

                    break;
                }
            }

            // Reset the progress
            _tryAgainRadialProgress.progress = 100;
        }


        /// <summary>
        /// Change the UI layout based on the level type the user is playing
        /// </summary>
        public void SwitchLevelTypeSource(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    _restartWhereYouFailedLabel.text = "Restart where you failed?";
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.AddToClassList("default-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    break;
                case LevelType.DailyLevel:
                    _restartWhereYouFailedLabel.text = "Restart where you failed?";
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.AddToClassList("default-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                case LevelType.RankedLevel:
                    _restartWhereYouFailedLabel.text = "Try again?";
                    _aspectRatioContainer.RemoveFromClassList("default-levels");
                    _aspectRatioContainer.AddToClassList("ranked-level");
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
        }
    }
}