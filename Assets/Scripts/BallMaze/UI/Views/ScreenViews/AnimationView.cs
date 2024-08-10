using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    // Invisible view that takes up the whole screen and is only used to animate UI elements (eg: coins earnt)
    public class AnimationView : ScreenView
    {
        public AnimationView(VisualElement root) : base(root)
        {

        }


        /// <summary>
        /// Animate the coins from the start position to the end position.
        /// </summary>
        /// <param name="startPosition">A Vector2 representing the start position (in pixels)</param>
        /// <param name="endPosition">A Vector2 representing the end position (in pixels)</param>
        /// <param name="numberOfCoins">The number of coins to spawn during the animation. Defaults to UIManager.DEFAULT_NUMBER_OF_COINS_TO_ANIMATE</param>
        /// <param name="duration">The duration of the animation in seconds. Defaults to UIManager.ANIMATION_DURATION</param>
        /// <returns></returns>
        public async void AnimateCoins(Vector2 startPosition, Vector2 endPosition, int? numberOfCoins = null, float? duration = null)
        {
            numberOfCoins = numberOfCoins ?? UIManager.Instance.DEFAULT_NUMBER_OF_COINS_TO_ANIMATE;
            duration = duration ?? UIManager.Instance.ANIMATION_DURATION;

            for (int i = 0; i < numberOfCoins.Value; i++)
            {
                VisualElement coin = new VisualElement();

                coin.AddToClassList("coin");

                coin.style.top = startPosition.y;
                coin.style.left = startPosition.x;

                _root.Add(coin);

                // Tween the coins from the start to the end position
                DOTween.To(() => startPosition.y, x => coin.style.top = x, endPosition.y, duration.Value).SetEase(Ease.InQuart);
                DOTween.To(() => startPosition.x, x => coin.style.left = x, endPosition.x, duration.Value).SetEase(Ease.InQuart).OnComplete(() =>
                {
                    coin.RemoveFromHierarchy();
                });

                await UniTask.Delay(TimeSpan.FromMilliseconds(UIManager.Instance.DELAY_BETWEEN_COINS));
            }
        }


        /// <summary>
        /// Animate the daily level streak once the player completes the last daily level.
        /// </summary>
        /// <param name="streak"></param> <summary>
        /// <returns></returns>
        public async void AnimateDailyLevelStreak(int streak)
        {
            VisualElement dailyLevelsStreakContainer = (UIManager.Instance.UIViews[UIViewType.DailyLevels] as DailyLevelsView).GetStreakElement();

            // Clone the daily levels streak to animate it
            VisualElement dailyLevelsStreakContainerClone = dailyLevelsStreakContainer.visualTreeAssetSource.CloneTree().Q<VisualElement>("daily-levels__streak-container");

            dailyLevelsStreakContainerClone.style.opacity = 0;
            dailyLevelsStreakContainerClone.style.position = Position.Absolute;
            dailyLevelsStreakContainerClone.style.bottom = Length.Percent(8);
            dailyLevelsStreakContainerClone.style.marginBottom = 0;

            _root.Add(dailyLevelsStreakContainerClone);

            // Make each day before the streak active
            for (int i = 1; i <= streak - 1; i++)
            {
                VisualElement dailyLevelStreakDay = dailyLevelsStreakContainerClone.Q<VisualElement>($"daily-levels__streak-day-{i}");

                dailyLevelStreakDay.RemoveFromClassList("daily-levels__streak-day-inactive");
                dailyLevelStreakDay.AddToClassList("daily-levels__streak-day-active");

                // On the last day, keep the connection bar white, since we want to animate it
                if (i == streak - 1)
                {
                    dailyLevelStreakDay.Q<VisualElement>("daily-levels__streak-next-day-connection").style.backgroundColor = Color.white;
                }
            }

            // Fade in the daily levels streak container clone
            await DOTween.To(() => 0f, x => dailyLevelsStreakContainerClone.style.opacity = x, 1f, UIManager.Instance.STREAK_FADE_ANIMATION_DURATION).SetEase(UIManager.Instance.STREAK_FADE_IN_EASING_FUNCTION).AsyncWaitForCompletion();

            await UniTask.WaitForSeconds(1);

            // If the player's streak is on the first day, don't animate the connection bar
            if (streak != 1)
            {
                // Get the connection bar of the next day to animate it
                VisualElement dailyLevelStreakDayNext = dailyLevelsStreakContainerClone.Q<VisualElement>($"daily-levels__streak-day-{streak - 1}");
                VisualElement dailyLevelStreakDayNextConnectionBar = dailyLevelStreakDayNext.Q<VisualElement>("daily-levels__streak-next-day-connection");

                // Create a new connection bar with the same position and size as the next day's connection bar
                VisualElement connectionAnimation = InstantiateDailyLevelStreakVisualElement(
                    height: dailyLevelStreakDayNextConnectionBar.resolvedStyle.height,
                    top: dailyLevelStreakDayNext.resolvedStyle.top + dailyLevelStreakDayNextConnectionBar.resolvedStyle.top,
                    left: dailyLevelStreakDayNext.resolvedStyle.left + dailyLevelStreakDayNext.Q<VisualElement>("daily-levels__streak-day-container").resolvedStyle.width,
                    parent: dailyLevelsStreakContainerClone
                );

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                // Animate the new connection bar to grow from left to right to fill the next day's connection bar
                await DOTween.To(() => 0, x => connectionAnimation.style.width = x, dailyLevelStreakDayNextConnectionBar.resolvedStyle.width, UIManager.Instance.STREAK_CONNECTION_BAR_ANIMATION_DURATION / 2).SetEase(Ease.Linear).AsyncWaitForCompletion();

                // Remove the connection bar after the animation is complete
                connectionAnimation.RemoveFromHierarchy();

                // Make the real connection bar active after destroying the clone connection bar
                dailyLevelStreakDayNextConnectionBar.style.backgroundColor = UIManager.Instance.STREAK_ACTIVE_COLOR;
            }

            VisualElement dailyLevelStreakNewDay = dailyLevelsStreakContainerClone.Q<VisualElement>($"daily-levels__streak-day-{streak}");

            VisualElement newDayAnimation = InstantiateDailyLevelStreakVisualElement(
                height: dailyLevelStreakNewDay.resolvedStyle.height,
                top: dailyLevelStreakNewDay.resolvedStyle.top,
                left: dailyLevelStreakNewDay.resolvedStyle.left,
                parent: dailyLevelsStreakContainerClone
            );

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            // Get the target width based on the streak's day.
            // If it's day 7, the width is the whole element (since there is no connection bar),
            // otherwise, it's just the container width (wihout the connection bar)
            float targetWidth = streak != 7
                ? dailyLevelStreakNewDay.Q<VisualElement>("daily-levels__streak-day-container").resolvedStyle.width
                : dailyLevelStreakNewDay.resolvedStyle.width;

            // Animate the new day container to grow from left to right to fill the next day's day container
            await DOTween.To(() => 0, x => newDayAnimation.style.width = x, targetWidth, UIManager.Instance.STREAK_CONNECTION_BAR_ANIMATION_DURATION / 2).SetEase(Ease.Linear).AsyncWaitForCompletion();

            // Remove the newDay element after the animation is complete
            newDayAnimation.RemoveFromHierarchy();

            // Make the real connection newDay element active after destroying the newDay's clone
            if (streak != 7)
            {
                dailyLevelStreakNewDay.Q<VisualElement>("daily-levels__streak-day-container").style.backgroundColor = UIManager.Instance.STREAK_ACTIVE_COLOR;
            }
            else
            {
                dailyLevelStreakNewDay.style.backgroundColor = UIManager.Instance.STREAK_ACTIVE_COLOR;
            }

            await UniTask.WaitForSeconds(1);

            // Get the target width based on the streak's day.
            // If it's day 7, the width is the whole element (since there is no connection bar),
            // otherwise, it's just the container width (wihout the connection bar)
            VisualElement dailyLevelStreakNewDayDayContainer = streak != 7
                ? dailyLevelStreakNewDay.Q<VisualElement>("daily-levels__streak-day-container")
                : dailyLevelStreakNewDay;

            // Animate the coins from the center of the new day's day container
            AnimateCoins(
                new Vector2(
                    dailyLevelStreakNewDayDayContainer.worldBound.x + dailyLevelStreakNewDayDayContainer.worldBound.width / 2 - UIManager.Instance.STREAK_COIN_SIZE / 2,
                    dailyLevelStreakNewDayDayContainer.worldBound.y + dailyLevelStreakNewDayDayContainer.worldBound.height / 2 - UIManager.Instance.STREAK_COIN_SIZE / 2),
                (UIManager.Instance.UIViews[UIViewType.Permanent] as PermanentView).GetCoinsImagePosition(),
                numberOfCoins: UIManager.Instance.STREAK_NUMBER_OF_COINS_TO_ANIMATE,
                duration: UIManager.Instance.STREAK_COIN_ANIMATION_DURATION
            );

            // Wait for the coins animation to complete
            await UniTask.Delay((int)(UIManager.Instance.STREAK_NUMBER_OF_COINS_TO_ANIMATE * UIManager.Instance.STREAK_COIN_ANIMATION_DURATION * 1000));

            // Fade out the daily levels streak container clone to end the animation and clean up
            await DOTween.To(() => 1f, x => dailyLevelsStreakContainerClone.style.opacity = x, 0f, UIManager.Instance.STREAK_FADE_ANIMATION_DURATION).SetEase(UIManager.Instance.STREAK_FADE_OUT_EASING_FUNCTION).AsyncWaitForCompletion();

            dailyLevelsStreakContainerClone.RemoveFromHierarchy();
        }


        private VisualElement InstantiateDailyLevelStreakVisualElement(float height, float top, float left, VisualElement parent)
        {
            VisualElement visualElement = new VisualElement();
            visualElement.style.position = Position.Absolute;
            visualElement.style.backgroundColor = UIManager.Instance.STREAK_ACTIVE_COLOR;
            visualElement.style.width = 0;
            visualElement.style.height = height;
            visualElement.style.top = top;
            visualElement.style.left = left;

            parent.Add(visualElement);

            return visualElement;
        }


        /// <summary>
        /// Return the duration of the full level daily level streak animation
        /// from the moment the player reaches the target to the end of the last coin animation.
        /// </summary>
        /// <returns></returns>
        public static int GetDailyLevelCompletedAnimationDuration()
        {
            // The delay is calculated as follows:
            // - UI fade in duration
            // - 1s delay
            // - Half of the duration of the connection bar animation --> for the connection bar
            // - Half of the Duration of the connection bar animation --> for the new day's container
            // - 1s delay
            // - Half of the duration of the coin animation
            return
                (int)((
                    UIManager.Instance.STREAK_FADE_ANIMATION_DURATION +
                    1 +
                    UIManager.Instance.STREAK_CONNECTION_BAR_ANIMATION_DURATION / 2 +
                    UIManager.Instance.STREAK_CONNECTION_BAR_ANIMATION_DURATION / 2 +
                    1 +
                    UIManager.Instance.STREAK_NUMBER_OF_COINS_TO_ANIMATE * UIManager.Instance.STREAK_COIN_ANIMATION_DURATION / 2
                ) * 1000);
        }
    }
}