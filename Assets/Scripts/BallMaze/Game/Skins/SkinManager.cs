using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace BallMaze
{
    public enum SkinRarity
    {
        Common,
        Rare,
        Epic,
        Unique,
        Flags,
        Unknown
    }


    public class Skin
    {
        public int id { get; set; }
        public string name { get; set; }
        public SkinRarity category { get; set; }
        public int price { get; set; }
        public string materialPath { get; set; }
        public string meshPath { get; set; }
        public bool isBuyable { get; set; }
        public string lockText { get; set; }
    }


    public class SkinManager : MonoBehaviour
    {
        // Singleton pattern
        private static SkinManager _instance;
        public static SkinManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("SkinManager is null!");
                }
                return _instance;
            }
        }

        // A list of all the skins and their properties (name, rarity, price, materials...)
        private Skin[] _skinsList;

        private AsyncOperationHandle<TextAsset> _skinsListLoadHandle;

        public static bool IsSkinListLoaded = false;
        public static Exception SkinListLoadingException = null;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _skinsListLoadHandle = Addressables.LoadAssetAsync<TextAsset>("SkinsList");
            _skinsListLoadHandle.Completed += OnSkinsListLoaded;
        }



        /// <summary>
        /// Return a list of all the skins
        /// </summary>
        /// <returns></returns>
        public Skin[] GetSkinsList()
        {
            return _skinsList;
        }


        /// <summary>
        /// Return a list of all the skins from the given category
        /// </summary>
        /// <param name="rarity"></param>
        /// <returns></returns>
        public Skin[] GetSkinsFromCategory(SkinRarity rarity)
        {
            return Array.FindAll(GetSkinsList(), skin => skin.category == rarity);
        }


        public static string GetSkinRarityName(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Common:
                    return "Common";
                case SkinRarity.Rare:
                    return "Rare";
                case SkinRarity.Epic:
                    return "Epic";
                case SkinRarity.Unique:
                    return "Unique";
                case SkinRarity.Flags:
                    return "Flags";
                default:
                    return "???";
            }
        }


        /// <summary>
        /// Called asynchronously when the skins list adressable is loaded
        /// Deserialize the JSON file and save the skins list
        /// </summary>
        /// <param name="operation"></param>
        private void OnSkinsListLoaded(AsyncOperationHandle<TextAsset> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _skinsList = JsonConvert.DeserializeObject<Skin[]>(operation.Result.text);

                IsSkinListLoaded = true;
            }
            else
            {
                // Save the exception that occurred while loading the skins list
                SkinListLoadingException = operation.OperationException;
            }

            _skinsListLoadHandle.Completed -= OnSkinsListLoaded;
        }


        private void OnDestroy()
        {
            Addressables.Release(_skinsListLoadHandle);
        }
    }
}