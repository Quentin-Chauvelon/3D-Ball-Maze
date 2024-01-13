using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    [CreateAssetMenu(fileName = "Config", menuName = "3D Ball Maze/Config", order = 0)]
    public class Config : ScriptableObject
    {
        public bool setLevelToLoad;
        public string levelToLoad;
    }
}