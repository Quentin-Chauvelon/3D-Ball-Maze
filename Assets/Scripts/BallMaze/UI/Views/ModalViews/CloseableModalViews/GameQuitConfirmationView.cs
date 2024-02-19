using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class GameQuitConfirmationView : ModalView
    {
        public override bool isCloseable => true;


        // Visual Elements
        private Button _closeButton;
        private Button _cancelButton;
        private Button _quitButton;


        public GameQuitConfirmationView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _cancelButton = _root.Q<Button>("game-quit-confirmation__cancel-button");
            _quitButton = _root.Q<Button>("game-quit-confirmation__quit-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clickable.clicked += () => { Hide(); };

            _cancelButton.clickable.clicked += () => { Hide(); };

            _quitButton.clickable.clicked += () =>
            {
                GameManager.Instance.QuitGame();
                Hide();
            };
        }
    }
}