using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensionMethods;


namespace BallMaze
{
    public class Maze : MonoBehaviour
    {
        private GameObject _maze;
        private LevelLoader _levelLoader;

        private List<GameObject> _walls;
        private List<GameObject> _floorTiles;
        private List<GameObject> _obstacles;


        void Awake()
        {
            _maze = GameObject.Find("Maze");

            _levelLoader = GetComponent<LevelLoader>();
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void BuildMaze(LevelType levelType, string levelId)
        {
            Level level = _levelLoader.DeserializeLevel(levelType, levelId);

            // TODO: load the level
        }


        /// <summary>
        /// Deletes all descendants of the maze object.
        /// </summary>
        public void ClearMaze()
        {
            foreach (GameObject obj in gameObject.transform)
            {
                Destroy(obj);
            }
        }


        /// <summary>
        ///  Update the maze orientation to match the given rotation.
        /// </summary>
        /// <param name="rotation"></param>
        public void UpdateMazeOrientation(Quaternion rotation)
        {
            _maze.transform.rotation = rotation;
        }
    }
}