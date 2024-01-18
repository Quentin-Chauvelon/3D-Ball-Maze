using System;
using UnityEngine;


namespace BallMaze
{
    public class Controls : MonoBehaviour
    {
        private FloatingJoystick _joystick;
        private Vector3 _lastJoystickPosition = Vector3.zero;

        private const float MAX_ORIENTATION_CHANGE_PER_SECOND = 12f;

        private bool _areControlsEnabled = false;
        private bool _areControlsVisible = false;

        private const short MAX_MAZE_ORIENTATION = 10;


        void Awake()
        {
            _joystick = FindObjectOfType<FloatingJoystick>();
        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// Get the orientation based on the selected controls method (accelerometer or joystick).
        /// </summary>
        /// <returns></returns>
        public Quaternion GetControlsOrientation()
        {
            // If the controls are not enabled, the player can't rotate the maze
            if (!_areControlsEnabled)
            {
                return Quaternion.identity;
            }

            if (SettingsManager.Instance.UsesJoystick())
            {
                return GetJoystickOrientation();
            }
            else if (SettingsManager.Instance.UsesAccelerometer())
            {
                return GetAccelerometerOrientation();
            }
            else
            {
                return Quaternion.identity;
            }
        }


        public Vector2 GetRawControlsDirection()
        {
            // If the controls are not enabled, the player can't rotate the maze
            if (!_areControlsEnabled)
            {
                return Vector2.zero;
            }

            if (SettingsManager.Instance.UsesJoystick())
            {
                return Vector2.zero - _joystick.Direction;
            }
            else if (SettingsManager.Instance.UsesAccelerometer())
            {
                return Vector2.zero;
            }
            else
            {
                return Vector2.zero;
            }
        }


        private Quaternion GetJoystickOrientation()
        {
            float orientationChange = MAX_ORIENTATION_CHANGE_PER_SECOND * Time.deltaTime;

            // Get the joystick position. Clamp the vector so that it doesn't rotate too much at once
            Vector3 currentJoystickPosition = new Vector3(
                Mathf.Clamp(-_joystick.Vertical, _lastJoystickPosition.x - orientationChange, _lastJoystickPosition.x + orientationChange), 
                0f,
                Mathf.Clamp(_joystick.Horizontal, _lastJoystickPosition.z - orientationChange, _lastJoystickPosition.z + orientationChange)
            );

            // Save the last joystick position
            _lastJoystickPosition = currentJoystickPosition;

            currentJoystickPosition *= MAX_MAZE_ORIENTATION;

            return Quaternion.Euler(currentJoystickPosition);
        }


        private Quaternion GetAccelerometerOrientation()
        {
            return Quaternion.identity;
        }


        private void EnableControls(bool enabled)
        {
            _areControlsEnabled = enabled;
        }


        private void ShowControls(bool visible)
        {
            _areControlsVisible = visible;

            // Enable or disable the joystick
            _joystick.Enable(visible);
        }


        /// <summary>
        /// Enables and shows the controls.
        /// </summary>
        public void EnableAndShowControls()
        {
            EnableControls(true);
            ShowControls(true);
        }


        /// <summary>
        /// Enables and shows the controls.
        /// </summary>
        public void EnableAndHideControls()
        {
            EnableControls(true);
            ShowControls(false);
        }


        /// <summary>
        /// Enables and shows the controls.
        /// </summary>
        public void DisableAndShowControls()
        {
            EnableControls(false);
            ShowControls(true);
        }


        /// <summary>
        /// Enables and shows the controls.
        /// </summary>
        public void DisableAndHideControls()
        {
            EnableControls(false);
            ShowControls(false);
        }
    }
}