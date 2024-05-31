using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;


namespace BallMaze
{
    public class FileDataHandler
    {
        private string dataDirPath = "";
        private string dataFileName = "";
        private bool useEncryption = false;
        private readonly string encryptionCodeWord = "SecretEncryptionCodeWord";
        private readonly string backupExtension = ".bak";


        public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
        {
            this.dataDirPath = dataDirPath;
            this.dataFileName = dataFileName;
            this.useEncryption = useEncryption;
        }


        public PlayerData Load(bool allowRestoreFromBackup = true)
        {
            // use Path.Combine to account for different OS's having different path separators
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            PlayerData loadedData = null;

            if (File.Exists(fullPath))
            {
                try
                {
                    // load the serialized data from the file
                    string dataToLoad = "";
                    using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            dataToLoad = reader.ReadToEnd();
                        }
                    }

                    // optionally decrypt the data
                    if (useEncryption)
                    {
                        dataToLoad = EncryptDecrypt(dataToLoad);
                    }

                    // deserialize the data from Json back into the C# object
                    loadedData = JsonConvert.DeserializeObject<PlayerData>(dataToLoad);
                }
                catch (Exception e)
                {
                    // since we're calling Load(..) recursively, we need to account for the case where
                    // the rollback succeeds, but data is still failing to load for some other reason,
                    // which without this check may cause an infinite recursion loop.
                    if (allowRestoreFromBackup)
                    {
                        Debug.LogWarning("Failed to load data file. Attempting to roll back.\n" + e);
                        bool rollbackSuccess = AttemptRollback(fullPath);
                        if (rollbackSuccess)
                        {
                            // try to load again recursively
                            loadedData = Load(false);
                        }
                    }
                    // if we hit this else block, one possibility is that the backup file is also corrupt
                    else
                    {
                        Debug.LogError("Error occured when trying to load file at path: "
                            + fullPath + " and backup did not work.\n" + e);
                    }
                }
            }
            return loadedData;
        }


        public void Save(PlayerData data)
        {
            // use Path.Combine to account for different OS's having different path separators
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            string backupFilePath = fullPath + backupExtension;

            try
            {
                // create the directory the file will be written to if it doesn't already exist
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                // serialize the C# game data object into Json
                string dataToStore = JsonConvert.SerializeObject(data, Formatting.Indented);

                // optionally encrypt the data
                if (useEncryption)
                {
                    dataToStore = EncryptDecrypt(dataToStore);
                }

                // write the serialized data to the file
                using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(dataToStore);
                    }
                }

                // verify the newly saved file can be loaded successfully
                PlayerData verifiedPlayerData = Load();
                // if the data can be verified, back it up
                if (verifiedPlayerData != null)
                {
                    File.Copy(fullPath, backupFilePath, true);
                }
                // otherwise, something went wrong and we should throw an exception
                else
                {
                    throw new Exception("Save file could not be verified and backup could not be created.");
                }

            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
            }
        }


        public void Delete()
        {
            string fullPath = Path.Combine(dataDirPath, dataFileName);

            try
            {
                // ensure the data file exists at this path before deleting the directory
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    File.Delete(fullPath + backupExtension);
                }
                else
                {
                    Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete profile data at path: " + fullPath + "\n" + e);
            }
        }


        // the below is a simple implementation of XOR encryption
        private string EncryptDecrypt(string data)
        {
            string modifiedData = "";
            for (int i = 0; i < data.Length; i++)
            {
                modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
            }
            return modifiedData;
        }


        private bool AttemptRollback(string fullPath)
        {
            bool success = false;
            string backupFilePath = fullPath + backupExtension;

            try
            {
                // if the file exists, attempt to roll back to it by overwriting the original file
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, fullPath, true);
                    success = true;
                    Debug.LogWarning("Had to roll back to backup file at: " + backupFilePath);
                }
                // otherwise, we don't yet have a backup file - so there's nothing to roll back to
                else
                {
                    throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to roll back to backup file at: "
                    + backupFilePath + "\n" + e);
            }

            return success;
        }
    }
}