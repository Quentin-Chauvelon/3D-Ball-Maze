using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class InitializeUnityServices : MonoBehaviour
{
    private static readonly string ENVIRONMENT = "development";


    public static async UniTask<bool> Initialize()
    {
        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName(ENVIRONMENT);

            await UnityServices.InitializeAsync(options);
            Debug.Log("Unity Services initialized");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to initialize Unity Services: {exception}");
            return false;
        }
    }
}