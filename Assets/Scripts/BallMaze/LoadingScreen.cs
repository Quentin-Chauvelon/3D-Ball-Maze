using Cysharp.Threading.Tasks;
using UnityEngine;
using System;


namespace BallMaze
{
    public class LoadingScreen : MonoBehaviour
    {
        public static readonly int TIMEOUT = 20;


        private async void Start()
        {
            // TODO: display loading screen UI

            // Wait for Unity Gaming Services to initialize
            if (GameManager.Instance.editorIsWebGL)
            {
                bool result = await InitializeUnityServices.Initialize();
                if (!result)
                {
                    Debug.Log("Couldn't initialize Unity Services, game unable to load");
                    GameUnableToLoad();
                    return;
                }
                Debug.Log("Unity Services have been initialized, game starting");
            }

            // Initialize the game and all its services/components
            GameManager.Instance.Initialize();

            // UniTask.WhenAll will wait for all tasks to complete
            // UniTask.WhenAny will wait for the first task to complete (either all the initialization tasks or the timeout task)
            int position = await UniTask.WhenAny(
                UniTask.WhenAll(
                    // If Cloud Save is enabled, wait for the CloudDataHandler to be initialized
                    DataPersistenceManager.isCloudSaveEnabled
                        ? UniTask.WaitUntil(() => DataPersistenceManager.cloudDataHandlerInitialized)
                        : UniTask.CompletedTask
                ),
                UniTask.Delay(TimeSpan.FromSeconds(TIMEOUT))
            );

            // If the position of the task that completed first is 1, it means it's the timeout task
            if (position == 1)
            {
                Debug.Log("Couldn't initialize services, game unable to load");
                GameUnableToLoad();
            }
            else
            {
                Debug.Log("Services have been initialized, game launching");
                GameManager.Instance.StartGame();
            }
        }


        private void GameUnableToLoad()
        {
            // TODO: show error with a button to restart the game, have a small button to show error message? (and add a message as a parameter to this method)

            _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
            return;
        }
    }
}