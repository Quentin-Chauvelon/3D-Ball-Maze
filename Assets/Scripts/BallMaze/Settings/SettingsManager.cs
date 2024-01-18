using BallMaze.UI;
using Unity.Profiling;
using UnityEngine;


namespace BallMaze
{
    public enum ControlsSettings
    {
        Joystick,
        Accelerometer
    }


    public enum StartOnSettings
    {
        Cooldown,
        Touch
    }


    public enum JoystickPosition
    {
        Left,
        Right
    }

    public class SettingsManager : MonoBehaviour
    {
        // Singleton pattern
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("SettingsManager is null!");
                }
                return _instance;
            }
        }

        // Joystick UI elements
        private RectTransform _joystickRectTransform; // Container
        private RectTransform _joystickBackgroundRectTransform; // Background

        // Controls
        private ControlsSettings _controls;
        public ControlsSettings controls
        {
            get { return _controls; }
            set
            {
                // If the device doesn't have an accelerometer, enable the joystick
                if (!hasAccelerometer)
                {
                    _controls = ControlsSettings.Joystick;
                }

                _controls = value;

                PlayerPrefs.SetInt("controls", (int)_controls);
            }
        }

        private JoystickPosition _joystickPosition;
        public JoystickPosition joystickPosition
        {
            get { return _joystickPosition; }
            set
            {
                _joystickPosition = value;

                if (_joystickPosition == JoystickPosition.Left)
                {
                    _joystickRectTransform.anchorMin = new Vector2(0, 0);
                    _joystickRectTransform.anchorMax = new Vector2(JOYSTICK_WIDTH, JOYSTICK_HEIGHT);

                    _joystickBackgroundRectTransform.anchorMin = new Vector2(0, 0);
                    _joystickBackgroundRectTransform.anchorMax = new Vector2(0, 0);
                    _joystickBackgroundRectTransform.position = new Vector3(DISTANCE_FROM_EDGE, DISTANCE_FROM_EDGE, 0);
                }
                else if (_joystickPosition == JoystickPosition.Right)
                {
                    _joystickRectTransform.anchorMin = new Vector2(0, 0);
                    _joystickRectTransform.anchorMax = new Vector2(1 - JOYSTICK_WIDTH, JOYSTICK_HEIGHT);

                    _joystickBackgroundRectTransform.anchorMin = new Vector2(1, 0);
                    _joystickBackgroundRectTransform.anchorMax = new Vector2(1, 0);
                    _joystickBackgroundRectTransform.position = new Vector3(-DISTANCE_FROM_EDGE, DISTANCE_FROM_EDGE, 0);
                }

                PlayerPrefs.SetInt("joystickPosition", (int)_joystickPosition);
            }
        }

        public bool hasAccelerometer => SystemInfo.supportsAccelerometer;

        // Start level methods
        private StartOnSettings _startOn;
        public StartOnSettings startOn
        {
            get { return _startOn; }
            set
            {
                _startOn = value;

                PlayerPrefs.SetInt("startOn", (int)_startOn);
            }
        }

        private int _cooldownDuration = 3;
        public int cooldownDuration
        {
            get { return _cooldownDuration; }
            set
            {
                // The duration must be between 1 and 10 seconds
                _cooldownDuration = Mathf.Clamp(value, 1, 10);

                PlayerPrefs.SetInt("cooldownDuration", _cooldownDuration);
            }
        }

        // Constants
        private const int DISTANCE_FROM_EDGE = 300;
        private const float JOYSTICK_WIDTH = 0.4f;
        private const float JOYSTICK_HEIGHT = 0.65f;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _joystickRectTransform = FindObjectOfType<FloatingJoystick>().GetComponent<RectTransform>();
            _joystickBackgroundRectTransform = _joystickRectTransform.Find("Background").GetComponent<RectTransform>();

            LoadSettings();
        }


        private void Start()
        {
            UIManager.Instance.UpdateSettings();
        }


        /// <summary>
        /// Loads the settings from the PlayerPrefs.
        /// If no settings are found, the default settings are used.
        /// </summary>
        private void LoadSettings()
        {
            // Load the controls. Defaults to Joystick
            _controls = PlayerPrefs.HasKey("controls")
                ? (ControlsSettings)PlayerPrefs.GetInt("controls")
                : ControlsSettings.Joystick;

            // Load the joystick joystickPosition. Defaults to Left
            _joystickPosition = PlayerPrefs.HasKey("joystickPosition")
                ? (JoystickPosition)PlayerPrefs.GetInt("joystickPosition")
                : JoystickPosition.Left;

            // Load the start method. Defaults to Touch
            _startOn = PlayerPrefs.HasKey("startOn")
                ? (StartOnSettings)PlayerPrefs.GetInt("startOn")
                : StartOnSettings.Touch;

            // Load the cooldown duration. Defaults to 3 seconds
            _cooldownDuration = PlayerPrefs.HasKey("cooldownDuration")
                ? PlayerPrefs.GetInt("cooldownDuration")
                : 3;
        }
    }
}