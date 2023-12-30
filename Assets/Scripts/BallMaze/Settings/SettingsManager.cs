using UnityEngine;


namespace BallMaze
{
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

        // Controls
        private static bool _useJoystick = true;
        private static bool _useAccelerometer = false;
        private static bool _hasAccelerometer = false;


        // Start level methods
        private static bool _startAfterCooldown = true;
        private static bool _startOnTouch = false;
        private static int _cooldownDuration = 3;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            SetStartMethod("touch"); // TODO: remove this line
        }


        /// <summary>
        /// Sets the controls to use and disables the others.
        /// </summary>
        /// <param name="controls">A string representing the controls to use. Can be "joystick" or "accelerometer"</param>
        private void SetControls(string controls)
        {
            if (controls == "joystick")
            {
                _useJoystick = true;
                _useAccelerometer = false;
            }
            else if (_hasAccelerometer && controls == "accelerometer")
            {
                _useJoystick = false;
                _useAccelerometer = true;
            }
        }


        /// <summary>
        /// Sets the start method to use and disables the others.
        /// </summary>
        /// <param name="method">A string representing the method to use. Can be "cooldown" or "touch"</param>
        private void SetStartMethod(string method)
        {
            if (method == "cooldown")
            {
                _startAfterCooldown = true;
                _startOnTouch = false;
            }
            else if (method == "touch")
            {
                _startAfterCooldown = false;
                _startOnTouch = true;
            }
        }


        public bool UsesJoystick()
        {
            return _useJoystick;
        }


        public bool UsesAccelerometer()
        {
            return _useAccelerometer;
        }


        public bool StartAfterCooldown()
        {
            return _startAfterCooldown;
        }


        public bool StartOnTouch()
        {
            return _startOnTouch;
        }
    }
}