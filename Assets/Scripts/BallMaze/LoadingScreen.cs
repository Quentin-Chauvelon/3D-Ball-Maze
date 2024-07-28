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
        public static LoadingScreenView LoadingScreenUIView;

        private int _numberOfTasksCompleted = 0;

        private static readonly int TIMEOUT = 20;
        private readonly int NUMBER_OF_TASKS = 1;


        private void Awake()
        {
            LoadingScreenUIView = new LoadingScreenView(GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("loading-screen__background"));
        }


        private async void Start()
        {
            // Initialize and show the loading screen UI
            LoadingScreenUIView.InitializeLoadingScreen(LoadingIndicatorType.ProgressBar);

            // Wait for Unity Gaming Services to initialize
            if (GameManager.Instance.EditorIsWebGL || GameManager.Instance.EditorIsMobile)
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
                    LoadingScreenUIView.Hide();
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
            LoadingScreenUIView.SetProgressBarValue((100 * _numberOfTasksCompleted) / NUMBER_OF_TASKS);
        }


        private void GameUnableToLoad(Exception e)
        {
            ExceptionManager.ShowExceptionMessage(e, "ExceptionMessagesTable", "UnableToStartGame", ExceptionActionType.RestartGame);
            _ = InternetManager.Instance.CheckIsOnlineAndDisplayUI();
            return;
        }
    }
}
