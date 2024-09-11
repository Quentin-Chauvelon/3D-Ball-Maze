using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace BallMaze
{
    public enum SkinCategory
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
        public SkinCategory category { get; set; }
        public int price { get; set; }
        public string materialPath { get; set; }
        public string meshPath { get; set; }
        public bool isBuyable { get; set; }
        public string lockText { get; set; }
    }


    public class SkinManager : MonoBehaviour
    {
        // A list of all the skins and their properties (name, category, price, materials...)
        private Skin[] _skinsList;

        private AsyncOperationHandle<TextAsset> _skinsListLoadHandle;

        public static bool IsSkinListLoaded = false;
        public static Exception SkinListLoadingException = null;

        private List<int> _unlockedSkins;

        public int EquippedSkin = 0;


        public SkinManager()
        {
            _skinsListLoadHandle = Addressables.LoadAssetAsync<TextAsset>("SkinsList");
            _skinsListLoadHandle.Completed += OnSkinsListLoaded;
        }


        /// <summary>
        /// Load the list of unlocked skins from the player data.
        /// It also sorts the list to be able to use binary search to check if a skin is unlocked
        /// </summary>
        /// <param name="unlockedSkins"></param>
        public void SetUnlockedSkins(List<int> unlockedSkins)
        {
            _unlockedSkins = unlockedSkins;
            _unlockedSkins.Sort();
        }


        public List<int> GetUnlockedSkins()
        {
            return _unlockedSkins;
        }


        /// <summary>
        /// Check if the given id is in the list of unlocked skins using a binary search
        /// since the list is sorted on each new skin unlocked
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsSkinUnlocked(int id)
        {
            return _unlockedSkins.BinarySearch(id) >= 0;
        }


        public void EquipSkin(int id)
        {
            EquippedSkin = id;
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
        /// <param name="category"></param>
        /// <returns></returns>
        public Skin[] GetSkinsFromCategory(SkinCategory category)
        {
            return Array.FindAll(GetSkinsList(), skin => skin.category == category);
        }


        public static string GetSkinCategoryName(SkinCategory category)
        {
            switch (category)
            {
                case SkinCategory.Common:
                    return "Common";
                case SkinCategory.Rare:
                    return "Rare";
                case SkinCategory.Epic:
                    return "Epic";
                case SkinCategory.Unique:
                    return "Unique";
                case SkinCategory.Flags:
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