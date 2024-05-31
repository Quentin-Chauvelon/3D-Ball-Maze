using System.Collections;
using System.Collections.Generic;
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
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clicked += () =>
            {
                LevelManager.Instance.ResumeLevel();
                UIManager.Instance.Hide(UIViewType.Skip);
            };

            _unlockNextLevelCoinsButton.clicked += () =>
            {
                // TODO: pay coins to unlock the next level check if has enough coins and hide the UI + unlock next level if it's the case
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
                LevelManager.Instance.ResumeLevel();
                UIManager.Instance.Hide(UIViewType.Skip);
            };
        }


        public override void Show()
        {
            base.Show();

            LevelManager.Instance.PauseLevel();
        }
    }
}