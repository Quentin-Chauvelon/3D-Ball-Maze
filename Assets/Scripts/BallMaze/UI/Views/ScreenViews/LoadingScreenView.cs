using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class LoadingScreenView : ScreenView
    {
        // Visual Elements
        private ProgressBar _loadingProgressBar;

        public LoadingScreenView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _loadingProgressBar = _root.Q<ProgressBar>("loading-screen__loading-progress-bar");
        }


        public void SetProgressBarValue(int value)
        {
            _loadingProgressBar.value = value;
            _loadingProgressBar.title = $"{value}%";
        }
    }
}