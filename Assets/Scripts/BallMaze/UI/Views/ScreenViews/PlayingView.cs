using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class PlayingView : ScreenView
    {
        // Visual Elements
        private VisualElement _timerContainer;
        private Label _timerLabel;

        public PlayingView(VisualElement root) : base(root)
        {

        }

        protected override void SetVisualElements()
        {
            _timerContainer = _root.Q<VisualElement>("playing__timer-container");
            _timerLabel = _root.Q<Label>("playing__timer-label");
        }

        public void SetTimerText(float time)
        {
            _timerLabel.text = $"{time.ToString("00.00")}s";
        }
    }
}