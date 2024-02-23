using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;


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

        private PlayerData playerData;
        private List<IDataPersistence> dataPersistenceObjects;
        private FileDataHandler fileDataHandler;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (disableDataPersistence)
            {
                Debug.LogWarning("Data Persistence is currently disabled!");
            }

            fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        }


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }


        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            dataPersistenceObjects = FindAllDataPersistenceObjects();

            LoadGame();

            AutoSave();
        }


        public void DeleteData()
        {
            fileDataHandler.Delete();

            // reload the game so that our data matches the newly selected profile id
            LoadGame();
        }


        public void NewGame()
        {
            playerData = new PlayerData();
        }


        public void LoadGame()
        {
            // return right away if data persistence is disabled
            if (disableDataPersistence)
            {
                return;
            }

            // load any saved data from a file using the data handler
            playerData = fileDataHandler.Load();

            // start a new game if the data is null and we're configured to initialize data for debugging purposes
            if (playerData == null && initializeDataIfNull)
            {
                NewGame();
            }

            // if no data can be loaded, don't continue
            if (playerData == null)
            {
                Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
                return;
            }

            // push the loaded data to all other scripts that need it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(playerData);
            }
        }

        public void SaveGame()
        {
            // return right away if data persistence is disabled
            if (disableDataPersistence)
            {
                return;
            }

            // if we don't have any data to save, log a warning here
            if (playerData == null)
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            // pass the data to other scripts so they can update it
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(playerData);
            }

            // timestamp the data so we know when it was last saved
            playerData.lastUpdated = DateTime.Now.ToBinary();

            // save that data to a file using the data handler
            fileDataHandler.Save(playerData);
        }


        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            // FindObjectsofType takes in an optional boolean to include inactive gameobjects
            IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }


        public bool HasPlayerData()
        {
            return playerData != null;
        }



        private async void AutoSave()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(autoSaveTimeSeconds));
                SaveGame();
                Debug.Log("Auto Saved Game");
            }
        }
    }
}