using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using BallMaze;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AI;


namespace BallMaze
{
    public class RemoteConfigManager
    {
        public struct userAttributes { }
        public struct appAttributes { }

        public const bool REMOTE_CONFIG_ENABLED = true;


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

        public static void ApplyRemoteSettings(ConfigResponse configResponse)
        {
            if (DateTime.UtcNow.DayOfYear % 2 == 0)
            {
                RemoteConfigService.Instance.appConfig.config.GetValue("dailyLevelsEvenDays");
            }
            else
            {
                RemoteConfigService.Instance.appConfig.config.GetValue("dailyLevelsOddDays");
            }
        }
    }
}