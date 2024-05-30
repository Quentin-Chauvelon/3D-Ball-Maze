using System.IO;
using UnityEngine;

namespace BallMaze
{
    public class DefaultLevelsLevelManager : LevelManagerBase
    {
        public override LevelType levelType => LevelType.Default;

        public override string LEVELS_PATH => Path.Combine(Application.persistentDataPath, "levels");

        protected override bool _isSecondChanceEnabled => true;

        public const string DEFAULT_LEVELS_FILE_NAME = "defaultLevels.json";
        public const string DEFAULT_LEVELS_SELECTION_FILE_NAME = "defaultLevelsSelection.json";


        public DefaultLevelsLevelManager()
        {
            ResetLevelManager();

            // If the levels folder doesn't exist, create it
            if (!Directory.Exists(LEVELS_PATH))
            {
                Directory.CreateDirectory(LEVELS_PATH);
            }
        }
    }
}