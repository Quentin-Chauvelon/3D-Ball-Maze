using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.UI
{
    [Serializable]
    public class LevelSelection
    {
        public string id;
        public string name;

        public float[] times;
    }


    [Serializable]
    public class LevelsSelection
    {
        public int numberOfLevels;
        public LevelSelection[] levels;
    }
}