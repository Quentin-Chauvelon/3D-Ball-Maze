using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BallMaze.UI;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;


namespace BallMaze
{
    public class DailyReward : MonoBehaviour
    {
        public static bool Collected = false;
        public static int DailyRewardStreak;
        public static int LastDailyRewardClaimedDay;

        // List of objects containing the rewards for each day
        // We use objects because the rewards can be either coins or skins which have different properties
        public static object[] DailyRewards = null;


        public static void PopulateDailyRewards(object[] rewards)
        {
            DailyRewards = rewards;

            (UIManager.Instance.UIViews[UIViewType.DailyReward] as DailyRewardView).PopulateDailyRewards(rewards, DailyRewardStreak);
        }


        public static bool CollectDailyReward(int day)
        {
            // If the reward has already been collected or the player didn't click the reward corresponding to the right day (streak + 1)
            if (Collected || day != DailyRewardStreak + 1)
            {
                return false;
            }

            (bool result, RewardType rewardType) = RewardsManager.HandleReward(DailyRewards[day - 1]);

            if (result)
            {
                Collected = true;
                DailyRewardStreak++;
                LastDailyRewardClaimedDay = GameManager.Instance.GetUtcNowTime().DayOfYear;

                // If Cloud Save is enabled, save to the cloud
                if (DataPersistenceManager.isCloudSaveEnabled && DataPersistenceManager.cloudDataHandlerInitialized)
                {
                    _ = DataPersistenceManager.Instance.cloudDataHandler.Save(new Dictionary<CloudSaveKey, object> {
                        { CloudSaveKey.dailyRewardStreak, DailyRewardStreak},
                        { CloudSaveKey.lastDailyRewardClaimedDay, LastDailyRewardClaimedDay}
                    });
                }

                if (rewardType == RewardType.Coins)
                {
                    VisualElement dailyRewardDayButton = (UIManager.Instance.UIViews[UIViewType.DailyReward] as DailyRewardView).GetDailyRewardButtonFromDay(day);

                    // Animate the coins from the center of the day's reward
                    (UIManager.Instance.UIViews[UIViewType.Animation] as AnimationView).AnimateCoins(
                        new Vector2(
                            dailyRewardDayButton.worldBound.x + dailyRewardDayButton.worldBound.width / 2 - UIManager.Instance.DAILY_REWARD_COIN_SIZE / 2,
                            dailyRewardDayButton.worldBound.y + dailyRewardDayButton.worldBound.height / 2 - UIManager.Instance.DAILY_REWARD_COIN_SIZE / 2),
                        (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).GetCoinsImagePosition()
                    );
                }
            }

            return result;
        }
    }
}