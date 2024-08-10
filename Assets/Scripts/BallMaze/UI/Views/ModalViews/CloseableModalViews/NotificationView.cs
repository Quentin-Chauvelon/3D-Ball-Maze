using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public enum NotificationType
    {
        Info,
        Warning,
        Error
    }

    public class NotificationView : ModalView
    {
        public override bool isCloseable => false;

        // Top visual element
        private VisualElement _notificationContainer;

        // Visual elements
        private Button _notificationBackgroundButton;
        private VisualElement _notificationImageBackground;
        private VisualElement _notificationImage;
        private Label _notificationText;
        private Label _notificationSizingLabel;

        // The percentage of the screen the notification image should be in width
        private const float NOTIFICATION_IMAGE_WIDTH_PERCENTAGE = 0.04f;

        // The margin (in percent) between the image and the text
        private const float NOTIFICATION_IMAGE_TEXT_MARGIN = 0.02f;

        // The distance (in percent) from the top of the screen
        private const float NOTIFICATION_MARGIN_FROM_TOP = 0.03f;

        // The duration in seconds of the expanding animation of the notification (after it has drop down)
        private const float NOTIFICATION_TWEEN_DURATION = 0.5f;


        public NotificationView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _notificationBackgroundButton = _root.Q<Button>("notification__background");
            _notificationImageBackground = _root.Q<VisualElement>("notification__image-background");
            _notificationImage = _root.Q<VisualElement>("notification__image");
            _notificationText = _root.Q<Label>("notification__text");
            _notificationSizingLabel = _root.Q<Label>("notification__sizing-label");
        }


        public async override void Hide()
        {
            // Tween the opacity of the text from opaque to transparent
            DOTween.To(() => 1, x => _notificationText.style.opacity = x, 0, NOTIFICATION_TWEEN_DURATION);

            // Shrink animation
            await DOTween.To(() => _notificationBackgroundButton.resolvedStyle.width, x => _notificationBackgroundButton.style.width = x, _notificationImageBackground.resolvedStyle.width, NOTIFICATION_TWEEN_DURATION).SetEase(Ease.OutQuart).AsyncWaitForCompletion();

            _notificationText.text = "";

            await _notificationBackgroundButton.TweenToPosition(GameManager.GetScreenSize().y * -0.07f);

            base.Hide();
        }


        /// <summary>
        /// Display a notification according to the given arguments
        /// </summary>
        /// <param name="text"></param>
        /// <param name="notificationType"></param>
        /// <param name="duration">The duration in seconds before the notification is hidden</param>
        public async void Notify(string text, NotificationType notificationType = NotificationType.Info, int duration = 7)
        {
            // If there is already a notification being displayed, hide it and wait before displaying the new one
            if (isEnabled)
            {
                Hide();

                await UniTask.Delay((int)(UIUtitlities.UI_TWEEN_DURATION * 1000) + 500);
            }

            // Move the notification out of the top of the screen
            _notificationBackgroundButton.style.top = Length.Percent(-7);
            _notificationText.text = "";

            Show();

            _notificationBackgroundButton.clicked += () => { UIManager.Instance.Hide(UIViewType.Notification); };

            _notificationSizingLabel.text = text;

            float screenWidth = GameManager.GetScreenSize().x;

            _notificationBackgroundButton.style.width = screenWidth * NOTIFICATION_IMAGE_WIDTH_PERCENTAGE;
            // Setting the image min width and max width here allows it to keep the same width during the whole animation since it is set in pixels
            // We also have to set min and max, otherwise the text will squash it
            _notificationImageBackground.style.minWidth = screenWidth * NOTIFICATION_IMAGE_WIDTH_PERCENTAGE;
            _notificationImageBackground.style.maxWidth = screenWidth * NOTIFICATION_IMAGE_WIDTH_PERCENTAGE;

            // Wait one frame for the text to render to get the width
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            // Move the notification down inside the screen
            await _notificationBackgroundButton.TweenToPosition(GameManager.GetScreenSize().y * NOTIFICATION_MARGIN_FROM_TOP);

            _notificationText.text = text;
            _notificationText.style.opacity = 1;

            float totalWidth = screenWidth * NOTIFICATION_IMAGE_WIDTH_PERCENTAGE + _notificationSizingLabel.resolvedStyle.width + screenWidth * NOTIFICATION_IMAGE_TEXT_MARGIN;

            // Remove the text, otherwise clicks are blocked and can't get through the notification sizing label
            _notificationSizingLabel.text = "";

            // Tween the opacity of the text from transparent to opaque
            DOTween.To(() => 0, x => _notificationText.style.opacity = x, 1, NOTIFICATION_TWEEN_DURATION - 0.2f);

            // Expand animation
            await DOTween.To(() => _notificationBackgroundButton.resolvedStyle.width, x => _notificationBackgroundButton.style.width = x, totalWidth, NOTIFICATION_TWEEN_DURATION).SetEase(Ease.OutQuart).AsyncWaitForCompletion();

            await UniTask.WaitForSeconds(duration);

            Hide();
        }
    }
}