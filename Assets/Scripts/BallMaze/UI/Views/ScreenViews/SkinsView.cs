using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class SkinsView : ScreenView
    {
        private VisualElement _skinsContainer;
        private VisualElement _skinsScrollViewContainer;

        private const int NUMBER_OF_ITEMS_PER_ROW = 4;

        private const int SKINS_ITEMS_CONTAINER_PADDING_PERCENT = 2;


        private const int GAP_PERCENT = 3;

        public SkinsView(VisualElement root) : base(root)
        {

        }


        protected override void SetVisualElements()
        {
            _skinsContainer = _root.Q<VisualElement>("skins__skins-horizontal-container");
            _skinsScrollViewContainer = _root.Q<VisualElement>("skins__skins-vertical-container");

            _skinsContainer.Q<VisualElement>("skins__item-red").Q<Button>("skins__skin-item-background").clicked += () => { PreviewSkin(0); };
            _skinsContainer.Q<VisualElement>("skins__item-blue").Q<Button>("skins__skin-item-background").clicked += () => { PreviewSkin(1); };

            _skinsContainer.Q<VisualElement>("skins__item-red").Q<Button>("skins__skin-item-background").RemoveFromClassList("skin-locked");
            _skinsContainer.Q<VisualElement>("skins__item-red").Q<Button>("skins__skin-item-background").AddToClassList("skin-unlocked");

            // Update the size of the children when the size of the container changes
            _skinsContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }


        private void PreviewSkin(int skin)
        {
            Debug.Log($"Previewing skin {skin}");
        }


        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size)
            {
                return;
            }

            float containerWidth = _skinsScrollViewContainer.resolvedStyle.width;

            // Set the max width of the skins container. We have to use this because UI toolkit can't arrange
            // items in a grid. Thus we have to use a scroll view to scroll in one direction (vertical container),
            // and then, we use a visual element to hold all the items with flex wrap set to create the grid effect (horizontal container)
            // But if we don't set the max width of this element, it expands and the items never wrap
            _skinsContainer.style.maxWidth = containerWidth;

            _skinsContainer.style.paddingTop = Length.Percent(SKINS_ITEMS_CONTAINER_PADDING_PERCENT);
            _skinsContainer.style.paddingRight = Length.Percent(SKINS_ITEMS_CONTAINER_PADDING_PERCENT);
            _skinsContainer.style.paddingBottom = Length.Percent(SKINS_ITEMS_CONTAINER_PADDING_PERCENT);
            _skinsContainer.style.paddingLeft = Length.Percent(SKINS_ITEMS_CONTAINER_PADDING_PERCENT);

            foreach (VisualElement item in _skinsContainer.Children())
            {
                // The margin is a certain percentage of the size of the container
                float itemMargin = containerWidth * 0.03f;

                // To calculate the size of each item, we remove the margins of each item (number of items * 2 because they
                // each have margins on each side) and the padding of the container. And then we divide it by the number of
                // items that we want per row. Finally, substract 3 from the result, to have a little room, otherwise
                // sometimes the 4th item is wrapped to the following row, leaving one less item for each row
                float itemSize = (containerWidth - SKINS_ITEMS_CONTAINER_PADDING_PERCENT * 2 - itemMargin * (NUMBER_OF_ITEMS_PER_ROW * 2)) / NUMBER_OF_ITEMS_PER_ROW - 3;

                // Set the margin of each item
                item.style.marginTop = itemMargin;
                item.style.marginRight = itemMargin;
                item.style.marginBottom = itemMargin;
                item.style.marginLeft = itemMargin;

                // Set the size of each item
                VisualElement itemImage = item.Q<VisualElement>("skins__skin-item-image");
                itemImage.style.minWidth = itemSize;
                itemImage.style.maxWidth = itemSize;
                itemImage.style.minHeight = itemSize;
                itemImage.style.maxHeight = itemSize;
            }
        }
    }
}