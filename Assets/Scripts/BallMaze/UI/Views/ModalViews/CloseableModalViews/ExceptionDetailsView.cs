using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class ExceptionDetailsView : ModalView
    {
        public override bool isCloseable => true;

        // Visual Elements
        private Button _closeButton;
        private TextField _errorStackTraceTextField;
        private Button _copyToClipboardButton;
        private Button _sendToSupportButton;


        public ExceptionDetailsView(VisualElement root) : base(root)
        {
            _errorStackTraceTextField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            _errorStackTraceTextField.textSelection.isSelectable = false;
        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _errorStackTraceTextField = _root.Q<TextField>("exception-details__error-message-stacktrace-text-field");
            _copyToClipboardButton = _root.Q<Button>("exception-details__copy-to-clipboard-button");
            _sendToSupportButton = _root.Q<Button>("exception-details__send-to-support-button");
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clickable.clicked += () =>
            {
                UIManager.Instance.Hide(UIViewType.ExceptionDetails);
            };

            _copyToClipboardButton.clicked += CopyToClipboard;

            _sendToSupportButton.clicked += () =>
            {
                UIManager.Instance.Show(UIViewType.ExceptionSendToSupport);
            };
        }


        public override void Hide()
        {
            // Hide the send to support view
            if (UIManager.Instance.UIViews[UIViewType.ExceptionSendToSupport].isEnabled)
            {
                UIManager.Instance.Hide(UIViewType.ExceptionSendToSupport);
            }

            base.Hide();
        }


        /// <summary>
        /// Copy the error's stack trace to the clipboard
        /// see: https://github.com/sanukin39/UniClipboard/blob/master/Assets/UniClipboard/UniClipboard.cs for OS based copying
        /// </summary>
        private void CopyToClipboard()
        {
            GUIUtility.systemCopyBuffer = _errorStackTraceTextField.text;
        }


        public void UpdateException(ExceptionObject exception)
        {
            _errorStackTraceTextField.value = exception.stackTrace;

            (UIManager.Instance.UIViews[UIViewType.ExceptionSendToSupport] as ExceptionSendToSupportView).UpdateException(exception);
        }
    }
}