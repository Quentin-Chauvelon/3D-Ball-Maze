using System;
using System.Collections.Generic;
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
        public string imagePath { get; set; }
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

        // Cache the materials for the skins, so that we don't have to load them each time we preview a skin
        private static Dictionary<int, Material> _skinMaterials = new Dictionary<int, Material>();
        // Handle to load material for the skin preview
        private static AsyncOperationHandle<Material> _skinMaterialHandle;

        public static bool IsSkinListLoaded = false;
        public static Exception SkinListLoadingException = null;

        // To check more efficently if a skin is unlocked, we use a binary search.
        // Thus, the list of unlocked skins must be sorted at all times
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


        public Skin GetSkinFromId(int id)
        {
            foreach (Skin skin in _skinsList)
            {
                if (skin.id == id)
                {
                    return skin;
                }
            }

            return _skinsList[0];
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


        /// <summary>
        /// Return a boolean indicating if the material for the skin matching the given id is cached
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsMaterialCached(int id)
        {
            return _skinMaterials.ContainsKey(id);
        }


        /// <summary>
        /// Return the material for the skin matching the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Material GetMaterialFromCache(int id)
        {
            return _skinMaterials[id];
        }


        /// <summary>
        /// Load the material at materialName from addressables and call the callback method with the result
        /// </summary>
        /// <param name="id"></param>
        /// <param name="materialName"></param>
        /// <param name="callback"></param>
        public static void LoadMaterialFromAddressables(int id, string materialName, Action<bool, Material> callback)
        {
            _skinMaterialHandle = Addressables.LoadAssetAsync<Material>($"Materials/{materialName}");
            _skinMaterialHandle.Completed += operation => OnMaterialLoaded(id, operation, callback);
        }


        /// <summary>
        /// Called asynchronously when the material for the skin is loaded.
        /// It caches the material.
        /// It also calls the callback method with the result (the first parameter
        /// is a boolean indicating if the material was loaded successfully, and the
        /// second one is the material itself, or null if it failed)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operation"></param>
        /// <param name="callback"></param>
        private static void OnMaterialLoaded(int id, AsyncOperationHandle<Material> operation, Action<bool, Material> callback)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _skinMaterials.Add(id, operation.Result);

                callback(true, operation.Result);
            }
            else
            {
                callback(false, null);
            }

            _skinMaterialHandle.Completed -= operation => OnMaterialLoaded(id, operation, callback);
            Addressables.Release(_skinMaterialHandle);
        }


        public bool BuySkin(int id)
        {
            if (!IsSkinUnlocked(id))
            {
                Skin skin = GetSkinFromId(id);

                if (PlayerManager.Instance.CoinsManager.HasEnoughCoins(skin.price))
                {
                    _unlockedSkins.Add(id);
                    _unlockedSkins.Sort();

                    PlayerManager.Instance.CoinsManager.UpdateCoins(-skin.price);

                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Equip the skin matching the given id by applying the skin's material to the ball
        /// </summary>
        /// <param name="id"></param>
        public void EquipSkin(int id)
        {
            if (IsSkinUnlocked(id))
            {
                EquippedSkin = id;

                if (IsMaterialCached(id))
                {
                    LevelManager.Instance.Ball.UpdateBallMaterial(true, GetMaterialFromCache(id));
                }
                else
                {
                    LoadMaterialFromAddressables(id, GetSkinFromId(id).materialPath, LevelManager.Instance.Ball.UpdateBallMaterial);
                }
            }
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

                // Once the skins list is loaded, we can equip the player's skin
                EquipSkin(EquippedSkin);
            }
            else
            {
                // Save the exception that occurred while loading the skins list
                SkinListLoadingException = operation.OperationException;
            }

            _skinsListLoadHandle.Completed -= OnSkinsListLoaded;
            Addressables.Release(_skinsListLoadHandle);
        }
    }
}