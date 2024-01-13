using System;
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
        public Difficulty difficulty;
        public LevelType levelType;

        public Vector3 startPosition;
        public Target[] targets;

        public FloorTile[] floorTiles;
        public Wall[] walls;
        public Corner[] corners;

        public float[] times;
    }


    [Serializable]
    public class FloorTile
    {
        public int id;
        public Vector3 p;
    }


    // For now it's better to store the tile the wall is on and the direction rather than the position
    // because when storing the position as a Vector3, floating point precision add lots of digits (e.g. 0.55000000000000004 instead of 0.55)
    // which increases the file size (about 800 bytes more for 30 walls). Although it's not a lot, if we do this for every wall in every level, it starts to add up.
    // Moreover, if we wanted to modify the wall model, we would have to modify the position of every wall, while this way, we only have to change the wall creation code.
    // But this method is slower, because we have to loop through all the walls and floor tiles (O(n^2)).
    // If we want to change this later, an idea to overcome floating point precision would be to stringify the vector (Vector3.ToString("F4"))
    [Serializable]
    public class Wall
    {
        public int id; // Id of the floor tile the wall is on
        public Direction d; // The direction the wall is facing relative to the floor tile (north = z-, east = x-, south = z+, west = x+) 
    }


    [Serializable]
    public class Corner
    {
        public int id; // Id of the floor tile the corner is on
        public Direction d; // The direction the corner is facing relative to the floor tile (north-east = z- x-, south-east = z+ x-, south-west = z+ x+, north-west = z+ x+)
    }


    [Serializable]
    public class Target
    {
        public int id;
        public Vector3 p;
        public Vector3 r;
    }


    public enum Difficulty
    {
        VeryEasy,
        Easy,
        Medium,
        Hard,
        Extreme
    }


    public enum Direction
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }
}