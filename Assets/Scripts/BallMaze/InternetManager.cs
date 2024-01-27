using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Redcode.Awaiting;


namespace BallMaze
{
    public class InternetManager : MonoBehaviour
    {
        // Singleton pattern
        private static InternetManager _instance;
        public static InternetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("InternetManager is null!");
                }
                return _instance;
            }
        }

        private bool _wasOnline;
        private bool _isOnline;
        public bool isOnline
        {
            get { return _isOnline; }
            set
            {
                _wasOnline = _isOnline;
                _isOnline = value;

                // If the internet was not available and now it is, start the loop to check every few seconds if it's still available
                if (_isOnline && !_isCheckOnlineLoopRunning)
                {
                    CheckIsOnlineLoop();
                }
            }
        }

        // Variable to know if the first check has completed
        public bool initialized;

        private bool _isCheckOnlineLoopRunning;

        private const string PING_ADDRESS = "www.google.com";
        // If internet is available, check every 30 seconds if it's still available
        private const int CHECK_IS_ONLINE_DELAY = 30;
        // Maximum number of attempts to check if the internet is available (only on Initialize());
        private const int MAX_ATTEMPTS = 5;


        /// <summary>
        /// On start, check if the internet is available.
        /// If it's not, try again every 5 seconds until it is for up to 5 times.
        /// </summary>
        private async void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);

            _wasOnline = false;
            isOnline = false;
            initialized = false;

            // Retry up to 5 times to check if the internet is available
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                bool result = await CheckIsOnline();
                if (result)
                {
                    break;
                }
                else
                {
                    await Task.Delay(5000);
                }
            }

            // Set initialized to true so that other script can know that if isOnline is false,
            // it's because internet is not available and not because the ping is still running
            initialized = true;
            InternetManagerEvents.intialized?.Invoke();
        }


        /// <summary>
        /// Check every few seconds if the internet is still available.
        /// This allows to detect if the player loses the internet connection while playing.
        /// </summary>
        public async void CheckIsOnlineLoop()
        {
            if (_isCheckOnlineLoopRunning)
            {
                return;
            }

            _isCheckOnlineLoopRunning = true;

            while (true)
            {
                await Task.Delay(CHECK_IS_ONLINE_DELAY * 1000);

                bool result = await CheckIsOnline();
                if (!result)
                {
                    break;
                }
            }

            _isCheckOnlineLoopRunning = false;
        }


        /// <summary>
        /// Check if the internet is available by pinging the given address.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckIsOnline()
        {
            // If the no network is available, we can't be connected to the internet
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                isOnline = false;
                return false;
            }

            await Ping();

            return isOnline;
        }





        /// <summary>
        /// Ping the given address to check if the internet is available.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Ping()
        {
            UnityWebRequest request = new UnityWebRequest(PING_ADDRESS);
            request.timeout = 3;
            await request.SendWebRequest();

            if (request.responseCode < 299 && string.IsNullOrEmpty(request.error))
            {
                isOnline = true;
                return true;
            }
            else
            {
                isOnline = false;
                return false;
            }
        }
    }
}