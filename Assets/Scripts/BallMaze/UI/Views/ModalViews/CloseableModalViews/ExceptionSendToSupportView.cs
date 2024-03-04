using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class ExceptionSendToSupportView : ModalView
    {
        public override bool isCloseable => true;

        private const int MAX_ADDITIONAL_INFORMATION_LENGTH = 400;

        // Visual Elements
        private Button _closeButton;
        private Label _errorMessageLabel;
        private TextField _additionalInformationTextField;
        private Label _additionalInformationCharacterCountLabel;
        private Button _sendButton;

        private bool _isPlaceHolderActive = false;


        public ExceptionSendToSupportView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("close-button-template__close-button");
            _errorMessageLabel = _root.Q<Label>("exception-send-to-support__error-message-label");
            _additionalInformationTextField = _root.Q<TextField>("exception-send-to-support__additional-information-text-field");
            _additionalInformationCharacterCountLabel = _root.Q<Label>("exception-send-to-support__additional-information-character-count-label");
            _sendButton = _root.Q<Button>("exception-send-to-support__send-button");

            _additionalInformationTextField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
            // Set the max length of the additional information text field and update the character count label
            _additionalInformationTextField.maxLength = MAX_ADDITIONAL_INFORMATION_LENGTH;
            UpdateCharacterCount();
            UpdatePlaceholderVisibility();

            // When the user types in the additional information text field, update the character count and the placeholder visibility
            _additionalInformationTextField.RegisterCallback<ChangeEvent<string>>(e =>
            {
                UpdateCharacterCount();

                // Pass the new value as a parameter, otherwise when the user types,
                // the placeholder by setting the value to an empty string, thus erasing what the user typed
                // And so, we need to set the value back to the user's input
                UpdatePlaceholderVisibility(
                    string.IsNullOrEmpty(e.newValue)
                        ? ""
                        : e.newValue[0].ToString()
                );
            });

            // Hide the placeholder when the user focuses on the text field
            _additionalInformationTextField.RegisterCallback<FocusInEvent>(_ => UpdatePlaceholderVisibility());
        }


        protected override void RegisterButtonCallbacks()
        {
            _closeButton.clickable.clicked += () =>
            {
                UIManager.Instance.Hide(UIViewType.ExceptionSendToSupport);
            };

            _sendButton.clicked += () =>
            {
                ExceptionManager.Instance.SendErrorToSupport(_isPlaceHolderActive ? "" : _additionalInformationTextField.text);
            };
        }


        private void UpdateCharacterCount()
        {
            if (_isPlaceHolderActive)
            {
                _additionalInformationCharacterCountLabel.text = $"0/{MAX_ADDITIONAL_INFORMATION_LENGTH}";
            }
            else
            {
                _additionalInformationCharacterCountLabel.text = $"{_additionalInformationTextField.text.Length}/{MAX_ADDITIONAL_INFORMATION_LENGTH}";
            }
        }


        private void UpdatePlaceholderVisibility(string typedText = "")
        {
            if (_isPlaceHolderActive)
            {
                _isPlaceHolderActive = false;

                // We need to use SetValueWithoutNotify, otherwise the ChangeEvent is called again which causes an infinite loop and crashes the game
                _additionalInformationTextField.SetValueWithoutNotify(typedText);
                _additionalInformationTextField.RemoveFromClassList("text-field__placeholder-active");
                _additionalInformationTextField.AddToClassList("text-field__placeholder-inactive");
            }
            else if (!_isPlaceHolderActive && string.IsNullOrEmpty(_additionalInformationTextField.value))
            {
                _isPlaceHolderActive = true;

                // We need to use SetValueWithoutNotify, otherwise the ChangeEvent is called again which causes an infinite loop and crashes the game
                _additionalInformationTextField.SetValueWithoutNotify(LocalizationSettings.StringDatabase.GetLocalizedString("UITable", "ExceptionSendToSupport_AdditionalInformationPlaceholderText"));
                _additionalInformationTextField.RemoveFromClassList("text-field__placeholder-inactive");
                _additionalInformationTextField.AddToClassList("text-field__placeholder-active");
            }
        }


        public void UpdateException(ExceptionObject exception)
        {
            _errorMessageLabel.text = $"Error: {exception.friendlyMessage}";
        }
    }
}