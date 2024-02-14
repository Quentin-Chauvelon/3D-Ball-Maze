using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI {
    public class ModeSelectionView : ScreenView
    {
        private ScrollView _modeSelectionContainerScrollView;
        private Button _defaultLevelsButton;
        private Button _dailylevelsButton;
        private Button _rankedButton;
        private Button _eventButton;

        // The height of the mode relative to its container
        private const float MODE_HEIGHT_PERCENTAGE = 0.8f;


        public ModeSelectionView(VisualElement root) : base(root)
        {

        }

        protected override void SetVisualElements()
        {
            _modeSelectionContainerScrollView = _root.Q<ScrollView>("mode-selection__modes-container-scroll-view");
            _defaultLevelsButton = _root.Q<Button>("mode-selection__default-levels-button");
            _dailylevelsButton = _root.Q<Button>("mode-selection__daily-levels-button");
            _rankedButton = _root.Q<Button>("mode-selection__ranked-button");
            _eventButton = _root.Q<Button>("mode-selection__event-button");

            // Update the size of the children when the size of the container changes
            _modeSelectionContainerScrollView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        protected override void RegisterButtonCallbacks()
        {
            _defaultLevelsButton.clickable.clicked += () =>
            {
                // We are playing classic, so disable the event UI
                (UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).SetEventModeSelected(false);
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
            };

            _dailylevelsButton.clickable.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.DailyLevels);
            };

            _eventButton.clickable.clicked += () =>
            {
                // We are playing event, so enable the event UI
                (UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).SetEventModeSelected(true);
                UIManager.Instance.Show(UIViewType.DefaultLevelSelection);
            };
        }


        /// <summary>
        /// Update the size of all modes so that they are always square
        /// </summary>
        /// <param name="evt"></param>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size)
            {
                return;
            }

            float height = (float)(_modeSelectionContainerScrollView.resolvedStyle.height * MODE_HEIGHT_PERCENTAGE);

            if (height > 1)
            {
                foreach (VisualElement child in _modeSelectionContainerScrollView.Children())
                {
                    child.style.width = height;
                    child.style.height = height;
                }
            }
        }
    }
}