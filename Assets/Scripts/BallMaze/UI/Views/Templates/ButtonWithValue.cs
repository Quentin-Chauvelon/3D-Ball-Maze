using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    class ButtonWithValue : Button
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<ButtonWithValue, UxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlIntAttributeDescription customValue = new() { name = "custom_value", defaultValue = 0 };

            public override void Init(VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext)
            {
                base.Init(visualElement, attributes, creationContext);
                var element = visualElement as ButtonWithValue;

                if (element != null)
                {
                    element.CustomValue = customValue.GetValueFromBag(attributes, creationContext);
                }
            }
        }

        public int CustomValue { get; set; } = 0;
    }

}