using BallMaze.Obstacles;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityExtensionMethods;


namespace BallMaze
{
    public class Maze : MonoBehaviour
    {
        private GameObject _maze;
        private LevelLoader _levelLoader;

        public GameObject start;

        public Dictionary<GameObject, Obstacle> obstacles { get; private set; } = new Dictionary<GameObject, Obstacle>();


        void Awake()
        {
            _maze = GameObject.Find("Maze");

            _levelLoader = GetComponent<LevelLoader>();
        }


        /// <summary>
        /// Builds the maze matching the given level.
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelId"></param>
        /// <returns>A boolean indicating if the maze could be loaded</returns>
        public bool BuildMaze(LevelType levelType, string levelId)
        {
            Level level = _levelLoader.DeserializeLevel(levelType, levelId);
            if (level == null)
            {
                return false;
            }

            // Create all container objects
            GameObject flagTargetsContainer = new GameObject("FlagTargets");
            GameObject floorsContainer = new GameObject("Floors");
            GameObject wallsContainer = new GameObject("Walls");
            GameObject cornersContainer = new GameObject("Corners");
            GameObject absolutelyPositionnableObstaclesContainer = new GameObject("AbsolutelyPositionnableObstacles");
            GameObject relativelyPositionnableObstaclesContainer = new GameObject("RelativelyPositionnableObstacles");
            flagTargetsContainer.transform.SetParent(_maze.transform);
            floorsContainer.transform.SetParent(_maze.transform);
            wallsContainer.transform.SetParent(_maze.transform);
            cornersContainer.transform.SetParent(_maze.transform);
            absolutelyPositionnableObstaclesContainer.transform.SetParent(_maze.transform);
            relativelyPositionnableObstaclesContainer.transform.SetParent(_maze.transform);

            // Create the start object
            start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(_maze.transform);

            foreach (Floor floor in level.floors)
            {
                GameObject floorGameObject = floor.Render(obstacles);
                floorGameObject.transform.SetParent(floorsContainer.transform);
                obstacles[floorGameObject] = floor;
            }


            // Walls
            foreach (Wall wall in level.walls)
            {
                GameObject wallGameObject = wall.Render(obstacles);
                wallGameObject.transform.SetParent(wallsContainer.transform);
                obstacles[wallGameObject] = wall;
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                GameObject cornerGameObject = corner.Render(obstacles);
                cornerGameObject.transform.SetParent(cornersContainer.transform);
                obstacles[cornerGameObject] = corner;
            }

            // Target
            GameObject targetGameObject = level.target.Render(obstacles);
            targetGameObject.transform.SetParent(flagTargetsContainer.transform);
            obstacles[targetGameObject] = level.target;

            return true;
        }


        /// <summary>
        /// Returns a boolen indicating if a maze is already loaded.
        /// </summary>
        public bool IsMazeLoaded()
        {
            return _maze.transform.childCount > 0;
        }


        /// <summary>
        /// Returns the bounds of the maze (total size of all its descendants).
        /// </summary>
        /// <returns></returns>
        public Bounds GetMazeBounds()
        {
            Renderer[] renderers = _maze.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderers[0].bounds;
            
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }


        /// <summary>
        /// Deletes all descendants of the maze object.
        /// </summary>
        public void ClearMaze()
        {
            // Loop through all the maze's children and delete them
            foreach (Transform transform in gameObject.transform)
            {
                Destroy(transform.gameObject);
            }

            ResetMazeOrientation();
        }


        /// <summary>
        ///  Update the maze orientation to match the given rotation.
        /// </summary>
        /// <param name="rotation"></param>
        public void UpdateMazeOrientation(Quaternion rotation)
        {
            _maze.transform.rotation = rotation;
        }


        /// <summary>
        /// Resets the maze orientation to the default one.
        /// </summary>
        public void ResetMazeOrientation()
        {
            UpdateMazeOrientation(Quaternion.identity);
        }
    }
}