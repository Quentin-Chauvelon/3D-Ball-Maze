using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BallMaze.Events;


namespace BallMaze
{
    public class CoinsManager
    {
        private int _coins;
        public int Coins
        {
            get { return _coins; }
        }

        private int _coinsMultiplier;

        public CoinsManager()
        {
            _coins = 0;
        }


        /// <summary>
        /// Check and return if the player has enough coins
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool HasEnoughCoins(int amount)
        {
            return _coins >= amount;
        }


        /// <summary>
        /// Set the amount of coins the player has
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="uiUpdateDelay">The amount of milliseconds to wait before updating the UI</param>
        public void SetCoins(int amount, int uiUpdateDelay = 0)
        {
            _coins = amount;

            if (uiUpdateDelay == 0)
            {
                // Update the UI immediately
                UpdateCoinsUI(amount);
            }
            else
            {
                // Update the UI after a delay
                UpdateCoinsUIAfterDelay(amount, uiUpdateDelay);
            }
        }


        /// <summary>
        /// Update the amount of coins the player has
        /// </summary>
        /// <param name="increment"></param>
        /// <param name="multiplier"></param>
        /// <param name="uiUpdateDelay">The amount of milliseconds to wait before updating the UI</param>
        public async void UpdateCoins(int increment, int multiplier, int uiUpdateDelay = 0)
        {
            if (increment == 0)
            {
                return;
            }

            // The multiplier should never be 0, so set it to 1 if it is (to avoid errasing player data)
            if (multiplier == 0)
            {
                multiplier = 1;
            }

            if (increment > 0)
            {
                increment = increment * multiplier;
            }

            // Make sure the player's balance will still be positive after updating the coins
            if (increment < 0 && _coins + increment < 0)
            {
                return;
            }

            _coins += increment;

            // If Cloud Save is enabled, save to the cloud
            if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
            {
                await DataPersistenceManager.Instance.cloudDataHandler.Save(CloudSaveKey.coins, _coins);
            }

            if (uiUpdateDelay == 0)
            {
                // Update the UI immediately
                UpdateCoinsUI(increment);
            }
            else
            {
                // Update the UI after a delay
                UpdateCoinsUIAfterDelay(increment, uiUpdateDelay);
            }
        }


        /// <summary>
        /// Update the amount of coins the player has
        /// </summary>
        /// <param name="increment"></param>
        /// <param name="uiUpdateDelay">The amount of milliseconds to wait before updating the UI</param>
        public void UpdateCoins(int increment, int uiUpdateDelay = 0)
        {
            UpdateCoins(increment, _coinsMultiplier, uiUpdateDelay);
        }


        /// <summary>
        /// Update the coins UI
        /// </summary>
        /// <param name="increment"></param>
        public void UpdateCoinsUI(int increment)
        {
            PlayerEvents.CoinsUpdated?.Invoke(_coins, increment);
        }


        /// <summary>
        /// Update the coins UI after waiting for the given amount of milliseconds
        /// </summary>
        /// <param name="increment"></param>
        /// <param name="uiUpdateDelay"></param>
        private async void UpdateCoinsUIAfterDelay(int increment, int uiUpdateDelay)
        {
            await UniTask.Delay(uiUpdateDelay);
            UpdateCoinsUI(increment);
        }


        /// <summary>
        /// Set the coins multiplier
        /// </summary>
        /// <param name="multiplier"></param>
        public void SetCoinsMultiplier(short multiplier)
        {
            _coinsMultiplier = multiplier;
        }
    }
}