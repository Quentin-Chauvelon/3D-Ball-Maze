using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BallMaze
{
    public class CameraManager : MonoBehaviour
    {
        private Camera _camera;

        void Awake()
        {
            _camera = Camera.main;
        }


        /// <summary>
        /// Updates the camera position and rotation based on the given bounds (representing the total size of the maze).
        /// This is used to dynamically fit the maze in the camera's perspective for every maze and every screen size.
        /// </summary>
        /// <see href="https://forum.unity.com/threads/fit-object-exactly-into-perspective-cameras-field-of-view-focus-the-object.496472/"/>
        /// <param name="bounds"></param>
        public void FitMazeInPerspective(Bounds bounds)
        {
            // We can represent the maze by a sphere and use its radius to determine the distance between the center of the maze and its farthest point
            float radius = (float)Math.Sqrt(Math.Pow(bounds.extents.x, 2) + Math.Pow(bounds.extents.z, 2));

            // We need to place the camera above the maze because the following calculations will move the camera along a line going from the center of the maze to the camera's position
            _camera.transform.position = bounds.center + new Vector3(0, 10, 0);

            // Calculate the minimum distance between the camera and the center of the maze so that the camera can see the whole maze
            float minDistance = radius / Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView / 2);

            // Calculate the best position for the camera
            Vector3 normVectorBoundsCenter2CurrentCamPos = (_camera.transform.position - bounds.center) / Vector3.Magnitude(_camera.transform.position - bounds.center);

            // Move the camera to the calculated position and rotate it by 20 degrees (it looks better than a top-down view)
            _camera.transform.position = new Vector3(bounds.center.x, (minDistance * normVectorBoundsCenter2CurrentCamPos).y - 1f, bounds.center.z);
            _camera.transform.RotateAround(bounds.center, new Vector3(1, 0, 0), 20);

            _camera.transform.LookAt(bounds.center, new Vector3(0, -1, -1));

            // Translate the camera along the z-axis because the 20 degrees rotation makes look like its not centered
            _camera.transform.position = new Vector3(_camera.transform.position.x, _camera.transform.position.y, _camera.transform.position.z + 0.4f);

            _camera.nearClipPlane = minDistance - radius;
        }
    }
}