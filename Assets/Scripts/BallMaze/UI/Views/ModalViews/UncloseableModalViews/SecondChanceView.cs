using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class SecondChanceView : ModalView
    {
        public override bool isCloseable => false;


        private const int RADIAL_PROGRESS_DURATION = 6;

        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.80f;

        // Visual Elements
        private RadialProgress _secondChanceRadialProgress;
        private Button _secondChanceButton;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private Button _tryAgainButton;


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
        }


        protected override void RegisterButtonCallbacks()
        {
            _secondChanceButton.clicked += () =>
            {
                LevelManager.Instance.UseSecondChance();
                UIManager.Instance.Hide(UIViewType.SecondChance);
            };

            _defaultLevelsListButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
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
                UIManager.Instance.Hide(UIViewType.SecondChance);
            };
        }


        public override void Show()
        {
            base.Show();

            // Tween the modal view from the top to the center of the screen
            UIUtitlities.TweenModalViewFromTop(_root, UI_HEIGHT_PERCENTAGE);

            StartRadialProgress();
        }


        public override async void Hide()
        {
            // Tween the modal view back to the top of the screen
            await UIUtitlities.TweenModalViewToTopAndWait(_root, UI_HEIGHT_PERCENTAGE);

            base.Hide();
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
                _secondChanceRadialProgress.progress -= step;
                await UniTask.Yield();

                if (_secondChanceRadialProgress.progress <= 0)
                {
                    LevelManager.Instance.UseSecondChance();
                    UIManager.Instance.Hide(UIViewType.SecondChance);
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
                    _defaultLevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
        }
    }
}