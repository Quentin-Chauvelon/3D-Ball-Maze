using System;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class InitializeUnityServices : MonoBehaviour
{
    private static readonly string ENVIRONMENT = "development";


    public static async UniTask Initialize()
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName(ENVIRONMENT);

        await UnityServices.InitializeAsync(options);
        Debug.Log("Unity Services initialized");
    }
}