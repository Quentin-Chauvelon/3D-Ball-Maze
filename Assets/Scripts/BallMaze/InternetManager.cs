using BallMaze.Events;
using BallMaze.UI;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;


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
        private bool _isNoInternetUIVisible;

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

                // Set initialized to true so that other script can know that if isOnline is false,
                // it's because internet is not available and not because the ping is still running
                // do it after the first iteration so that other scripts don't have to wait 25 seconds
                // if the player doesn't have internet, but still keep trying 5 times after that (in the background)
                initialized = true;

                if (result)
                {
                    break;
                }
                else
                {
                    await Task.Delay(5000);
                }
            }

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

            // Stop the loop if isQuitting is true, otherwise after exiting playmode in the unity editor, async methods keep running forever
            while (!GameManager.isQuitting)
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
            // If no network is available, we can't be connected to the internet
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                isOnline = false;
                return false;
            }

            await Ping();

            return isOnline;
        }


        public async Task CheckIsOnlineAndDisplayUI(Action callback = null)
        {
            // If the UI is already being displayed, don't call checkIsOnline again as a loop is already running
            // We simply want to add the callback method to the list of callbacks to call when the player goes back online
            if (_isNoInternetUIVisible)
            {
                // Display the UI and will call the callback method when the player goes back online
                (UIManager.Instance.UIViews[UIViewType.NoInternet] as NoInternetView).DisplayNoInternetUI(false, callback);

                return;
            }

            //If internet is not available, display the UI to let the player know
            if (!await CheckIsOnline())
            {
                _isNoInternetUIVisible = true;

                // Display the UI and will call the callback method when the player goes back online
                (UIManager.Instance.UIViews[UIViewType.NoInternet] as NoInternetView).DisplayNoInternetUI(false, callback);
            }

            // Stop the loop if isQuitting is true, otherwise after exiting playmode in the unity editor, async methods keep running forever
            while (!GameManager.isQuitting)
            {
                await Task.Delay(5000);

                if (await CheckIsOnline())
                {
                    _isNoInternetUIVisible = false;

                    // Hide the UI and will call the callback methods
                    (UIManager.Instance.UIViews[UIViewType.NoInternet] as NoInternetView).DisplayNoInternetUI(true);

                    break;
                }
            }
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