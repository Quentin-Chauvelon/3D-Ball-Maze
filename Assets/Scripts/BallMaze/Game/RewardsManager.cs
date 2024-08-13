using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BallMaze
{
    /// <summary>
    /// The type of reward that can be earned
    /// </summary>
    public enum RewardType
    {
        Unknowned,
        Coins,
        Skin
    }

    /// <summary>
    /// A class representing a coin earned as a reward (daily rewards, daily levels streak...)
    /// </summary>
    public class CoinReward
    {
        public int amount;

        public CoinReward(JObject coinReward)
        {
            amount = coinReward.GetValue("amount").Value<int>();
        }
    }


    /// <summary>
    /// A class representing a skin earned as a reward (daily rewards, daily levels streak...)
    /// The skin can either be random (if random is true) or be a specific skin (if random is false and id is not -1)
    /// </summary>
    public class SkinReward
    {
        public int id;
        public bool random;
        public SkinRarity rarity;
        public int amount;


        public SkinReward(JObject skinReward)
        {
            id = skinReward.GetValue("id").Value<int>();
            random = skinReward.GetValue("random").Value<bool>();
            rarity = (SkinRarity)skinReward.GetValue("rarity").Value<int>();
            amount = skinReward.GetValue("amount").Value<int>();
        }
    }


    public static class RewardsManager
    {
        public static RewardType GetRewardType(object reward)
        {
            int rewardType = ((JObject)reward).GetValue("type").Value<int>();

            return Enum.IsDefined(typeof(RewardType), rewardType)
                ? (RewardType)rewardType
                : RewardType.Unknowned;
        }

        public static (bool, RewardType) HandleReward(object reward)
        {
            RewardType rewardType = GetRewardType(reward);

            switch (rewardType)
            {
                case RewardType.Coins:
                    return (HandleCoinReward(new CoinReward((JObject)reward)), rewardType);
                case RewardType.Skin:
                    return (HandleSkinReward(new SkinReward((JObject)reward)), rewardType);
                default:
                    return (false, RewardType.Unknowned);
            }
        }


        private static bool HandleCoinReward(CoinReward coinReward)
        {
            PlayerManager.Instance.CoinsManager.UpdateCoins(coinReward.amount);

            return true;
        }


        private static bool HandleSkinReward(SkinReward skinReward)
        {
            // TODO: Implement this
            Debug.LogError("HandleSkinReward not implemented yet");
            return true;
        }


        public static CoinReward GetCoinReward(object reward)
        {
            return new CoinReward((JObject)reward);
        }


        public static SkinReward GetSkinReward(object reward)
        {
            return new SkinReward((JObject)reward);
        }
    }
}