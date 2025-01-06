using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class LabelBestFit : Label
    {
        public new class UxmlFactory : UxmlFactory<LabelBestFit, UxmlTraits> { }

        public new class UxmlTraits : AspectRatio.UxmlTraits
        {
            readonly UxmlStringAttributeDescription text = new() { name = "text", defaultValue = "" };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
            {
                base.Init(visualElement, attributes, creationContext);

                LabelBestFit element = visualElement as LabelBestFit;

                if (element != null)
                {
                    element.text = text.GetValueFromBag(attributes, creationContext);
                }

                element.style.fontSize = 1; // Triggers OnGeometryChanged callback
            }
        }

        private int textRefreshes = 0;
        private const int MAX_FONT_REFRESHES = 5;

        public LabelBestFit()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
        }

        void OnAttachToPanelEvent(AttachToPanelEvent e)
        {
            parent?.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            UpdateFontSize();
        }

        void OnGeometryChangedEvent(GeometryChangedEvent e)
        {
            if (e.newRect.width == 0 || e.newRect.height == 0)
            {
                return;
            }

            UpdateFontSize();
        }


        private void UpdateFontSize()
        {
            if (textRefreshes < MAX_FONT_REFRESHES)
            {
                Vector2 textSize = MeasureTextSize(text, float.MaxValue, MeasureMode.AtMost, float.MaxValue, MeasureMode.AtMost);
                float fontSize = Mathf.Max(style.fontSize.value.value, 1); // Unity can return a font size of 0 which would break the auto fit
                float heightDictatedFontSize = Mathf.Abs(resolvedStyle.height) - resolvedStyle.paddingTop - resolvedStyle.paddingBottom;
                float widthDictatedFontSize = Mathf.Abs((resolvedStyle.width - resolvedStyle.paddingLeft - resolvedStyle.paddingRight) / textSize.x) * fontSize;
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