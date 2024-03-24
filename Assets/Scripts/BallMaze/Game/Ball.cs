using BallMaze.Obstacles;
using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    public class Ball : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        private GameObject _lastHitObstacle;

        // Change this value to play with the ball's speed
        private const short FORCE_SPEED_DIVIDER = 100;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (LevelManager.Instance.LevelState == LevelState.Playing)
            {
                if (_lastHitObstacle != null)
                {
                    // Check if the ball clipped through an obstacle and move bac on top if it did
                    MoveBallOnObstacle();
                }
            }
        }


        private void OnCollisionEnter(Collision other)
        {
            // Get the root of the obstacle
            GameObject otherObstacleGameObject = LevelManager.Instance.GetObstacleGameObjectFromBallCollision(other.gameObject);

            if (LevelManager.Instance.Maze.obstacles[otherObstacleGameObject] != null && LevelManager.Instance.Maze.obstacles[otherObstacleGameObject].canRollOn)
            {
                _lastHitObstacle = other.gameObject;

                Renderer[] renderers = _lastHitObstacle.GetComponentsInChildren<Renderer>();
                Bounds bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; ++i)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            LevelManager.Instance.HandleBallCollision(other);
        }


        private void OnTriggerEnter(Collider other)
        {
            LevelManager.Instance.HandleBallTrigger(other);
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
        /// Checks if the ball clipped through the obstacle it was last on.
        /// If it did, reposition the ball on top of the obstacle.
        /// </summary>
        private void MoveBallOnObstacle()
        {
            // If there is something under the ball, don't move it
            // This is needed otherwise the ball will move back on top of tall obstacles such as tunnels
            if (Physics.Raycast(transform.position, Vector3.down, 5f))
            {
                return;
            }

            // Get the obstacle exact size
            Renderer[] renderers = _lastHitObstacle.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // Check if the ball is within the x and z bounds of the obstacle and if it is under the obstacle but not too low (we don't want to move back from a normal fall)
            if (!(transform.position.x < bounds.center.x - bounds.size.x / 2 || transform.position.x > bounds.center.x + bounds.size.x / 2 ||
                transform.position.z < bounds.center.z - bounds.size.z / 2 || transform.position.z > bounds.center.z + bounds.size.z / 2 ||
                transform.position.y < bounds.center.y - 1.5f || transform.position.y > bounds.center.y))
            {
                bool ballMoved = false;

                // Raycast above to get the y position of the obstacle to move the ball on top of it
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.up, out hit, 1.5f))
                {
                    if (hit.transform.gameObject == _lastHitObstacle)
                    {
                        transform.position = new Vector3(transform.position.x, hit.point.y + transform.localScale.y / 2 + 0.05f, transform.position.z);
                        ballMoved = true;
                    }
                }

                // If the raycast didn't hit anything, check if the floor contains a mesh whose name contains "Floor".
                // This is done to avoid moving the ball too high (eg: for tunnels, we don't want the ball to be on top of the tunnel but on the floor)
                if (!ballMoved)
                {
                    foreach (Transform child in _lastHitObstacle.transform)
                    {
                        if (child.name.Contains("Floor"))
                        {
                            transform.position = new Vector3(transform.position.x, child.position.y + transform.localScale.y / 2 + 0.05f, transform.position.z);
                            ballMoved = true;
                            break;
                        }
                    }
                }

                // If the floor doesn't contain a mesh whose name contains "Floor" (eg: rails...), move the ball on top of the obstacle
                if (!ballMoved)
                {
                    transform.position = new Vector3(transform.position.x, _lastHitObstacle.transform.position.y + transform.localScale.y / 2 + 0.05f, transform.position.z);
                }
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