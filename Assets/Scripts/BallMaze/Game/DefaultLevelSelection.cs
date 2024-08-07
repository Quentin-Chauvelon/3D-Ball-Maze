using BallMaze.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensionMethods;
using Cysharp.Threading.Tasks;
using System.IO;
using BallMaze.UI;
using UnityEngine.Localization.Settings;
using Unity.VisualScripting;


namespace BallMaze
{
    public class PopulateLevelSelectionException : Exception
    {
        public PopulateLevelSelectionException() { }
        public PopulateLevelSelectionException(string message) : base(message) { }
        public PopulateLevelSelectionException(string message, Exception inner) : base(message, inner) { }
    }

    public class DefaultLevelSelection
    {
        // Last time the default level files were checked for modification
        private DateTime _lastDefaultLevelFilesModifiedCheck;
        public DateTime LastDefaultLevelFilesModifiedCheck
        {
            get { return _lastDefaultLevelFilesModifiedCheck; }
            set
            {
                _lastDefaultLevelFilesModifiedCheck = value;
                PlayerPrefs.SetInt("LastDefaultLevelFilesModifiedCheck", _lastDefaultLevelFilesModifiedCheck.DateToUnixTimestamp());
            }
        }

        // Last time the default level files were downloaded
        private DateTime _lastDefaultLevelFilesDownload;
        public DateTime LastDefaultLevelFilesDownload
        {
            get { return _lastDefaultLevelFilesDownload; }
            set
            {
                _lastDefaultLevelFilesDownload = value;
                PlayerPrefs.SetInt("LastDefaultLevelFilesDownload", _lastDefaultLevelFilesDownload.DateToUnixTimestamp());
            }
        }

        private bool _isDownloading;
        private bool _forceDownload;


        public void Initialize()
        {
            // Load the last time the default level files were checked for modification if it exists
            if (PlayerPrefs.HasKey("LastDefaultLevelFilesModifiedCheck"))
            {
                _lastDefaultLevelFilesModifiedCheck = PlayerPrefs.GetInt("LastDefaultLevelFilesModifiedCheck").UnixTimestampToDate();
            }
            else
            {
                _lastDefaultLevelFilesModifiedCheck = DateTime.UnixEpoch;
            }

            // Load the last time the default level files were downloaded if it exists
            if (PlayerPrefs.HasKey("LastDefaultLevelFilesDownload"))
            {
                _lastDefaultLevelFilesDownload = PlayerPrefs.GetInt("LastDefaultLevelFilesDownload").UnixTimestampToDate();
            }
            else
            {
                _lastDefaultLevelFilesDownload = DateTime.UnixEpoch;
            }

            LoadDefaultLevelSelection();
        }


        public async void LoadDefaultLevelSelection()
        {
            // Populate the level selection view once at the very beggining so that even we don't have to wait for the ping to finish to load the levels
            // because the ping can take a long time if the player doesn't have internet
            if (File.Exists(Path.Combine(LevelManager.Instance.LEVELS_PATH, DefaultLevelsLevelManager.DEFAULT_LEVELS_SELECTION_FILE_NAME)))
            {
                PopulateLevelSelectionView();
            }

            // If the internet manager has not been initialized yet, wait for it to be initialized, otherwise we don't know if the internet is available
            await new WaitUntil(() => InternetManager.Instance.initialized);

            if (InternetManager.Instance.isOnline)
            {
                (bool success, DateTime lastModifiedDate) = await FileManager.GetLastUrlModifiedDate(FileManager.URL_API + "levels/defaultLevelsSelectionLastModifiedDate");
                if (success)
                {
                    // Update the last time the default level files were checked for modification to now
                    LastDefaultLevelFilesModifiedCheck = DateTime.UtcNow;

                    // If the file has been modified since the last time it was downloaded or forceDownload is true, re-download it
                    if ((lastModifiedDate > _lastDefaultLevelFilesDownload || _forceDownload) && !_isDownloading)
                    {
                        _isDownloading = true;
                        _forceDownload = false;

                        await DownloadDefaultLevelFiles();

                        _isDownloading = false;

                        PopulateLevelSelectionView();
                    }

                    return;
                }
            }
            else
            {
                // If internet is not available, but the download has been forced, display no internet message
                if (_forceDownload)
                {
                    // Display no internet message and call LoadDefaultLevelSelection() when the player goes back online
                    await InternetManager.Instance.CheckIsOnlineAndDisplayUI(LoadDefaultLevelSelection);

                    UIManager.Instance.Show(UIViewType.MainMenu);

                    return;
                }
            }

            // If the file doesn't exist and we have no internet, we have no way of loading the levels,
            // So display no internet message and wait for the player to go back online
            if (!File.Exists(Path.Combine(LevelManager.Instance.LEVELS_PATH, DefaultLevelsLevelManager.DEFAULT_LEVELS_SELECTION_FILE_NAME)))
            {
                // Display no internet message and call LoadDefaultLevelSelection() when the player goes back online
                await InternetManager.Instance.CheckIsOnlineAndDisplayUI(LoadDefaultLevelSelection);
            }
        }


