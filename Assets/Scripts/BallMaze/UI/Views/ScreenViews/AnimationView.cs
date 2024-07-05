using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    // Invisible view that takes up the whole screen and is only used to animate UI elements (eg: coins earnt)
    public class AnimationView : ScreenView
    {
        // The number of milliseconds to wait before animating the next coin
        public const int DELAY_BETWEEN_COINS_ANIMATION = 100;

        public const int DEFAULT_NUMBER_OF_COINS_TO_ANIMATE = 20;

        public AnimationView(VisualElement root) : base(root)
        {

        }


        public async void AnimateCoins(Vector2 startPosition, Vector2 endPosition, int numberOfCoins = DEFAULT_NUMBER_OF_COINS_TO_ANIMATE, float duration = 0.25f)
        {
            for (int i = 0; i < numberOfCoins; i++)
            {
                VisualElement coin = new VisualElement();

                coin.AddToClassList("coin");

                coin.style.top = startPosition.y;
                coin.style.left = startPosition.x;

                _root.Add(coin);

                // Tween the coins from the start to the end position
                DOTween.To(() => startPosition.y, x => coin.style.top = x, endPosition.y, duration).SetEase(Ease.InQuart);
                DOTween.To(() => startPosition.x, x => coin.style.left = x, endPosition.x, duration).SetEase(Ease.InQuart).OnComplete(() =>
                {
                    coin.RemoveFromHierarchy();
                });

                await UniTask.Delay(TimeSpan.FromMilliseconds(DELAY_BETWEEN_COINS_ANIMATION));
            }
        }
    }
}