using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BallMaze.UI
{
    public class MainImageButton : AspectRatio
    {
        public new class UxmlFactory : UxmlFactory<MainImageButton, UxmlTraits> { }
        public new class UxmlTraits : AspectRatio.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<MainButtonType> type = new() { name = "type", defaultValue = MainButtonType.Primary };
            readonly UxmlEnumAttributeDescription<MainButtonShape> shape = new() { name = "shape", defaultValue = MainButtonShape.Rectangle };
            readonly UxmlEnumAttributeDescription<MainButtonBorderSize> borderSize = new() { name = "border-size", defaultValue = MainButtonBorderSize.OnePointFive };
            readonly UxmlStringAttributeDescription icon = new() { name = "icon", defaultValue = "UI/Icons/" };
            readonly UxmlStringAttributeDescription text = new() { name = "text", defaultValue = "" };
            readonly UxmlColorAttributeDescription textColor = new() { name = "text-color", defaultValue = Color.white };
            readonly UxmlIntAttributeDescription outlineWidth = new() { name = "outline-width", defaultValue = 2 };
            readonly UxmlColorAttributeDescription outlineColor = new() { name = "outline-color", defaultValue = Color.black };
            readonly UxmlIntAttributeDescription letterSpacing = new() { name = "letter-spacing", defaultValue = 2 };
            readonly UxmlIntAttributeDescription paddingTopBottom = new() { name = "padding-top-bottom", defaultValue = 10 };
            readonly UxmlIntAttributeDescription paddingLeftRight = new() { name = "padding-left-right", defaultValue = 10 };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
            {
                base.Init(visualElement, attributes, creationContext);

                MainImageButton element = visualElement as MainImageButton;

                if (element != null)
                {
                    element.Type = type.GetValueFromBag(attributes, creationContext);
                    element.Shape = shape.GetValueFromBag(attributes, creationContext);
                    element.BorderSize = borderSize.GetValueFromBag(attributes, creationContext);
                    element.PaddingTopBottom = paddingTopBottom.GetValueFromBag(attributes, creationContext);
                    element.PaddingLeftRight = paddingLeftRight.GetValueFromBag(attributes, creationContext);

                    if (element.Label != null && text != null)
                    {
                        element.Label.text = text.GetValueFromBag(attributes, creationContext);
                    }

                    // Style element with the given attributes
                    element.Button.style.color = textColor.GetValueFromBag(attributes, creationContext);
                    element.Button.style.unityTextOutlineWidth = outlineWidth.GetValueFromBag(attributes, creationContext);
                    element.Button.style.unityTextOutlineColor = outlineColor.GetValueFromBag(attributes, creationContext);
                    element.Button.style.textShadow = new StyleTextShadow(new TextShadow() { blurRadius = 0, color = outlineColor.GetValueFromBag(attributes, creationContext), offset = new Vector2(5, 10) });
                    element.Button.style.letterSpacing = letterSpacing.GetValueFromBag(attributes, creationContext);
                    element.Icon.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>(icon.GetValueFromBag(attributes, creationContext)));

                    // If the text attribute is empty, hide the label
                    element.Label.style.display = string.IsNullOrEmpty(text.GetValueFromBag(attributes, creationContext))
                        ? DisplayStyle.None
                        : DisplayStyle.Flex;
                }

                element.style.fontSize = 1; // Triggers OnGeometryChanged callback
            }
        }

        public Button Button => this.Q<Button>("main-image-button__button");
        public VisualElement Icon => this.Q<VisualElement>("main-image-button__icon");
        public Label Label => this.Q<Label>("main-image-button__label");

        public MainButtonType Type { get; private set; }
        public MainButtonShape Shape { get; private set; }
        public MainButtonBorderSize BorderSize { get; private set; }
        public int PaddingTopBottom { get; private set; }
        public int PaddingLeftRight { get; private set; }

        private int textRefreshes = 0;

        private const int MAX_FONT_REFRESHES = 5;

        public MainImageButton()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("UI/MainImageButton");
            visualTree.CloneTree(this);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }

        void OnAttachToPanelEvent(AttachToPanelEvent e)
        {
            parent?.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            UpdateFontSize();
            Button.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("UI/MainButtonsBackground/" + Type.ToString() + "_Button_" + Shape.ToString() + "_V2_Border-" + ((float)BorderSize + 0.5f)));
        }

        void OnGeometryChangedEvent(GeometryChangedEvent e)
        {
            // Use width instead of padding, otherwise the label under the icon will also be affected
            Icon.style.width = resolvedStyle.width - 2 * PaddingLeftRight;
            Icon.style.maxHeight = resolvedStyle.height - 2 * PaddingTopBottom;

            UpdateFontSize();
        }


        private void UpdateFontSize()
        {
            if (Label.style.display == DisplayStyle.None)
            {
                // If the label is hidden, move the icon to the center
                Icon.style.translate = new StyleTranslate(new Translate(0, 0));
                return;
            }

            // If the label is visible, move the icon up
            Icon.style.translate = new StyleTranslate(new Translate(0, Length.Percent(-10)));

            if (textRefreshes < MAX_FONT_REFRESHES)
            {
                Vector2 textSize = Label.MeasureTextSize(Label.text, float.MaxValue, MeasureMode.AtMost, float.MaxValue, MeasureMode.AtMost);
                float fontSize = Mathf.Max(style.fontSize.value.value, 1); // Unity can return a font size of 0 which would break the auto fit
                float heightDictatedFontSize = Mathf.Abs(Label.resolvedStyle.height);
                float widthDictatedFontSize = Mathf.Abs(Label.resolvedStyle.width / textSize.x) * fontSize;
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