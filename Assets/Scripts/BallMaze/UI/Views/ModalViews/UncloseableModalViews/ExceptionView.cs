using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class ExceptionView : ModalView
    {
        public override bool isCloseable => false;

        // Visual Elements
        private Label _errorMessageLabel;
        private Button _seeMoreLabelButton;
        private Button _seeMoreIconButton;
        private Button _actionButton;


        public ExceptionView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _errorMessageLabel = _root.Q<Label>("exception__error-message-label");
            _seeMoreLabelButton = _root.Q<Button>("exception__see-more-label-button");
            _seeMoreIconButton = _root.Q<Button>("exception__see-more-icon-button");
            _actionButton = _root.Q<Button>("exception__action-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _seeMoreLabelButton.clicked += SeeMoreButtonClicked;
            _seeMoreIconButton.clicked += SeeMoreButtonClicked;

            _actionButton.clicked += () =>
            {
                UIManager.Instance.UIViews[UIViewType.Exception].Hide();
                ExceptionManager.Instance.ActionButtonClicked();
            };
        }


        /// <summary>
        /// Display the exception details UI
        /// </summary>
        private void SeeMoreButtonClicked()
        {
            UIManager.Instance.Show(UIViewType.ExceptionDetails);
        }


        public override void Hide()
        {
            ExceptionManager.isError = false;

            // Hide the exception details view if it's open
            if (UIManager.Instance.UIViews[UIViewType.ExceptionDetails].isEnabled)
            {
                UIManager.Instance.Hide(UIViewType.ExceptionDetails);
            }

            base.Hide();
        }


        public void UpdateException(ExceptionObject exception)
        {
            _errorMessageLabel.text = exception.friendlyMessage;
            _actionButton.text = exception.action.name;

            (UIManager.Instance.UIViews[UIViewType.ExceptionDetails] as ExceptionDetailsView).UpdateException(exception);
        }
    }
}