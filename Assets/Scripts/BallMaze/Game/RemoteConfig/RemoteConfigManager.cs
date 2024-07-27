using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using BallMaze;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AI;
using Newtonsoft.Json;
using System.IO;


namespace BallMaze
{
    /// <summary>
    /// This class is used to manage the way the remote config are fetched from the server.
    /// In production, it should be set to Enabled.
    /// </summary>
    public enum RemoteConfigFetchState
    {
        /// <summary>
        /// Disable remote config completely
        /// </summary>
        Disabled,
        /// <summary>
        /// Enable remote config fetching from the server
        /// </summary>
        Enabled,
        /// <summary>
        /// Enable remote config loading from a local file
        /// </summary>
        Stub
    }


    [Serializable]
    public class RemoteConfigData
    {
        public Level[] dailyLevelsEvenDays;
        public Level[] dailyLevelsOddDays;
    }


    public class RemoteConfigManager
    {
        public struct userAttributes { }
        public struct appAttributes { }


        public static void Initialize()
        {
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        }


        public static async void FetchAndApplyRemoteSettings()
        {
            // Wait for the internet manager to be initialized
            if (!InternetManager.Instance.initialized)
            {
                await UniTask.WaitUntil(() => InternetManager.Instance.initialized);
            }

            // If the player is online
            if (InternetManager.Instance.isOnline)
            {
                if (GameManager.DEBUG)
                {
                    Debug.Log("Fetching remote settings");
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
            }
        }


        /// <summary>
        /// Apply the remote settings once they have been successfully fetched from the server
        /// </summary>
        /// <param name="configResponse"></param>
        public static void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            if (DateTime.UtcNow.DayOfYear % 2 == 0)
            {
                DailyLevelsLevelManager.PopulateDailyLevels(RemoteConfigService.Instance.appConfig.config.GetValue("dailyLevelsEvenDays").ToObject<Level[]>());
            }
            else
            {
                DailyLevelsLevelManager.PopulateDailyLevels(RemoteConfigService.Instance.appConfig.config.GetValue("dailyLevelsOddDays").ToObject<Level[]>());
            }
        }


        public static void FetchAndApplyRemoteSettingsStub()
        {
            string jsonData = File.ReadAllText(Path.Combine(Application.persistentDataPath, "remoteConfig.json"));

            ApplyRemoteSettingsStub(JsonConvert.DeserializeObject<RemoteConfigData>(jsonData));
        }


        /// <summary>
        /// Apply the remote settings from a saved file containing the same attributes as the remote settings.
        /// This is used for testing only
        /// </summary>
        /// <param name="data"></param>
        public static void ApplyRemoteSettingsStub(RemoteConfigData data)
        {
            if (DateTime.UtcNow.DayOfYear % 2 == 0)
            {
                DailyLevelsLevelManager.PopulateDailyLevels(data.dailyLevelsEvenDays);
            }
            else
            {
                DailyLevelsLevelManager.PopulateDailyLevels(data.dailyLevelsOddDays);
            }
        }
    }
}