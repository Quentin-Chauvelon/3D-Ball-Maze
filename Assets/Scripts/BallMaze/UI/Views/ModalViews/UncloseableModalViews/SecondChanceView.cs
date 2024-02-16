using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Redcode.Awaiting;


namespace BallMaze.UI
{
    public class SecondChanceView : ModalView
    {
        public override bool isCloseable => false;


        private const int RADIAL_PROGRESS_DURATION = 6;

        // Visual Elements
        private RadialProgress _radialProgress;
        private Button _secondChanceButton;
        private Button _defaultlevelsListButton;
        private Button _homeButton;
        private Button _tryAgainButton;


        public SecondChanceView(VisualElement root) : base(root)
        {
            Show();
        }


        protected override void SetVisualElements()
        {
            _radialProgress = _root.Q<RadialProgress>("second-chance__radial-progress");
            _secondChanceButton = _root.Q<Button>("second-chance__second-chance-button");
            _defaultlevelsListButton = _root.Q<Button>("second-chance__default-levels-list-button");
            _homeButton = _root.Q<Button>("second-chance__home-button");
            _tryAgainButton = _root.Q<Button>("second-chance__try-again-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _secondChanceButton.clicked += () =>
            {
                // TODO: Call the level manager second chance
                Hide();
            };

            _defaultlevelsListButton.clicked += () =>
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
                // TODO: Call the level manager to restart the level
                Hide();
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
                _radialProgress.progress -= step;
                await new WaitForFixedUpdate();

                if (_radialProgress.progress <= 0)
                {
                    // TODO: Call the level manager second chance
                    Hide();
                }
            }

            // Reset the progress
            _radialProgress.progress = 100;
        }


        /// <summary>
        /// Change the UI layout based on the level type the user is playing
        /// </summary>
        private void SwitchLevelTypeSource(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Default:
                    _defaultlevelsListButton.style.display = DisplayStyle.Flex;
                    _homeButton.style.display = DisplayStyle.None;
                    break;
                case LevelType.DailyLevel:
                    _defaultlevelsListButton.style.display = DisplayStyle.None;
                    _homeButton.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
        }
    }
}