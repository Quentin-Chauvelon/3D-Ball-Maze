using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using UnityEngine.Localization.Settings;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;


namespace BallMaze
{
    public enum CloudSaveKey
    {
        coins,
    }


    public enum CloudSaveExceptionType
    {
        Load,
        LoadRateLimited,
        Save,
        SaveRateLimited,
        Delete,
        DeleteRateLimited,
        Generic
    }


    public struct CloudSaveKeyInfo
    {
        public string key { get; }
        public SaveOptions saveOption { get; }

        public CloudSaveKeyInfo(string key, SaveOptions saveOption)
        {
            this.key = key;
            this.saveOption = saveOption;
        }
    }


    public class CloudDataHandler
    {
        private Dictionary<CloudSaveKey, CloudSaveKeyInfo> _cloudSaveKeys = new Dictionary<CloudSaveKey, CloudSaveKeyInfo>();
        public bool initialized = false;


        /// <summary>
        /// Use an asynchronous constructor because we have to wait for some services to initialize.
        /// Since it's not possible to have an asynchronous constructor, we instead have a static method
        /// that loads the services and then call and return the constructor
        /// </summary>
        /// <returns></returns>
        async public static UniTask<CloudDataHandler> CloudDataHandlerAsync()
        {
            bool error = false;
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                error = true;
                ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "CloudSaveGenericError", ExceptionAction.RestartGame);
                _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
            }

            CloudDataHandler cloudDataHandler = new CloudDataHandler();
            cloudDataHandler.initialized = !error;
            return cloudDataHandler;
        }


        // Private constructor called by the async static method
        private CloudDataHandler()
        {
            _cloudSaveKeys.Add(CloudSaveKey.coins, new CloudSaveKeyInfo("coins", new SaveOptions(new DefaultWriteAccessClassOptions())));
        }



        /// <summary>
        /// Fetch all data from Cloud Save and return a PlayerData object with the data
        /// </summary>
        /// <returns></returns>
        public async UniTask<PlayerData> Load()
        {
            try
            {
                PlayerData playerData = new PlayerData();

                var data = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

                if (data.TryGetValue("coins", out var coins))
                {
                    playerData.coins = coins.Value.GetAs<int>();
                }
                else
                {
                    Debug.Log($"{coins} key not found!");
                }

                return playerData;
            }
            catch (CloudSaveValidationException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Load);
                return new PlayerData();
            }
            catch (CloudSaveRateLimitedException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.LoadRateLimited);
                return new PlayerData();
            }
            catch (CloudSaveException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Load);
                return new PlayerData();
            }
        }


        /// <summary>
        /// Save the given object to the cloud with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async UniTask Save(CloudSaveKey key, object value)
        {
            // If Cloud Save is disabled or if there is an error, don't save
            if (!DataPersistenceManager.isCloudSaveEnabled || ExceptionManager.isError)
            {
                return;
            }

            try
            {
                // Retrieve information about the key such as the key string (name) and its save options (public or default)
                CloudSaveKeyInfo cloudSaveKeyInfo = _cloudSaveKeys[key];

                var data = new Dictionary<string, object> { { cloudSaveKeyInfo.key, value } };

                Debug.Log($"Saving key: {cloudSaveKeyInfo.key} with value: {value}");
                await CloudSaveService.Instance.Data.Player.SaveAsync(data, cloudSaveKeyInfo.saveOption);
            }
            catch (CloudSaveValidationException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Save);
            }
            catch (CloudSaveRateLimitedException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.SaveRateLimited);
            }
            catch (CloudSaveException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Save);
            }
        }


        /// <summary>
        /// Save all given values to the cloud. This is usefull to save all the data in a single batch
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public async UniTask Save(Dictionary<CloudSaveKey, object> values)
        {
            try
            {
                var defaultSaveOptionData = new Dictionary<string, object>();
                var publicSaveOptionData = new Dictionary<string, object>();

                foreach (KeyValuePair<CloudSaveKey, object> entry in values)
                {
                    CloudSaveKeyInfo cloudSaveKeyInfo = _cloudSaveKeys[entry.Key];

                    if (cloudSaveKeyInfo.saveOption == new SaveOptions(new DefaultWriteAccessClassOptions()))
                    {
                        defaultSaveOptionData.Add(cloudSaveKeyInfo.key, entry.Value);
                    }
                    else if (cloudSaveKeyInfo.saveOption == new SaveOptions(new PublicWriteAccessClassOptions()))
                    {
                        publicSaveOptionData.Add(cloudSaveKeyInfo.key, entry.Value);
                    }

                }

                await CloudSaveService.Instance.Data.Player.SaveAsync(defaultSaveOptionData, new SaveOptions(new DefaultWriteAccessClassOptions()));
                await CloudSaveService.Instance.Data.Player.SaveAsync(publicSaveOptionData, new SaveOptions(new PublicWriteAccessClassOptions()));
            }
            catch (CloudSaveValidationException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Save);
            }
            catch (CloudSaveRateLimitedException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.SaveRateLimited);
            }
            catch (CloudSaveException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Save);
            }
        }


        public async UniTask Delete()
        {
            try
            {
                await CloudSaveService.Instance.Data.Player.DeleteAllAsync();
            }
            catch (CloudSaveValidationException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Delete);
            }
            catch (CloudSaveRateLimitedException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.DeleteRateLimited);
            }
            catch (CloudSaveException e)
            {
                DisplayCloudSaveException(e, CloudSaveExceptionType.Delete);
            }
        }


        /// <summary>
        /// Prints the given exception to the console and displays the exception UI .
        /// </summary>
        /// <param name="exception"></param>
        private void DisplayCloudSaveException(Exception exception, CloudSaveExceptionType exceptionType = CloudSaveExceptionType.Generic)
        {
            switch (exceptionType)
            {
                case CloudSaveExceptionType.Load:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveLoadGenericError", ExceptionAction.RestartGame);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                case CloudSaveExceptionType.LoadRateLimited:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveLoadRateLimitedError", ExceptionAction.RestartGame);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                case CloudSaveExceptionType.Save:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveSaveGenericError", ExceptionAction.RestartGame);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                case CloudSaveExceptionType.SaveRateLimited:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveSaveRateLimitedError", ExceptionAction.Resume);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                case CloudSaveExceptionType.Delete:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveDeleteGenericError", ExceptionAction.Resume);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                case CloudSaveExceptionType.DeleteRateLimited:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveDeleteRateLimitedError", ExceptionAction.RestartGame);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
                default:
                    ExceptionManager.ShowExceptionMessage(exception, "ExceptionMessagesTable", "CloudSaveGenericError", ExceptionAction.RestartGame);
                    _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
                    break;
            }
        }
    }
}