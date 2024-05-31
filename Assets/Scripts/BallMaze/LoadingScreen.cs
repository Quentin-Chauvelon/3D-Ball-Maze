using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using BallMaze.UI;
using UnityEngine.Localization.Settings;


namespace BallMaze
{
    public class LoadingScreen : MonoBehaviour
    {
        private UIDocument _loadingScreenUIDocument;
        private LoadingScreenView _loadingScreenUIView;

        private int _numberOfTasksCompleted = 0;

        private static readonly int TIMEOUT = 20;
        private readonly int NUMBER_OF_TASKS = 1;


        private async void Start()
        {
            // Initialize and show the loading screen UI
            _loadingScreenUIDocument = GetComponent<UIDocument>();
            _loadingScreenUIView = new LoadingScreenView(_loadingScreenUIDocument.rootVisualElement.Q<VisualElement>("loading-screen__background"));
            _loadingScreenUIView.SetProgressBarValue(0);
            _loadingScreenUIView.Show();

            // Wait for Unity Gaming Services to initialize
            if (GameManager.Instance.editorIsWebGL || GameManager.Instance.editorIsMobile)
            {
                try
                {
                    await InitializeUnityServices.Initialize();
                }
                catch (Exception e)
                {
                    Debug.Log("Failed to initialize Unity Services, game unable to load");
                    GameUnableToLoad(e);
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
                    (DataPersistenceManager.isCloudSaveEnabled
                        ? UniTask.WaitUntil(() => DataPersistenceManager.cloudDataHandlerInitialized)
                        : UniTask.CompletedTask
                    ).ContinueWith(UpdateProgress)
                ),
                UniTask.Delay(TimeSpan.FromSeconds(TIMEOUT))
            );

            // If the position of the task that completed first is 1, it means it's the timeout task
            try
            {
                if (position == 1)
                {
                    Debug.Log("Couldn't initialize services, game unable to load");
                    throw new Exception("Initialization timed out");
                }
                else
                {
                    Debug.Log("Services have been initialized, game launching");
                    _loadingScreenUIView.Hide();
                    GameManager.Instance.StartGame();
                }
            }
            catch (Exception e)
            {
                GameUnableToLoad(e);
            }
        }


        private void UpdateProgress()
        {
            _numberOfTasksCompleted += 1;
            _loadingScreenUIView.SetProgressBarValue((100 * _numberOfTasksCompleted) / NUMBER_OF_TASKS);
        }


        private void GameUnableToLoad(Exception e)
        {
            ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "UnableToStartGame", ExceptionActionType.RestartGame);
            _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
            return;
        }
    }
}
