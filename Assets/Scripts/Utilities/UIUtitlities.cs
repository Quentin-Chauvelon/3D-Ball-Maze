using System.Linq.Expressions;
using BallMaze;
using BallMaze.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UIElements;
using UnityExtensionMethods;

public class UIUtitlities
{
    public const float UI_TWEEN_DURATION = 0.5f;

    /// <summary>
    /// Tween the given modal view from the top to the center of the screen
    /// </summary>
    /// <param name="visualElement"></param>
    /// <param name="uiHeightPercentage">The percentage of the height taken by the UI. Must match the value in the UXML file</param>
    /// <param name="isScreenView">True if the element or its container has the "screen view" USS class, which leaves room for the permanent UI</param>
    /// <param name="duration"></param>
    public static void TweenModalViewFromTop(VisualElement visualElement, float uiHeightPercentage, bool isScreenView = true, float duration = UI_TWEEN_DURATION)
    {
        // If the visual element has the "screen view" USS class, it will leave room for the permanent UI
        // and thus only take 85% of the screen height
        float containerTotalHeight = 1f - (isScreenView ? PermanentView.UI_HEIGHT_PERCENTAGE : 0f);

        float height = GameManager.GetScreenSize().y;

        // Get the size of the UI, which is a fraction of the screen height
        float size = height * containerTotalHeight * uiHeightPercentage;

        // Move the UI out of the screen
        visualElement.style.top = -size - 30;

        // Animate the UI dropping down
        // Skip the height taken by the permanent UI, then move the UI to the center of the 85% left based on the UI height
        _ = visualElement.TweenToPosition(height * PermanentView.UI_HEIGHT_PERCENTAGE + height * (containerTotalHeight / 2) - (size / 2), duration);
    }


    public static async UniTask TweenModalViewToTopAndWait(VisualElement visualElement, float uiHeightPercentage, bool isScreenView = true, float duration = UI_TWEEN_DURATION)
    {
        // If the visual element has the "screen view" USS class, it will leave room for the permanent UI
        // and thus only take 85% of the screen height
        float containerTotalHeight = 1f - (isScreenView ? PermanentView.UI_HEIGHT_PERCENTAGE : 0f);

        // Get the size of the UI, the container of the UI is 85% of the screen height and the UI is 70% of the container
        await visualElement.TweenToPosition(-(GameManager.GetScreenSize().y * containerTotalHeight * uiHeightPercentage) - 30, duration);
    }
}
