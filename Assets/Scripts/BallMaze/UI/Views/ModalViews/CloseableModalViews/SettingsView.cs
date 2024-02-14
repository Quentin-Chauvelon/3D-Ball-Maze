using BallMaze.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class SettingsView : ModalView
    {
        public override bool isCloseable => true;

        // Top visual element
        private VisualElement _settingsContainer;

        // Visual elements
        private Button _closeButton;
        private RadioButton _controlsJoystickRadioButton;
        private RadioButton _controlsAccelerometerRadioButton;
        private Toggle _joystickPositionToggle;
        private RadioButtonGroup _startOnRadioButtonGroup;
        private SliderInt _cooldownDurationSlider;
        private Label _accelerometerUnavailableLabel;
        private DropdownField _languageDropdown;

        public SettingsView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _closeButton = _root.Q<Button>("settings__close-button");
            _controlsJoystickRadioButton = _root.Q<RadioButton>("settings__joystick-radio-button");
            _controlsAccelerometerRadioButton = _root.Q<RadioButton>("settings__accelerometer-radio-button");
            _joystickPositionToggle= _root.Q<Toggle>("settings__joystick-position-toggle");
            _startOnRadioButtonGroup = _root.Q<RadioButtonGroup>("settings__start-on-radio-button-group");
            _cooldownDurationSlider = _root.Q<SliderInt>("settings__cooldown-duration-slider");
            _accelerometerUnavailableLabel = _root.Q<Label>("settings__controls-accelerometer-unavailable-label");
            _languageDropdown = _root.Q<DropdownField>("settings__language-dropdown");

            _startOnRadioButtonGroup.choices = Enum.GetNames(typeof(StartOnSettings));

            // Add the available languages to the dropdown and select the current language
            AddLanguagesToDropdown();
        }


        protected override void RegisterButtonCallbacks()
        {
            // Close the settings modal view
            _closeButton.clickable.clicked += () => { UIManager.Instance.Hide(UIViewType.Settings); };

            // Set the controls
            _controlsJoystickRadioButton.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue)
                {
                    SettingsManager.Instance.controls = ControlsSettings.Joystick;
                    SettingsEvents.UpdatedControls?.Invoke();
                }
            });

            _controlsAccelerometerRadioButton.RegisterValueChangedCallback((evt) =>
            {
                if (SettingsManager.Instance.hasAccelerometer && evt.newValue)
                {
                    SettingsManager.Instance.controls = ControlsSettings.Accelerometer;
                    SettingsEvents.UpdatedControls?.Invoke();
                }
            });

            // Set the joystick position
            _joystickPositionToggle.RegisterValueChangedCallback((evt) =>
            {
                SettingsManager.Instance.joystickPosition = evt.newValue ? JoystickPosition.Right : JoystickPosition.Left;
            });

            // Set the start method
            _startOnRadioButtonGroup.RegisterValueChangedCallback((evt) =>
            {
                SettingsManager.Instance.startOn = (StartOnSettings)evt.newValue;
                SettingsEvents.UpdatedStartMethod?.Invoke();
            });

            // Set the cooldown duration
            _cooldownDurationSlider.RegisterValueChangedCallback((evt) =>
            {
                SettingsManager.Instance.cooldownDuration = evt.newValue;
                SettingsEvents.UpdatedStartMethod?.Invoke();
            });

            _languageDropdown.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                SettingsManager.Instance.language = evt.newValue;
            }); 
        }


        /// <summary>
        /// Update the UI to match the current settings
        /// </summary>
        public void Update()
        {
            // Update the controls
            if (SettingsManager.Instance.controls == ControlsSettings.Accelerometer && SettingsManager.Instance.hasAccelerometer)
            {
                _controlsAccelerometerRadioButton.value = true;
            } else
            {
                _controlsJoystickRadioButton.value = true;
            }

            // Update the joystick position. Toggle off if the joystick is on the left, on if it's on the right
            _joystickPositionToggle.value = SettingsManager.Instance.joystickPosition == JoystickPosition.Right ? true : false;

            // Update the start method
            _startOnRadioButtonGroup.value = (int)SettingsManager.Instance.startOn;

            // Update the cooldown duration
            _cooldownDurationSlider.value = SettingsManager.Instance.cooldownDuration;

            // Update the language
            int index = SettingsManager.Instance.language.IndexOf("(");
            if (index >= 0)
            {
                _languageDropdown.value = SettingsManager.Instance.language.Substring(0, index - 1);
            }
        }


        /// <summary>
        /// Update the UI to match the current accelerometer availability.
        /// Enables/Disables the accelerometer toggle button and shows/hides the accelerometer unavailable label.
        /// </summary>
        private void UpdateAccelerometerAvailability()
        {
            if (SettingsManager.Instance.hasAccelerometer)
            {
                _controlsAccelerometerRadioButton.SetEnabled(true);
                _accelerometerUnavailableLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _controlsAccelerometerRadioButton.SetEnabled(false);
                _accelerometerUnavailableLabel.style.display = DisplayStyle.Flex;
            }
        }


        public override void Show()
        {
            base.Show();

            UpdateAccelerometerAvailability();
        }


        /// <summary>
        /// Adds the available languages to the language dropdown and selects the current language.
        /// </summary>
        private void AddLanguagesToDropdown()
        {
            List<string> availableLanguages = new List<string>();
            string currentLocale = "";

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
                string localeName = locale.name;

                // Remove the country code from the locale name. Eg: "English (en)" -> "English"
                int index = localeName.IndexOf("(");
                if (index >= 0)
                {
                    localeName = localeName.Substring(0, index - 1);
                }

                availableLanguages.Add(localeName);

                // If the locale is the selected locale, set it as the current locale
                if (locale == LocalizationSettings.SelectedLocale)
                {
                    currentLocale = localeName;
                }
            }

            _languageDropdown.choices = availableLanguages;
            _languageDropdown.value = currentLocale;
        }
    }
}