        /// <summary>
        /// Download the default level files.
        /// The defaultLevels.json file containing all information about each level.
        /// The defaultLevelsSelection.json file containing less information about each level for faster download
        /// as is it only used to populate the level selection view.
        /// </summary>
        public async UniTask DownloadDefaultLevelFiles()
        {
            // Download defaultLevelsSelection.json file containing information about the default levels to populate the level selection view (id, name, times)
            // It contains the same information as the defaultLevels.json file but with less data for faster download
            bool success = await FileManager.DownloadFile(FileManager.URL + "levels/" + DefaultLevelsLevelManager.DEFAULT_LEVELS_SELECTION_FILE_NAME, Path.Combine(LevelManager.Instance.LEVELS_PATH, DefaultLevelsLevelManager.DEFAULT_LEVELS_SELECTION_FILE_NAME));
            // Download defaultLevels.json file containing all the information about each default level (id, name, obstacles position, times...)
            success = success && await FileManager.DownloadFile(FileManager.URL + "levels/" + DefaultLevelsLevelManager.DEFAULT_LEVELS_FILE_NAME, Path.Combine(LevelManager.Instance.LEVELS_PATH, DefaultLevelsLevelManager.DEFAULT_LEVELS_FILE_NAME));

            if (success)
            {
                LastDefaultLevelFilesDownload = DateTime.UtcNow;
            }
        }


        /// <summary>
        /// Populate the default level selection view with the given levels selection
        /// </summary>
        public void PopulateLevelSelectionView()
        {
            // Deserialize the default levels selection file
            LevelsSelection levelsSelection = LevelSelectionLoader.DeserializeLevelsSelection();

            if (levelsSelection != null && levelsSelection.levels.Length > 0)
            {
                (UIManager.Instance.UIViews[UIViewType.DefaultLevelSelection] as DefaultLevelSelectionView).PopulateLevelSelectionView(levelsSelection);

                DefaultLevelsLevelManager.LoadDefaultLevelsInformation(levelsSelection);
            }
            else
            {
                try
                {
                    // if the file has been checked in the last 5 minutes, don't check it again
                    if (_lastDefaultLevelFilesModifiedCheck.DateInTimeframe(300))
                    {
                        _forceDownload = true;
                        throw new PopulateLevelSelectionException($"Couldn't populate level selection view.\nlevelsSelection = {levelsSelection},\nlevelsSelection.levels.Length = {levelsSelection.levels.Length},\n_lastDefaultLevelFilesModifiedCheck = {_lastDefaultLevelFilesModifiedCheck}");
                    }
                    else
                    {
                        // Display no internet message and call LoadDefaultLevelSelection() when the player goes back online
                        _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI(LoadDefaultLevelSelection);
                    }
                }
                catch (PopulateLevelSelectionException e)
                {
                    ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "LevelSelectionLoadingCheckInternetGenericError");
                }
            }
        }
    }
}