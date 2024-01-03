using System;
using System.IO;
using UnityEngine;


namespace BallMaze
{
    public class CouldNotLoadLevelException : Exception
    {
        public CouldNotLoadLevelException() : base() { }
        public CouldNotLoadLevelException(string message) : base(message) { }
        public CouldNotLoadLevelException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class LevelLoader : MonoBehaviour
    {
        public Level DeserializeLevel(LevelType levelType, string levelId)
        {
            try
            {
                // Get the level's json data as a string
                string jsonData = GetLevelJsonAsString(levelType, levelId);

                // Deserialize the json data
                Level deserializedLevel = JsonUtility.FromJson<Level>(jsonData);
            }
            catch (CouldNotLoadLevelException e)
            {
                DisplayDefaultLevelLoadingException(e);
            }

            return null;
        }


        /// <summary>
        /// Get the level's json data as a string.
        /// </summary>
        /// <param name="levelType"></param>
        /// <param name="levelId"></param>
        /// <returns></returns>
        private string GetLevelJsonAsString(LevelType levelType, string levelId)
        {
            // Get the path to the file based on the level type
            string path;
            switch(levelType)
            {
                case LevelType.Default:
                    path = Path.Combine(LevelManager.LEVELS_PATH, "defaultLevels.json");
                    break;

                case LevelType.DailyLevel:
                    path = Path.Combine(LevelManager.LEVELS_PATH, "dailyLevels.json");
                    break;

                case LevelType.UserCreated:
                    path = Path.Combine(LevelManager.LEVELS_PATH, levelId + ".json");
                    break;

                default:
                    throw new CouldNotLoadLevelException();
            }

            // Read the json data from the file
            string jsonData = ReadLevelFile(path);

            // Get the line containing the level's data
            jsonData = GetLineContainingLevelId(jsonData, levelId);

            if (jsonData == "")
            {
                throw new CouldNotLoadLevelException();
            }

            return jsonData;
        }


        /// <summary>
        /// Get the entire line containing the level's data based on the level id.
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="levelId"></param>
        /// <returns></returns>
        private string GetLineContainingLevelId(string jsonData, string levelId)
        {
            foreach (string line in jsonData.Split('\n'))
            {
                if (line.Contains("\"id\":\"" + levelId + "\""))
                {
                    return line.Trim(new char[] { ' ', ',', '\r', '\n' });
                }
            }

            return "";
        }


        private string ReadLevelFile(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)) || !File.Exists(path))
            {
                throw new CouldNotLoadLevelException();
            }

            string jsonData = "";
            try
            {
                jsonData = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                DisplayDefaultLevelLoadingException(e);
            }

            return jsonData;
        }


        /// <summary>
        /// Prints the given exception to the console and displays the exception UI with the default level loading error message.
        /// </summary>
        private void DisplayDefaultLevelLoadingException()
        {
            ExceptionManager.Instance.ShowExceptionMessage("Sorry, there seems to have been a problem loading the level. Try checking your internet connection...", ExceptionManager.ExceptionAction.BackToLevels);
        }


        /// <summary>
        /// Prints the given exception to the console and displays the exception UI with the default level loading error message.
        /// </summary>
        /// <param name="exception"></param>
        private void DisplayDefaultLevelLoadingException(Exception exception)
        {
            Debug.LogException(exception, this);
            DisplayDefaultLevelLoadingException();
        }
    }
}