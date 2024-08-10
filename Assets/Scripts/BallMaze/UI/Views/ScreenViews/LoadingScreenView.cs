using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public enum LoadingIndicatorType
    {
        ProgressBar,
        CircularAnimation
    }


    public class LoadingScreenView : ScreenView
    {
        // Visual Elements
        private ProgressBar _loadingProgressBar;
        private VisualElement _circularAnimation;

        public LoadingScreenView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _loadingProgressBar = _root.Q<ProgressBar>("loading-screen__loading-progress-bar");
            _circularAnimation = _root.Q<VisualElement>("loading-screen__circular-animation");
        }


        public void InitializeLoadingScreen(LoadingIndicatorType loadingIndicatorType)
        {
            Show();

            if (loadingIndicatorType == LoadingIndicatorType.ProgressBar)
            {
                _loadingProgressBar.style.display = DisplayStyle.Flex;
                _circularAnimation.style.display = DisplayStyle.None;
                SetProgressBarValue(0);
            }
            else
            {
                _circularAnimation.style.display = DisplayStyle.Flex;
                _loadingProgressBar.style.display = DisplayStyle.None;
                StartCircularAnimation();
            }
        }


        public void SetProgressBarValue(int value)
        {
            _loadingProgressBar.value = value;
            _loadingProgressBar.title = $"{value}%";
        }


        public async void StartCircularAnimation()
        {
            while (isEnabled)
            {
                await DOTween.To(() => 0, x => _circularAnimation.transform.rotation = Quaternion.Euler(0, 0, x), 360, 2f).SetEase(Ease.Linear).AsyncWaitForCompletion();
            }
        }
    }
}