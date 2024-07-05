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
        /// <param name="amount"></param>
        /// <param name="multiplier"></param>
        /// <param name="uiUpdateDelay">The amount of milliseconds to wait before updating the UI</param>
        public async void UpdateCoins(int amount, int multiplier, int uiUpdateDelay = 0)
        {
            if (amount == 0)
            {
                return;
            }

            // The multiplier should never be 0, so set it to 1 if it is (to avoid errasing player data)
            if (multiplier == 0)
            {
                multiplier = 1;
            }

            if (amount > 0)
            {
                amount = amount * multiplier;
            }

            _coins += amount;

            // If Cloud Save is enabled, save to the cloud
            if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
            {
                await DataPersistenceManager.Instance.cloudDataHandler.Save(CloudSaveKey.coins, _coins);
            }

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
        /// <param name="amount"></param>
        /// <param name="uiUpdateDelay">The amount of milliseconds to wait before updating the UI</param>
        public void UpdateCoins(int amount, int uiUpdateDelay = 0)
        {
            UpdateCoins(amount, _coinsMultiplier, uiUpdateDelay);
        }


        /// <summary>
        /// Update the coins UI
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateCoinsUI(int amount)
        {
            PlayerEvents.CoinsUpdated?.Invoke(_coins, amount);
        }


        /// <summary>
        /// Update the coins UI after waiting for the given amount of milliseconds
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="uiUpdateDelay"></param>
        private async void UpdateCoinsUIAfterDelay(int amount, int uiUpdateDelay)
        {
            await UniTask.Delay(uiUpdateDelay);
            UpdateCoinsUI(amount);
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