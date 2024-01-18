using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI {
    public class ModalBackgroundView : UIView
    {
        public override bool isModal => true;

        // Visual Elements
        private Button _backgroundButton;


        public ModalBackgroundView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _backgroundButton = _root.Q<Button>("modal-background__button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _backgroundButton.clickable.clicked += () => { UIManager.Instance.Back(); };
        }
    }
}