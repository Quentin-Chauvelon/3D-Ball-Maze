using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using System.IO.IsolatedStorage;


namespace BallMaze
{
    // The DataPersistence system has been adapted from the Save Load System by Shaped By Rain Studios found
    // here https://www.youtube.com/watch?v=aUi9aijvpgs and
    // here https://github.com/shapedbyrainstudios/save-load-system/blob/5-bug-fixes-and-polish
    public class DataPersistenceManager : MonoBehaviour
    {
        // Singleton pattern
        private static DataPersistenceManager _instance;
        public static DataPersistenceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("DataPersistenceManager is null!");
                }
                return _instance;
            }
        }

        private bool disableDataPersistence = false;
        private bool initializeDataIfNull = true;

        private string fileName = "3dbm_data.json";
        private bool useEncryption = false;

        private int autoSaveTimeSeconds = 15;

        private PlayerData filePlayerData;
        private PlayerData cloudPlayerData;

        private List<IDataPersistence> dataPersistenceObjects;

        private FileDataHandler fileDataHandler;
        public static bool isFileSaveEnabled = false;

        public CloudDataHandler cloudDataHandler;
        // This bool is used to know if the CloudDataHandler has been initialized (Cloud Save and authentification initialized)
        public static bool cloudDataHandlerInitialized = false;
        public static bool isCloudSaveEnabled = false;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Store a bool to know if CloudSave is enabled. This is used so that we don't have to add conditional compilation every time we want to know
#if UNITY_WEBGL
            isCloudSaveEnabled = true;
            isFileSaveEnabled = false;
#elif UNITY_ANDROID || UNITY_IOS
            isCloudSaveEnabled = true;
            isFileSaveEnabled = true;
#endif

            if (disableDataPersistence)
            {
                Debug.LogWarning("Data Persistence is currently disabled!");
            }
        }


        public async void Initialize()
        {
            // If the editor is being tested as WebGL, enable Cloud Save
            if (GameManager.Instance.editorIsWebGL)
            {
                isCloudSaveEnabled = true;
                isFileSaveEnabled = false;
            }

            // If the editor is being tested as mobile, enable File Save
            if (GameManager.Instance.editorIsMobile)
            {
                isCloudSaveEnabled = true;
                isFileSaveEnabled = true;
            }

            fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

            // For the CloudDataHandler, we can't call the constructor directly
            cloudDataHandler = await CloudDataHandler.CloudDataHandlerAsync();
            cloudDataHandlerInitialized = cloudDataHandler.initialized;

            StartGame();
        }


        /// <summary>
        /// Load the data to start the game
        /// </summary>
        private void StartGame()
        {
            dataPersistenceObjects = FindAllDataPersistenceObjects();

            LoadGame();

            Debug.Log($"DataPersistenceManager initialized. File Save: {isFileSaveEnabled}, Cloud Save: {isCloudSaveEnabled}");
            if (isFileSaveEnabled)
            {
                AutoSave();
            }
        }


        public void DeleteData()
        {
            fileDataHandler.Delete();

            // reload the game so that our data matches the newly selected profile id
            LoadGame();
        }


        public async void LoadGame()
        {
            // return right away if data persistence is disabled
            if (disableDataPersistence)
            {
                return;
            }

            if (isFileSaveEnabled)
            {
                filePlayerData = fileDataHandler.Load();
            }

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (filePlayerData == null && initializeDataIfNull)
            {
                filePlayerData = new PlayerData();
            }

            if (isCloudSaveEnabled)
            {
                cloudPlayerData = await cloudDataHandler.Load();
            }

            if (cloudPlayerData == null && initializeDataIfNull)
            {
                cloudPlayerData = new PlayerData();
            }

            // if no data can be loaded, don't continue
            if (filePlayerData == null && cloudPlayerData == null)
            {
                Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
                return;
            }

            // Find the data that has been saved most recently. Add a few seconds to the cloud data to give it
            // a higher priority if they have been saved roughly at the same time
            PlayerData latestPlayerData = DateTime.FromBinary(filePlayerData.lastUpdated) > DateTime.FromBinary(cloudPlayerData.lastUpdated).AddSeconds(5)
                ? filePlayerData
                : cloudPlayerData;

            // push the loaded data to all other scripts that need it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(latestPlayerData);
            }
        }


        public void SaveGame()
        {
            if (cloudDataHandlerInitialized && isCloudSaveEnabled)
            {
                _ = cloudDataHandler.Save(CloudSaveKey.lastUpdated, DateTime.Now.ToBinary());
            }

            // return right away if data persistence is disabled or if file save is disabled
            if (disableDataPersistence || !isFileSaveEnabled)
            {
                return;
            }

            // if we don't have any data to save, log a warning here
            if (filePlayerData == null)
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            // pass the data to other scripts so they can update it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(filePlayerData);
            }

            // timestamp the data so we know when it was last saved
            filePlayerData.lastUpdated = DateTime.Now.ToBinary();

            // save that data to a file using the data handler
            fileDataHandler.Save(filePlayerData);
        }


        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            // FindObjectsofType takes in an optional boolean to include inactive gameobjects
            IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }



        private async void AutoSave()
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(autoSaveTimeSeconds));
                SaveGame();
                Debug.Log("Auto Saved Game");
            }
        }
    }
}