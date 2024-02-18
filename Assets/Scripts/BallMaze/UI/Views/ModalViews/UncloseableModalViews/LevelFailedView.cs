using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Redcode.Awaiting;


namespace BallMaze.UI
{
    public class LevelFailedView : ModalView
    {
        public override bool isCloseable => false;


        private const int RADIAL_PROGRESS_DURATION = 6;

        // Visual Elements
        private VisualElement _aspectRatioContainer;
        private Button _restartCoinsButton;
        private Button _restartAdButton;
        private Button _unlimitedRestartsIAPButton;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private RadialProgress _tryAgainRadialProgress;
        private Button _tryAgainButton;


        public LevelFailedView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _aspectRatioContainer = _root.Q<VisualElement>("level-failed__aspect-ratio-container");
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
                Hide();
            };

            _homeButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.MainMenu);
                Hide();
            };

            _tryAgainButton.clicked += () =>
            {
                // TODO: restart the level
            };
        }


        public override void Show()
        {
            base.Show();

            StartRadialProgress();
        }


        /// <summary>
        /// Decrease the radial progress every frame (acts as a timer) and restart the level once it reaches 0
        /// </summary>
        private async void StartRadialProgress()
        {
            float step = 100f / (RADIAL_PROGRESS_DURATION * 60);

            // Decrease the progress every frame while the view is visible
            while (isEnabled)
            {
                _tryAgainRadialProgress.progress -= step;
                await new WaitForFixedUpdate();

                if (_tryAgainRadialProgress.progress <= 0)
                {
                    // TODO: Try again
                    Hide();
                }
            }

            // Reset the progress
            _tryAgainRadialProgress.progress = 100;
        }


        /// <summary>
        /// Change the UI layout based on the level type the user is playing
        /// </summary>
        private void SwitchLevelTypeSource(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.AddToClassList("default-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    break;
                case LevelType.DailyLevel:
                    _aspectRatioContainer.RemoveFromClassList("ranked-level");
                    _aspectRatioContainer.AddToClassList("default-levels");
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                case LevelType.RankedLevel:
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