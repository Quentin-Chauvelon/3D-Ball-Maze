using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class SkipView : ModalView
    {
        public override bool isCloseable => true;


        // Visual Elements
        private Button _closeButton;
        private Button _unlockNextLevelCoinsButton;
        private Button _unlockNextLevelAdButton;
        private Button _unlockAllLevelsIAPButton;
        private Button _resumeButton;

        // Set to false when the UI is shown and is set to true whenever the player interacts with the UI (eg: try again, reset...).
        // This is needed because once the UI is hidden, we need to run the default behavior (ie: resume the level)
        // if and only if the player hasn't interacted with the UI. Otherwise, the player will be stuck and unable to play
        private bool _hasInteractedBeforeHide = false;


        // The percentage of the screen height the UI will take. Must match the value in the UXML file
        private const float UI_HEIGHT_PERCENTAGE = 0.8f;

        private const int COINS_TO_UNLOCK_NEXT_LEVEL = 100;


        public SkipView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _unlockNextLevelCoinsButton = _root.Q<Button>("skip__unlock-next-level-coins-button");
            _unlockNextLevelAdButton = _root.Q<Button>("skip__unlock-next-level-ad-button");
            _unlockAllLevelsIAPButton = _root.Q<Button>("skip__unlock-all-levels-iap-button");
            _resumeButton = _root.Q<Button>("skip__resume-button");

            // Update the cost label to unlock the next level
            _unlockNextLevelCoinsButton.Q<Label>("skip__unlock-next-level-coins-button-cost-label").text = COINS_TO_UNLOCK_NEXT_LEVEL.ToString();
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.ResumeLevel();
                UIManager.Instance.Hide(UIViewType.Skip);
            };

            _unlockNextLevelCoinsButton.clicked += () =>
            {
                if (!PlayerManager.Instance.CoinsManager.HasEnoughCoins(COINS_TO_UNLOCK_NEXT_LEVEL))
                {
                    return;
                }

                string nextLevelId = LevelManager.Instance.GetNextLevel();

                if (nextLevelId != null)
                {
                    // Unlock the next level if it hasn't already been unlocked
                    if (!PlayerManager.Instance.DefaultLevelsDataManager.IsLevelUnlocked(nextLevelId))
                    {
                        PlayerManager.Instance.DefaultLevelsDataManager.UnlockLevel(nextLevelId);

                        PlayerManager.Instance.CoinsManager.UpdateCoins(-COINS_TO_UNLOCK_NEXT_LEVEL);

                        LevelManager.Instance.LoadLevel(nextLevelId);

                        UIManager.Instance.Hide(UIViewType.Skip);
                    }
                }
            };

            _unlockNextLevelAdButton.clicked += () =>
            {
                // TODO: show ad and unlock next level if the ad was watched
            };

            _unlockAllLevelsIAPButton.clicked += () =>
            {
                // TODO: show the unlock all levels IAP UI, and if bought, unlock all levels + go to the next level
            };

            _resumeButton.clicked += () =>
            {
                _hasInteractedBeforeHide = true;

                LevelManager.Instance.ResumeLevel();
                UIManager.Instance.Hide(UIViewType.Skip);
            };

            PlayerEvents.CoinsUpdated += (int amount, int _) =>
            {
                // Update the style of the button based on if the player has enough coins
                // to unlock the next level or not
                if (amount >= COINS_TO_UNLOCK_NEXT_LEVEL)
                {
                    _unlockNextLevelCoinsButton.AddToClassList("button-enabled");
                    _unlockNextLevelCoinsButton.RemoveFromClassList("button-disabled");
                }
                else
                {
                    _unlockNextLevelCoinsButton.RemoveFromClassList("button-enabled");
                    _unlockNextLevelCoinsButton.AddToClassList("button-disabled");
                }
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
    }
}