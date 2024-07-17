using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class PlayingView : ScreenView
    {
        // Visual Elements
        private VisualElement _timerContainer;
        private Label _timerLabel;
        private Label _touchToStartLabel;
        // The animation uses two labels. This way, one can be tweened out while the other is tweened in.
        private Label _currentCountdownLabel;
        private Label _nextCountdownLabel;

        private float _countdownTime = 3f;
        // Used to update the countdown label only when the full second changes
        private int _lastCountdownFullSecond = 4;

        // The font size of the countdown labels in percent
        private const int COUNTDOWN_FONT_SIZE_PERCENTAGE = 200;


        public PlayingView(VisualElement root) : base(root)
        {

        }

        protected override void SetVisualElements()
        {
            _timerContainer = _root.Q<VisualElement>("playing__timer-container");
            _timerLabel = _root.Q<Label>("playing__timer-label");
            _touchToStartLabel = _root.Q<Label>("playing__touch-to-start-label");
            _currentCountdownLabel = _root.Q<Label>("playing__countdown-label-1");
            _nextCountdownLabel = _root.Q<Label>("playing__countdown-label-2");

            _currentCountdownLabel.style.fontSize = Length.Percent(COUNTDOWN_FONT_SIZE_PERCENTAGE);
            _nextCountdownLabel.style.fontSize = Length.Percent(COUNTDOWN_FONT_SIZE_PERCENTAGE);
        }

        public void SetTimerText(float time)
        {
            _timerLabel.text = time.ToString("00.00");
        }

        public void SetTouchToStartLabelVisibility(bool visible)
        {
            _touchToStartLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetCountdownLabelVisibility(bool visible)
        {
            _currentCountdownLabel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetCountdownText(string text)
        {
            _currentCountdownLabel.text = text;
        }

        /// <summary>
        /// Update the countdown time and animate the label.
        /// </summary>
        /// <returns>True if the countdown has reached 0, false otherwise</returns>
        public bool UpdateCountdown()
        {
            _countdownTime -= Time.deltaTime;

            // Update the countdown label only if a full second has changed, allows to animate it more easily
            if (Math.Ceiling(_countdownTime) != _lastCountdownFullSecond)
            {
                _lastCountdownFullSecond = (int)Math.Ceiling(_countdownTime);

                // If the countdown is at 3, don't play the tween out animation,
                // instead simply hide the label
                if (_lastCountdownFullSecond == SettingsManager.Instance.countdownDuration)
                {
                    _currentCountdownLabel.style.display = DisplayStyle.None;
                }
                else
                {
                    // Keep a reference to the label that will be tweened out before swapping the labels,
                    // otherwise it will start animating the other label
                    Label tweenOutLabel = _currentCountdownLabel;

                    // Tween out animation: fade out + decrease the size
                    DOTween.To(() => 1, x => tweenOutLabel.style.opacity = x, 0, 0.3f).SetEase(Ease.OutQuad);
                    DOTween.To(() => COUNTDOWN_FONT_SIZE_PERCENTAGE, x => tweenOutLabel.style.fontSize = Length.Percent(x), 10, 0.3f).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                        {
                            tweenOutLabel.style.display = DisplayStyle.None;
                        }
                    );
                }

                (_currentCountdownLabel, _nextCountdownLabel) = (_nextCountdownLabel, _currentCountdownLabel);

                _currentCountdownLabel.style.display = DisplayStyle.Flex;
                _currentCountdownLabel.style.top = Length.Percent(20);
                _currentCountdownLabel.style.opacity = 0;
                _currentCountdownLabel.style.fontSize = Length.Percent(COUNTDOWN_FONT_SIZE_PERCENTAGE);

                // Update the label's text
                if (_lastCountdownFullSecond == 0)
                {
                    _currentCountdownLabel.text = "GO!";
                }
                else
                {
                    _currentCountdownLabel.text = _lastCountdownFullSecond.ToString();
                }

                // If the countdown is at -1, don't play the tween in animation and hide the label
                if (_lastCountdownFullSecond == -1)
                {
                    _currentCountdownLabel.style.display = DisplayStyle.None;
                }
                else
                {
                    // Keep a reference to the label that will be tweened in before swapping the labels
                    // on the next iteration, otherwise it will start animating the other label
                    Label tweenInLabel = _currentCountdownLabel;

                    // Tween in animation: fade in + move up
                    DOTween.To(() => 0, x => tweenInLabel.style.opacity = x, 1, 0.2f).SetEase(Ease.OutQuad);
                    DOTween.To(() => 20, x => tweenInLabel.style.top = Length.Percent(x), 0, 0.2f).SetEase(Ease.OutQuad);
                }
            }

            // If the countdown has reached 0, hide the label and return true
            if (_countdownTime <= -1)
            {
                SetCountdownLabelVisibility(false);

                return true;
            }

            return false;
        }


        public void ResetCountdown()
        {
            _countdownTime = SettingsManager.Instance.countdownDuration + 1;

            // Set the default to something other than 3, otherwise the label won't animate for the first number since full second is already 3
            _lastCountdownFullSecond = SettingsManager.Instance.countdownDuration + 1;

            _currentCountdownLabel.style.display = DisplayStyle.None;
            _nextCountdownLabel.style.display = DisplayStyle.None;
            _currentCountdownLabel.text = "3";
            _nextCountdownLabel.text = "3";
        }
    }
}