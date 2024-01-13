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
        public List<GameObject> targets;
        public List<GameObject> floorTiles;
        public List<GameObject> walls;
        public List<GameObject> corners;
        public List<GameObject> obstacles;


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
            GameObject targetsContainer = new GameObject("Targets");
            GameObject floorTilesContainer = new GameObject("FloorTiles");
            GameObject wallsContainer = new GameObject("Walls");
            GameObject cornersContainer = new GameObject("Corners");
            GameObject obstaclesContainer = new GameObject("Obstacles");
            targetsContainer.transform.SetParent(_maze.transform);
            floorTilesContainer.transform.SetParent(_maze.transform);
            wallsContainer.transform.SetParent(_maze.transform);
            cornersContainer.transform.SetParent(_maze.transform);
            obstaclesContainer.transform.SetParent(_maze.transform);

            // Create the start object
            start = new GameObject("Start");
            start.transform.position = level.startPosition;
            start.transform.SetParent(_maze.transform);

            // Create all the targets
            foreach (Target target in level.targets)
            {
                GameObject targetObject = (GameObject)Instantiate(Resources.Load("Level/Targets/Target" + target.id.ToString()));
                targetObject.name = "Target";
                targetObject.transform.position = target.p;
                targetObject.transform.rotation = Quaternion.Euler(target.r);
                targetObject.transform.SetParent(targetsContainer.transform);

                targets.Add(targetObject);
            }

            // Create all the floor tiles
            foreach (FloorTile floorTile in level.floorTiles)
            {
                // Use CreatePrimitive for simple objects (such as floor tiles and walls) because after running some tests
                // It appears to be twice as fast as instantiating a prefab (700ms vs 1.5s for 10k objects)
                GameObject floorTileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floorTileObject.name = "Floor";
                floorTileObject.transform.localScale = new Vector3(1, 0.1f, 1);
                floorTileObject.transform.position = floorTile.p;
                floorTileObject.transform.SetParent(floorTilesContainer.transform);

                floorTiles.Add(floorTileObject);
            }

            // Create all the walls
            foreach (Wall wall in level.walls)
            {
                GameObject wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallObject.name = "Wall";

                // Adapt the wall's scale to the direction it's facing (equivalent to rotating the wall)
                if (wall.d == Direction.North || wall.d == Direction.South)
                {
                    wallObject.transform.localScale = new Vector3(1, 0.5f, 0.1f);
                }
                else
                {
                    wallObject.transform.localScale = new Vector3(0.1f, 0.5f, 1);
                }

                // Find the floor tile the wall is on
                foreach (FloorTile floorTile in level.floorTiles)
                {
                    if (wall.id == floorTile.id)
                    {
                        wallObject.transform.position = floorTile.p + new Vector3(0, 0.2f, 0);
                        break;
                    }
                }

                // Move the wall to the correct position based on the direction it's facing
                switch (wall.d)
                {
                    case Direction.North:
                        wallObject.transform.position -= new Vector3(0, 0, 0.45f);
                        break;
                    case Direction.East:
                        wallObject.transform.position -= new Vector3(0.45f, 0, 0);
                        break;
                    case Direction.South:
                        wallObject.transform.position += new Vector3(0, 0, 0.45f);
                        break;
                    case Direction.West:
                        wallObject.transform.position += new Vector3(0.45f, 0, 0);
                        break;
                }

                wallObject.transform.SetParent(wallsContainer.transform);

                walls.Add(wallObject);
            }

            // Create all the corners
            foreach (Corner corner in level.corners)
            {
                GameObject cornerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cornerObject.name = "Corner";
                cornerObject.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

                // Find the floor tile the corner is on
                foreach (FloorTile floorTile in level.floorTiles)
                {
                    if (corner.id == floorTile.id)
                    {
                        cornerObject.transform.position = floorTile.p + new Vector3(0, 0.2f, 0);
                        break;
                    }
                }

                // Move the corner to the correct position based on the direction it's facing
                switch (corner.d)
                {
                    case Direction.NorthEast:
                        cornerObject.transform.position -= new Vector3(0.45f, 0, 0.45f);
                        break;
                    case Direction.SouthEast:
                        cornerObject.transform.position += new Vector3(-0.45f, 0, 0.45f);
                        break;
                    case Direction.SouthWest:
                        cornerObject.transform.position += new Vector3(0.45f, 0, 0.45f);
                        break;
                    case Direction.NorthWest:
                        cornerObject.transform.position += new Vector3(0.45f, 0, -0.45f);
                        break;
                }

                cornerObject.transform.SetParent(cornersContainer.transform);

                corners.Add(cornerObject);
            }

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