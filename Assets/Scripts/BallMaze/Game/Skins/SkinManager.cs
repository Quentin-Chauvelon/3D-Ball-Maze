using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze
{
    public enum SkinRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }


    public class SkinManager
    {
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
                case SkinRarity.Legendary:
                    return "Legendary";
                default:
                    return "???";
            }
        }
    }
}