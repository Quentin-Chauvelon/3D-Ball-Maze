using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using UnityExtensionMethods;
using System.IO;


namespace BallMaze {
    public class FileManager : MonoBehaviour
    {
        // Constants
        public const string URL = "localhost:8080/";
        public const string URL_API = URL + "api.php/";

        public static bool _isDownloading;
        public static float progress;
        private static bool _isDownloadProgressUIVisible;


        /// <summary>
        /// Get the last modified date of the given url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>A tuple containing a boolean indicating if the request was a success or not
        /// and a DateTime being the result of the request (the date the file at the given url was last modified </returns>
        public static async Task<Tuple<bool, DateTime>> GetLastUrlModifiedDate(string url)
        {
            // Make a get request to the given url.
            // With UniTask, a try catch is needed, because if the user is offline, the request will throw an exception
            UnityWebRequest request;
            try
            {
                request = UnityWebRequest.Get(url);
                await request.SendWebRequest();
            }
            catch (Exception)
            {
                Debug.Log("Error getting last modified date of url: " + url);
                return new Tuple<bool, DateTime>(false, DateTime.UnixEpoch);
            }

            if (request.responseCode < 299 && string.IsNullOrEmpty(request.error))
            {
                try
                {
                    return new Tuple<bool, DateTime>(true, int.Parse(request.downloadHandler.text).UnixTimestampToDate());
                }
                catch
                {
                    return new Tuple<bool, DateTime>(false, DateTime.UnixEpoch);
                }
            }
            else
            {
                Debug.Log("Error getting last modified date of url: " + url + " with code " + request.responseCode + "\n" + request.error);
                return new Tuple<bool, DateTime>(false, DateTime.UnixEpoch);
            }
        }


        /// <summary>
        /// Download the file at the given url to the given path.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="displayDownloadProgressUI">True if the UI showing the download progress should be shown, false otherwise. Defaults to false</param>
        /// <returns>A boolean indicating if the download was a success or not</returns>
        public static async Task<bool> DownloadFile(string url, string path, bool displayDownloadProgressUI = false)
        {
            _isDownloading = true;

            // Make a get request to the given url
            UnityWebRequest request = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                progress = request.downloadProgress * 100;
                await Task.Yield();
            }

            DisplayDownloadProgressUI(displayDownloadProgressUI);

            if (request.responseCode < 299 && string.IsNullOrEmpty(request.error))
            {
                _isDownloading = false;

                File.WriteAllBytes(path, request.downloadHandler.data);
                return true;
            }
            else
            {
                _isDownloading = false;

                Debug.Log("Error downloading file from url: " + url + " to path" + path + " with code " + request.responseCode + "\n" + request.error);
                return false;
            }
        }


        public static void DisplayDownloadProgressUI(bool display)
        {
            _isDownloadProgressUIVisible = display;
        }
    }
}