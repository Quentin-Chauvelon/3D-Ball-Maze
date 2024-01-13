using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    public class Ball : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        // Change this value to play with the ball's speed
        private const short FORCE_SPEED_DIVIDER = 100;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }


        /// <summary>
        /// Freeze or unfreeze the ball.
        /// </summary>
        public void FreezeBall(bool freeze)
        {
            _rigidbody.isKinematic = freeze;
        }


        /// <summary>
        /// Set the ball's visibility.
        /// </summary>
        /// <param name="visible"></param>
        public void SetBallVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }


        /// <summary>
        /// Moves the ball to the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="resetRotation">Boolean indicating if the ball should keep its rotation</param>
        public void MoveBallToPosition(Vector3 position, bool resetRotation = true)
        {
            gameObject.transform.localPosition = position;

            if (resetRotation)
            {
                gameObject.transform.rotation = Quaternion.identity;
            }
        }


        /// <summary>
        /// Adds a force to the ball to make it move faster through the maze. Makes it more enjoyable to play.
        /// Also helps changing direction quicker.
        /// </summary>
        /// <param name="direction">The direction to move the ball towards. Usually the same direction as the joystick or accelerometer</param>
        public void AddForce(Vector2 direction)
        {
            _rigidbody.AddForce(new Vector3(direction.x, 0f, direction.y) / FORCE_SPEED_DIVIDER, ForceMode.Impulse);
        }
    }
}