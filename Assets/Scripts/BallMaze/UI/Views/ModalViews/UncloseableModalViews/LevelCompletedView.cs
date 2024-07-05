using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Threading.Tasks;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class LevelCompletedView : ModalView
    {
        public override bool isCloseable => false;

        private VisualElement[] _stars = new VisualElement[3];

        // Visual Elements
        private Label _coinsEarnedLabel;
        private VisualElement _coinsEarnedImage;
        private Button _doubleCoinsAdButton;
        private Button _doubleCoinsIAPButton;
        private Label _timeLabel;
        private Label _secondTimeInfoLabel;
        private Button _defaultLevelsListButton;
        private Button _homeButton;
        private Button _nextLevelButton;
        private Button _tryAgainButton;
        private VisualElement _newBestTimeFrame;

        private Sequence _starsTweenSequence;

        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.8f;

        // The number of milliseconds to wait before animating the next star
        private const int DELAY_BETWEEN_STARS_ANIMATION = 300;

        // The number of milliseconds to wait before starting to animate the stars
        private const int DELAY_BEFORE_STARS_ANIMATION = 800;

        // The duration of the star scale animation in milliseconds
        private const int STAR_SCALE_ANIMATION_DURATION = 150;

        // The number of milliseconds to wait before the last star animation and the first coin animation
        private const int DELAY_BETWEEN_STARS_AND_COINS_ANIMATION = 500;


        public LevelCompletedView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _stars[0] = _root.Q<VisualElement>("level-completed__star-1");
            _stars[1] = _root.Q<VisualElement>("level-completed__star-2");
            _stars[2] = _root.Q<VisualElement>("level-completed__star-3");
            _coinsEarnedLabel = _root.Q<Label>("level-completed__coins-earned-label");
            _coinsEarnedImage = _root.Q<VisualElement>("level-completed__coins-earned-coin-image");
            _doubleCoinsAdButton = _root.Q<Button>("level-completed__double-coins-ad-button");
            _doubleCoinsIAPButton = _root.Q<Button>("level-completed__double-coins-iap-button");
            _timeLabel = _root.Q<Label>("level-completed__time-label");
            _secondTimeInfoLabel = _root.Q<Label>("level-completed__second-time-info-label");
            _defaultLevelsListButton = _root.Q<Button>("level-completed__default-levels-list-button");
            _homeButton = _root.Q<Button>("level-completed__home-button");
            _nextLevelButton = _root.Q<Button>("level-completed__next-level-button");
            _tryAgainButton = _root.Q<Button>("level-completed__try-again-button");
            _newBestTimeFrame = _root.Q<VisualElement>("level-completed__new-best-time");
        }


        protected override void RegisterButtonCallbacks()
        {
            _doubleCoinsAdButton.clicked += () =>
            {
                // TODO: show ad and double the coins earned (give + update label) if the ad was watched
            };

            _doubleCoinsIAPButton.clicked += () =>
            {
                // TODO: show the double coins permanently IAP UI, and if bought, double the coins + update Ui (doubleds)
            };

            _defaultLevelsListButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
                UIManager.Instance.Hide(UIViewType.LevelCompleted);
            };

            _homeButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.MainMenu);
                UIManager.Instance.Hide(UIViewType.LevelCompleted);
            };

            _nextLevelButton.clicked += () =>
            {
                string nextLevel = LevelManager.Instance.GetNextLevel();

                if (!String.IsNullOrEmpty(nextLevel))
                {
                    LevelManager.Instance.LoadLevel(nextLevel);
                }

                // Show the permanent view elements back
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.Playing);

                UIManager.Instance.Hide(UIViewType.LevelCompleted);
            };

            _tryAgainButton.clicked += () =>
            {
                LevelManager.Instance.ResetLevel();

                // Show the permanent view elements back
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.Playing);

                UIManager.Instance.Hide(UIViewType.LevelCompleted);
            };
        }


        public override void Show()
        {
            base.Show();

            // Hide all the elements of the permanent view
            (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).UpdateVisibleElements(UIViewType.LevelCompleted);

            // Tween the modal view from the top to the center of the screen
            UIUtitlities.TweenModalViewFromTop(_root, UI_HEIGHT_PERCENTAGE + 0.05f);
        }



        public override async void Hide()
        {
            // Tween the modal view back to the top of the screen
            await UIUtitlities.TweenModalViewToTopAndWait(_root, UI_HEIGHT_PERCENTAGE + 0.05f);

            base.Hide();

            // If the player hides the UI before the stars tween finishes, kill it
            if (_starsTweenSequence != null)
            {
                _starsTweenSequence.Kill(true);
            }

            // If the UI elements have not been loaded yet, return
            if (_coinsEarnedLabel == null)
            {
                return;
            }

            _coinsEarnedLabel.text = "0";

            // Hide the stars
            _stars[0].RemoveFromClassList("star-active");
            _stars[1].RemoveFromClassList("star-active");
            _stars[2].RemoveFromClassList("star-active");

            // Reset the colors of the stars since the animation doesn't use the class
            _stars[0].style.unityBackgroundImageTintColor = StyleKeyword.Null;
            _stars[1].style.unityBackgroundImageTintColor = StyleKeyword.Null;
            _stars[2].style.unityBackgroundImageTintColor = StyleKeyword.Null;
        }


        /// <summary>
        /// Update the time labels and the number of stars the user just gained
        /// </summary>
        /// <param name="time">The time the user got in the level</param>
        /// <param name="starsAlreadygained">The amount of stars the user already had</param>
        /// <param name="starsGained">The amount of stars the user got in the level</param>
        /// <param name="secondTimeInfo">A string which depends on the context. For instance, if the player hasn't got all the stars
        /// it will be "NEXT STAR: XX:XXs, if the player got all the stars, it will be "BEST TIME: XX:XXs" or "NEW BEST TIME: XX:XXs"...
        /// </param>
        public void UpdateTime(float time, int starsAlreadygained, int starsGained, string secondTimeInfo)
        {
            // Show the stars the user got
            DisplayEarnedStars(starsAlreadygained, starsGained);

            _timeLabel.text = $"TIME: {time.ToString("00.00")}s";
            _secondTimeInfoLabel.text = secondTimeInfo;
        }


        private async void DisplayEarnedStars(int starsAlreadygained, int starsGained)
        {
            // Make the stars already gained active immediately
            for (int i = 0; i < starsAlreadygained; i++)
            {
                _stars[i].AddToClassList("star-active");
            }

            // Delay to wait before tweening the stars
            await UniTask.Delay(TimeSpan.FromMilliseconds(DELAY_BEFORE_STARS_ANIMATION));

            float endScale = 1.4f;

            // Tween the stars the user just gained
            for (int i = starsAlreadygained; i < starsGained; i++)
            {
                // If the UI has been hidden, stop tweening the stars
                if (!isEnabled)
                {
                    break;
                }

                await UniTask.Delay(TimeSpan.FromMilliseconds(DELAY_BETWEEN_STARS_ANIMATION));

                if (_starsTweenSequence != null)
                {
                    _starsTweenSequence.Kill();
                }

                _starsTweenSequence = DOTween.Sequence();

                // Tween the star bigger
                _starsTweenSequence.Append(DOTween.To(() => 1f, x => _stars[i].style.scale = new StyleScale(new Vector2((float)x, (float)x)), endScale, STAR_SCALE_ANIMATION_DURATION / 1000f));

                // Tween the star back to its original size after the tween completes
                _starsTweenSequence.Append(DOTween.To(() => endScale, x => _stars[i].style.scale = new StyleScale(new Vector2((float)x, (float)x)), 1f, STAR_SCALE_ANIMATION_DURATION / 1000f));

                // Tween the color of the star
                _starsTweenSequence.Insert(0, DOTween.To(() => _stars[i].style.unityBackgroundImageTintColor.value, x => _stars[i].style.unityBackgroundImageTintColor = x, new Color(1f, 1f, 0f), _starsTweenSequence.Duration()));

                await _starsTweenSequence.Play().AsyncWaitForCompletion();

                _starsTweenSequence = null;
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(DELAY_BETWEEN_STARS_AND_COINS_ANIMATION));

            DOTween.To(() => 0, x => _coinsEarnedLabel.text = x.ToString(), LevelManager.Instance.GetCoinsEarnedForLevel(starsAlreadygained, starsGained), 0.5f);

            // Animate the coins if the player has gained some coins
            if (isEnabled && starsGained - starsAlreadygained > 0)
            {
                // The image width is less than the width of the element itself, but the height is the same.
                // Thus, to get the position of the image, we need to add half of the width of the element and subtract half of the width of the image
                // Then, we add 10 pixels to compensate the width of the text that is going from one character to two characters (0 -> 10/20/30...)
                (UIManager.Instance.UIViews[UIViewType.Animation] as AnimationView).AnimateCoins(
                    new Vector2(_coinsEarnedImage.worldBound.x + _coinsEarnedImage.worldBound.size.x / 2 - _coinsEarnedImage.worldBound.size.y / 2 + 10, _coinsEarnedImage.worldBound.yMin),
                    (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).GetCoinsImagePosition()
                );
            }
        }


        /// <summary>
        /// Update the visibility of the new best time frame
        /// </summary>
        /// <param name="visible"></param>
        public void DisplayNewBestTimeFrame(bool visible)
        {
            _newBestTimeFrame.style.display = visible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }


        /// <summary>
        /// Return the duration of the full level completed animation based on the number of stars gained
        /// from the moment the player reaches the target to the end of the last coin animation.
        /// </summary>
        /// <param name="numberOfStarsGained"></param>
        /// <returns></returns>
        public static int GetLevelCompletedAnimationDuration(int numberOfStarsGained)
        {
            // The delay is calculated as follows:
            // - (delay between each star + star animation duration) * number of stars
            // - 1.5s between the target reached and the start of the UI animation
            // - 0.3s for the UI fade in
            // - 0.8s before the animation starts
            // - 0.6s to start the animation while the coins animation is still playing
            // - 0.5s for the delay between the last star animation and the start of the coin animation
            return
                LevelManagerBase.TARGET_REACHED_UI_DELAY +
                (int)(UIUtitlities.UI_TWEEN_DURATION * 1000) +
                DELAY_BEFORE_STARS_ANIMATION +
                numberOfStarsGained * (DELAY_BETWEEN_STARS_ANIMATION + STAR_SCALE_ANIMATION_DURATION) +
                DELAY_BETWEEN_STARS_AND_COINS_ANIMATION +
                AnimationView.DELAY_BETWEEN_COINS_ANIMATION * AnimationView.DEFAULT_NUMBER_OF_COINS_TO_ANIMATE;
        }
    }
}