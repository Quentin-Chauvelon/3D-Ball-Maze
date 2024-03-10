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
            GameObject obstaclesContainer = new GameObject("Obstacles");
            flagTargetsContainer.transform.SetParent(_maze.transform);
            floorsContainer.transform.SetParent(_maze.transform);
            wallsContainer.transform.SetParent(_maze.transform);
            cornersContainer.transform.SetParent(_maze.transform);
            obstaclesContainer.transform.SetParent(_maze.transform);

            // Create the start object
            start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(_maze.transform);

            foreach (Floor floor in level.floors)
            {
                // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after benchmarking it,
                // it appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
                GameObject floorGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floorGameObject.name = "Floor";
                floorGameObject.transform.localScale = new Vector3(1, 0.1f, 1);
                floorGameObject.transform.position = floor.position;
                floorGameObject.transform.SetParent(floorsContainer.transform);

                obstacles[floorGameObject] = floor;
            }

            // Walls
            foreach (Wall wall in level.walls)
            {
                GameObject wallGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallGameObject.name = "Wall";

                // Adapt the wall's scale to the direction it's facing (equivalent to rotating the wall)
                if (wall.direction == CardinalDirection.North || wall.direction == CardinalDirection.South)
                {
                    wallGameObject.transform.localScale = new Vector3(1, 0.5f, 0.1f);
                }
                else
                {
                    wallGameObject.transform.localScale = new Vector3(0.1f, 0.5f, 1);
                }

                Vector3 offset = Vector3.zero;
                // Add an offset to move the wall to the correct position based on the direction it's facing
                switch (wall.direction)
                {
                    case CardinalDirection.North:
                        offset = new Vector3(0, 0, -0.45f);
                        break;
                    case CardinalDirection.East:
                        offset = new Vector3(-0.45f, 0, 0);
                        break;
                    case CardinalDirection.South:
                        offset = new Vector3(0, 0, 0.45f);
                        break;
                    case CardinalDirection.West:
                        offset = new Vector3(0.45f, 0, 0);
                        break;
                    default:
                        Debug.LogWarning($"Invalid wall direction {wall.direction} for wall {wall.id}.");
                        break;
                }

                PositionObstacleOverObstacleFromId(wallGameObject.transform, wall.obstacleId, offset + new Vector3(0, 0.2f, 0));

                wallGameObject.transform.SetParent(wallsContainer.transform);

                obstacles[wallGameObject] = wall;
            }

            // Corners
            foreach (Corner corner in level.corners)
            {
                GameObject cornerGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cornerGameObject.name = "Corner";
                cornerGameObject.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

                Vector3 offset = Vector3.zero;
                // Add an offset to move the wall to the correct position based on the direction it's facing
                switch (corner.direction)
                {
                    case CardinalDirection.North:
                        offset = new Vector3(-0.45f, 0, -0.45f);
                        break;
                    case CardinalDirection.East:
                        offset = new Vector3(-0.45f, 0, 0.45f);
                        break;
                    case CardinalDirection.South:
                        offset = new Vector3(0.45f, 0, 0.45f);
                        break;
                    case CardinalDirection.West:
                        offset = new Vector3(0.45f, 0, -0.45f);
                        break;
                    default:
                        Debug.LogWarning($"Invalid wall direction {corner.direction} for wall {corner.id}.");
                        break;
                }

                PositionObstacleOverObstacleFromId(cornerGameObject.transform, corner.obstacleId, offset + new Vector3(0, 0.2f, 0));

                cornerGameObject.transform.SetParent(cornersContainer.transform);

                obstacles[cornerGameObject] = corner;
            }

            // Target
            GameObject targetGameObject = (GameObject)Instantiate(Resources.Load("Level/Targets/FlagTarget"));
            targetGameObject.name = "Target";

            PositionObstacleOverObstacleFromId(targetGameObject.transform, level.target.obstacleId, new Vector3(0, 0.4f, 0));

            targetGameObject.transform.SetParent(flagTargetsContainer.transform);

            obstacles[targetGameObject] = level.target;

            return true;
        }


        private void PositionObstacleOverObstacleFromId(Transform transform, int obstacleId, Vector3 offset)
        {
            foreach (KeyValuePair<GameObject, Obstacle> obstacle in obstacles)
            {
                if (obstacle.Value.id == obstacleId)
                {
                    transform.position = obstacle.Key.transform.position + offset;
                    return;
                }
            }

            Debug.LogWarning($"Could not find obstacle with id {obstacleId} to position {transform.name} over.");
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