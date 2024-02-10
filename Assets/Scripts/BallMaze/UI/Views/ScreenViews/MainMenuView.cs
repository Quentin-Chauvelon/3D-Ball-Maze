using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class MainMenuView : ScreenView
    {
        // Visual Elements
        private Button _profileButton;
        private Button _shopButton;
        private Button _skinsButton;
        private Button _playButton;
        private Label _bottomInfoLabel;


        public MainMenuView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _profileButton = _root.Q<Button>("main-menu__profile-button");
            _shopButton = _root.Q<Button>("main-menu__shop-button");
            _skinsButton = _root.Q<Button>("main-menu__skins-button");
            _playButton = _root.Q<Button>("main-menu__play-button");
            _bottomInfoLabel = _root.Q<Label>("main-menu__bottom-info");
        }


        protected override void RegisterButtonCallbacks()
        {
            
        }
    }
}