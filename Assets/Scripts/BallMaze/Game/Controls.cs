using BallMaze.Events;
using System;
using UnityEngine;


namespace BallMaze
{
    public class Controls : MonoBehaviour
    {
        private FloatingJoystick _joystick;
        private Vector3 _lastJoystickPosition = Vector3.zero;

        [SerializeField]
        private float MAX_ORIENTATION_CHANGE_PER_SECOND = 8f;

        private bool _areControlsEnabled = false;
        private bool _areControlsVisible = false;

        public const short MAX_MAZE_ORIENTATION = 10;


        void Awake()
        {
            _joystick = FindObjectOfType<FloatingJoystick>();

            SettingsEvents.UpdatedControls += () => { UpdateJoystickVisibility(_areControlsVisible); };
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

            if (SettingsManager.Instance.controls == ControlsSettings.Joystick)
            {
                return GetJoystickOrientation();
            }
            else if (SettingsManager.Instance.controls == ControlsSettings.Accelerometer)
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

            if (SettingsManager.Instance.controls == ControlsSettings.Joystick)
            {
                return Vector2.zero - _joystick.Direction;
            }
            else if (SettingsManager.Instance.controls == ControlsSettings.Accelerometer)
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


        /// <summary>
        /// Updates the joystick visibility according to the given parameter and the selected controls method.
        /// </summary>
        /// <param name="visible"></param>
        private void UpdateJoystickVisibility(bool visible)
        {
            // If the joystick should be visible and the controls are set to joystick, show the joystick, otherwise hide it
            if (SettingsManager.Instance.controls == ControlsSettings.Joystick && visible)
            {
                _joystick.Enable(true);
            }
            else
            {
                _joystick.Enable(false);
            }
        }


        private void EnableControls(bool enabled)
        {
            _areControlsEnabled = enabled;
        }


        private void ShowControls(bool visible)
        {
            _areControlsVisible = visible;

            UpdateJoystickVisibility(visible);
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