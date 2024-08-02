using System;
using BallMaze.Obstacles;
using Unity.VisualScripting;
using UnityEngine;


namespace BallMaze
{
    [Serializable]
    public class DefaultLevels
    {
        public float version;
        public int lastModified;

        public int numberOfLevels;
        public Level[] levels;
    }


    [Serializable]
    public class DailyLevels
    {
        public int levelsDate;
        public int lastModified;

        public Level[] levels;
    }


    [Serializable]
    public class UserCreatedLevel
    {
        public int version;
        public int lastModified;
        public int created;

        public string creatorId;
        public string creatorName;

        public Level[] levels;
    }


    [Serializable]
    public class Level
    {
        public string id;
        public string name;
        public string description;
        public DailyLevelDifficulty difficulty;
        public LevelType levelType;
        public int nbObstacles;
        public Vector3 mazeSize;

        public Vector3 startPosition;

        public Floor[] floors;
        public Wall[] walls;
        public Corner[] corners;
        public FlagTarget target;
        public Obstacle[] obstacles;

        public bool setCameraView;
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;

        public float[] times;
    }


    public enum CardinalDirection
    {
        North,
        East,
        South,
        West
    }
}