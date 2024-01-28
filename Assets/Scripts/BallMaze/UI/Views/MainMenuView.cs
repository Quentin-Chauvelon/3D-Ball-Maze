using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class MainMenuView : ScreenView
    {
        // Visual Elements
        private Button _defaultLevelSelectionButton;


        public MainMenuView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _defaultLevelSelectionButton = _root.Q<Button>("main-menu__default-level-selection-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            // Open the default level selection view
            _defaultLevelSelectionButton.clickable.clicked += () => { UIManager.Instance.Show(UIViews.DefaultLevelSelection); };
        }
    }
}