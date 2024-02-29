using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;


namespace BallMaze.UI
{
    public static class LevelSelectionLoader
    {
        /// <summary>
        /// Deserialize the levels selection and returns it as a LevelsSelection object.
        /// </summary>
        /// <returns></returns>
        public static LevelsSelection DeserializeLevelsSelection()
        {
            try
            {
                // Get the levels selection's json data as a string
                string jsonData = GetLevelsSelectionJsonAsString();

                // Deserialize the json data
                LevelsSelection levelsSelection = JsonUtility.FromJson<LevelsSelection>(jsonData);
                
                if (levelsSelection == null || levelsSelection.levels.Length == 0)
                {
                    throw new CouldNotLoadLevelException("LevelsSelection array is empty");
                }

                return levelsSelection;
            }
            catch (CouldNotLoadLevelException e)
            {
                DisplayLevelSelectionLoadingException(e);
            }

            return null;
        }


        /// <summary>
        /// Get the levels selection's json data as a string.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CouldNotLoadLevelException"></exception>
        private static string GetLevelsSelectionJsonAsString()
        {
            string path = Path.Combine(LevelManager.LEVELS_PATH, LevelManager.DEFAULT_LEVELS_SELECTION_FILE_NAME);

            // Read the json data from the file
            string jsonData = ReadLevelFile(path);

            if (jsonData == "")
            {
                throw new CouldNotLoadLevelException("Couldn't read json data");
            }

            return jsonData;
        }


        /// <summary>
        /// Reads the content of the file at the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="CouldNotLoadLevelException"></exception>
        private static string ReadLevelFile(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)) || !File.Exists(path))
            {
                throw new CouldNotLoadLevelException("Directoy or file doesn't exist at path " + path);
            }

            string jsonData = "";
            try
            {
                jsonData = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                DisplayLevelSelectionLoadingException(e);
            }

            return jsonData;
        }


        /// <summary>
        /// Prints the given exception to the console and displays the exception UI with the default level loading error message.
        /// </summary>
        /// <param name="exception"></param>
        private static void DisplayLevelSelectionLoadingException(Exception exception)
        {
            Debug.LogException(exception);
            ExceptionManager.ShowExceptionMessage(LocalizationSettings.StringDatabase.GetLocalizedString("ExceptionMessagesTable", "LevelSelectionLoadingCheckInternetGenericError"), ExceptionAction.BackToLevels);
        }
    }
}