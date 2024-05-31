using System.Collections;
using System.Collections.Generic;
using BallMaze.UI;
using UnityEngine;

namespace BallMaze
{
    public class LevelTimer
    {
        private float time;
        private bool isRunning;

        private PlayingView _playingView;

        public LevelTimer()
        {
            time = 0;
            isRunning = false;

            _playingView = UIManager.Instance.UIViews[UIViewType.Playing] as PlayingView;
        }

        private void SetTimerText()
        {
            _playingView.SetTimerText(time);
        }

        public void SetTime(float time)
        {
            this.time = time;
            SetTimerText();
        }

        public float GetTime()
        {
            return time;
        }

        public void Update(float deltaTime)
        {
            if (isRunning)
            {
                time += deltaTime;

                SetTimerText();
            }
        }

        public void Reset()
        {
            Pause();

            time = 0;

            SetTimerText();
        }


        public void Start()
        {
            Resume();
        }


        public void Resume()
        {
            isRunning = true;
        }


        public void Pause()
        {
            isRunning = false;
        }
    }
}