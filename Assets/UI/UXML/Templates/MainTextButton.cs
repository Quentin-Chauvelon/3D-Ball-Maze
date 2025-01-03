using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BallMaze.UI
{
    public class MainTextButton : AspectRatio
    {
        public new class UxmlFactory : UxmlFactory<MainTextButton, UxmlTraits> { }
        public new class UxmlTraits : AspectRatio.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<MainButtonType> type = new() { name = "type", defaultValue = MainButtonType.Primary };
            readonly UxmlEnumAttributeDescription<MainButtonShape> shape = new() { name = "shape", defaultValue = MainButtonShape.Rectangle };
            readonly UxmlEnumAttributeDescription<MainButtonBorderSize> borderSize = new() { name = "border-size", defaultValue = MainButtonBorderSize.OnePointFive };
            readonly UxmlStringAttributeDescription customBackground = new() { name = "custom-background", defaultValue = "" };
            readonly UxmlStringAttributeDescription text = new() { name = "text", defaultValue = "" };
            readonly UxmlColorAttributeDescription textColor = new() { name = "text-color", defaultValue = Color.white };
            readonly UxmlIntAttributeDescription outlineWidth = new() { name = "outline-width", defaultValue = 2 };
            readonly UxmlColorAttributeDescription outlineColor = new() { name = "outline-color", defaultValue = Color.black };
            readonly UxmlIntAttributeDescription letterSpacing = new() { name = "letter-spacing", defaultValue = 2 };
            readonly UxmlIntAttributeDescription paddingTopBottom = new() { name = "padding-top-bottom", defaultValue = 0 };
            readonly UxmlIntAttributeDescription paddingLeftRight = new() { name = "padding-left-right", defaultValue = 0 };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
            {
                base.Init(visualElement, attributes, creationContext);

                MainTextButton element = visualElement as MainTextButton;

                if (element != null)
                {
                    element.Type = type.GetValueFromBag(attributes, creationContext);
                    element.Shape = shape.GetValueFromBag(attributes, creationContext);
                    element.BorderSize = borderSize.GetValueFromBag(attributes, creationContext);
                    element.CustomBackground = customBackground.GetValueFromBag(attributes, creationContext);

                    if (element.Button != null && text != null)
                    {
                        element.Button.text = text.GetValueFromBag(attributes, creationContext);
                    }

                    element.Button.style.color = textColor.GetValueFromBag(attributes, creationContext);
                    element.Button.style.unityTextOutlineWidth = outlineWidth.GetValueFromBag(attributes, creationContext);
                    element.Button.style.unityTextOutlineColor = outlineColor.GetValueFromBag(attributes, creationContext);
                    element.Button.style.textShadow = new StyleTextShadow(new TextShadow() { blurRadius = 0, color = outlineColor.GetValueFromBag(attributes, creationContext), offset = new Vector2(5, 10) });
                    element.Button.style.letterSpacing = letterSpacing.GetValueFromBag(attributes, creationContext);

                    // Only set the padding if it has been specified so that it can be overridden by the USS stylesheet
                    if (paddingTopBottom.GetValueFromBag(attributes, creationContext) != 0)
                    {
                        element.Button.style.paddingTop = paddingTopBottom.GetValueFromBag(attributes, creationContext);
                        element.Button.style.paddingBottom = paddingTopBottom.GetValueFromBag(attributes, creationContext);
                    }

                    if (paddingLeftRight.GetValueFromBag(attributes, creationContext) != 0)
                    {
                        element.Button.style.paddingLeft = paddingLeftRight.GetValueFromBag(attributes, creationContext);
                        element.Button.style.paddingRight = paddingLeftRight.GetValueFromBag(attributes, creationContext);
                    }

                    // If the padding has been set, center the text
                    if (paddingTopBottom.GetValueFromBag(attributes, creationContext) != 0 || paddingLeftRight.GetValueFromBag(attributes, creationContext) != 0)
                    {
                        element.Button.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
                    }
                }

                element.style.fontSize = 1; // Triggers OnGeometryChanged callback
            }
        }

        public Button Button => this.Q<Button>("main-text-button__button");

        public MainButtonType Type { get; private set; }
        public MainButtonShape Shape { get; private set; }
        public MainButtonBorderSize BorderSize { get; private set; }
        public string CustomBackground { get; private set; }

        private int textRefreshes = 0;

        private const int MAX_FONT_REFRESHES = 5;

        public MainTextButton()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("UI/MainTextButton");
            visualTree.CloneTree(this);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }

        void OnAttachToPanelEvent(AttachToPanelEvent e)
        {
            parent?.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            UpdateFontSize();

            Button.style.backgroundImage = string.IsNullOrEmpty(CustomBackground)
                ? new StyleBackground(Resources.Load<Texture2D>("UI/MainButtonsBackground/" + Type.ToString() + "_Button_" + Shape.ToString() + "_V2_Border-" + ((float)BorderSize + 0.5f)))
                : new StyleBackground(Resources.Load<Texture2D>(CustomBackground));
        }

        void OnGeometryChangedEvent(GeometryChangedEvent e)
        {
            UpdateFontSize();
        }


        private void UpdateFontSize()
        {
            if (textRefreshes < MAX_FONT_REFRESHES)
            {
                Vector2 textSize = Button.MeasureTextSize(Button.text, float.MaxValue, MeasureMode.AtMost, float.MaxValue, MeasureMode.AtMost);
                float fontSize = Mathf.Max(style.fontSize.value.value, 1); // Unity can return a font size of 0 which would break the auto fit
                float heightDictatedFontSize = Mathf.Abs(Button.resolvedStyle.height) - Button.resolvedStyle.paddingTop - Button.resolvedStyle.paddingBottom;
                float widthDictatedFontSize = Mathf.Abs((Button.resolvedStyle.width - Button.resolvedStyle.paddingLeft - Button.resolvedStyle.paddingRight) / textSize.x) * fontSize;
                float newFontSize = Mathf.FloorToInt(Mathf.Min(heightDictatedFontSize, widthDictatedFontSize));
                newFontSize = Mathf.Clamp(newFontSize, 0, 200);
                if (Mathf.Abs(newFontSize - fontSize) > 1)
                {
                    textRefreshes++;
                    style.fontSize = new StyleLength(new Length(newFontSize));
                }
            }
            else
            {
                textRefreshes = 0;
            }

        }
    }
}