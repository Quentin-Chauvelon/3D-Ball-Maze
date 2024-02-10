using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class PermanentView : ScreenView
    {
        // Visual Elements
        private Button _settingsButton;


        public PermanentView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _settingsButton = _root.Q<Button>("permanent__settings-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            // Open the settings modal view
            _settingsButton.clickable.clicked += () => { UIManager.Instance.Show(UIViews.Settings); };
        }


        public override void Show()
        {
            base.Show();
        }
    }
}