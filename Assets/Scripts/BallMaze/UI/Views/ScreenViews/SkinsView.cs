using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using UnityExtensionMethods;


namespace BallMaze.UI
{
    public class SkinsView : ScreenView
    {
        // The skins cards are parented to this container
        private VisualElement _skinsContainer;
        private VisualElement _skinsScrollViewContainer;
        private Button _buyButton;
        private Button _equipButton;

        private Dictionary<int, Action> _skinItemClicksCallbacks;

        private SkinCategory _lastSelectedCategory = SkinCategory.Common;

        // Adressable handle to load the default level selection template
        private AsyncOperationHandle<VisualTreeAsset> _skinItemTemplateHandle;
        private VisualTreeAsset _skinItemTemplate;
        private bool _isSkinItemTemplateLoaded = false;
        private Exception _skinItemTemplateLoadingError = null;

        private const int NUMBER_OF_ITEMS_PER_ROW = 4;

        private const int SKINS_ITEMS_CONTAINER_PADDING_PERCENT = 2;

        public SkinsView(VisualElement root) : base(root)
        {
            _skinItemClicksCallbacks = new Dictionary<int, Action>();
        }


        protected override void SetVisualElements()
        {
            _skinsContainer = _root.Q<VisualElement>("skins__skins-horizontal-container");
            _skinsScrollViewContainer = _root.Q<VisualElement>("skins__skins-vertical-container");
            _buyButton = _root.Q<Button>("skins__buy-button");
            _equipButton = _root.Q<Button>("skins__equip-button");

            // Bind clicks to the category buttons
            _root.Q<Button>("skins__category-common").clicked += () => { PopulateCategory(SkinCategory.Common); };
            _root.Q<Button>("skins__category-rare").clicked += () => { PopulateCategory(SkinCategory.Rare); };
            _root.Q<Button>("skins__category-epic").clicked += () => { PopulateCategory(SkinCategory.Epic); };
            _root.Q<Button>("skins__category-unique").clicked += () => { PopulateCategory(SkinCategory.Unique); };
            _root.Q<Button>("skins__category-flags").clicked += () => { PopulateCategory(SkinCategory.Flags); };

            // Update the size of the children when the size of the container changes
            _skinsContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            _skinItemTemplateHandle = Addressables.LoadAssetAsync<VisualTreeAsset>("SkinItemTemplate");
            _skinItemTemplateHandle.Completed += SkinItemTemplateHandleCompleted;
        }


        public override async void Show()
        {
            base.Show();

            // If there has been an error while loading the skins list, show an error message
            if (SkinManager.SkinListLoadingException != null)
            {
                ExceptionManager.ShowExceptionMessage(SkinManager.SkinListLoadingException, "ExceptionMessagesTable", "SkinsListLoadingError", ExceptionActionType.BackToMainMenu);
                return;
            }

            // If there has been an error while loading the skin item card template, show an error message
            if (_skinItemTemplateLoadingError != null)
            {
                ExceptionManager.ShowExceptionMessage(_skinItemTemplateLoadingError, "ExceptionMessagesTable", "SkinItemTemplateLoadingError", ExceptionActionType.BackToMainMenu);
                return;
            }

            // If the skin list or the skin item template are not loaded, show a loading screen, with a timeout
            if (!SkinManager.IsSkinListLoaded || !_isSkinItemTemplateLoaded)
            {
                LoadingScreen.LoadingScreenUIView.InitializeLoadingScreen(LoadingIndicatorType.CircularAnimation);

                TimeoutException timeoutException = await UniTask.WaitUntil(() => SkinManager.IsSkinListLoaded && _isSkinItemTemplateLoaded).Timeout(20);

                LoadingScreen.LoadingScreenUIView.Hide();

                if (timeoutException != null)
                {
                    UIManager.Instance.Show(UIViewType.MainMenu);

                    ExceptionManager.ShowExceptionMessage(timeoutException, "ExceptionMessagesTable", "SkinsListLoadingError", ExceptionActionType.BackToMainMenu);
                }
            }

            PopulateCategory(_lastSelectedCategory);
        }


        public void PopulateCategory(SkinCategory category)
        {
            if (category == _lastSelectedCategory && _skinsContainer.childCount > 0)
            {
                return;
            }

            _lastSelectedCategory = category;

            Skin[] skins = PlayerManager.Instance.SkinManager.GetSkinsFromCategory(category);

            UnbindClicks();

            int numberOfCardsInCategory = _skinsContainer.childCount;

            // For each card already existing, update it with the new skins
            for (int i = 0; i < numberOfCardsInCategory; i++)
            {
                // If there are more cards than skins, remove the extra cards
                if (i > skins.Length - 1)
                {
                    // Don't use the index i, because the index of the cards changes when we remove one
                    // Thus, we always remove the element that is right after the last skin
                    _skinsContainer.RemoveAt(skins.Length);
                }
                // While we have enough cards, update them with the new skins
                // This way we don't have to destroy and instantiate new cards each time we change the category
                else
                {
                    VisualElement card = _skinsContainer.ElementAt(i);

                    UpdateCard(card, skins[i]);
                    BindClick(card.Q<ButtonWithValue>("skins__skin-item-background"));
                }
            }

            // If there are less cards than skins, create new cards
            if (numberOfCardsInCategory < skins.Length)
            {
                for (int i = numberOfCardsInCategory; i < skins.Length; i++)
                {
                    IntanstiateCard(skins[i]);
                }
            }
        }


        /// <summary>
        /// Instantiate a new card for the given skin and bind the click event
        /// </summary>
        /// <param name="skin"></param>
        private void IntanstiateCard(Skin skin)
        {
            VisualElement card = _skinItemTemplate.CloneTree();

            UpdateCard(card, skin);
            BindClick(card.Q<ButtonWithValue>("skins__skin-item-background"));

            _skinsContainer.Add(card);
        }


        /// <summary>
        /// Update the given card to match the information from the given skin
        /// </summary>
        /// <param name="card"></param>
        /// <param name="skin"></param>
        private void UpdateCard(VisualElement card, Skin skin)
        {
            ButtonWithValue cardBackground = card.Q<ButtonWithValue>("skins__skin-item-background");

            cardBackground.CustomValue = skin.id;

            // cardImage.style.backgroundImage = skin.materialPath;

            cardBackground.Q<Label>("skins__skin-item-price").text = skin.price.ToString();

            if (PlayerManager.Instance.SkinManager.IsSkinUnlocked(skin.id))
            {
                cardBackground.RemoveFromClassList("skin-locked");
                cardBackground.AddToClassList("skin-unlocked");
            }
            else
            {
                cardBackground.RemoveFromClassList("skin-unlocked");
                cardBackground.AddToClassList("skin-locked");
            }
        }


        /// <summary>
        /// Bind the click event of the given card
        /// </summary>
        /// <param name="card"></param>
        private void BindClick(ButtonWithValue card)
        {
            if (card == null)
            {
                return;
            }

            // The custom value of the card is the id of the skin
            Action skinItemClickedHandler = () => { PreviewSkin(card.CustomValue); };
            card.clicked += skinItemClickedHandler;
            _skinItemClicksCallbacks.Add(card.CustomValue, skinItemClickedHandler);
        }


        /// <summary>
        /// Unbind the click event of the given card
        /// </summary>
        /// <param name="card"></param>
        private void UnbindClick(ButtonWithValue card)
        {
            if (card == null)
            {
                return;
            }

            // The custom value of the card is the id of the skin
            card.clicked -= _skinItemClicksCallbacks[card.CustomValue];
        }


        /// <summary>
        /// Unbind the click events of all cards
        /// </summary>
        private void UnbindClicks()
        {
            // Unbind the click callback of each skin card
            foreach (VisualElement child in _skinsContainer.Children())
            {
                UnbindClick(child.Q<ButtonWithValue>("skins__skin-item-background"));
            }

            _skinItemClicksCallbacks.Clear();
        }


        private void PreviewSkin(int id)
        {
            Debug.Log($"Previewing skin {id}");
            List<Material> materials = new List<Material>();

            switch (id)
            {
                case 0:
                    materials.Add(Resources.Load<Material>("Red"));
                    GameObject.Find("Ball").GetComponent<MeshRenderer>().SetMaterials(materials);
                    break;
                case 1:
                    materials.Add(Resources.Load<Material>("Blue"));
                    GameObject.Find("Ball").GetComponent<MeshRenderer>().SetMaterials(materials);
                    break;
                case 2:
                    materials.Add(Resources.Load<Material>("Star"));
                    GameObject.Find("Ball").GetComponent<MeshRenderer>().SetMaterials(materials);
                    break;
                default:
                    materials.Add(Resources.Load<Material>("Red"));
                    GameObject.Find("Ball").GetComponent<MeshRenderer>().SetMaterials(materials);
                    break;
            }

            if (PlayerManager.Instance.SkinManager.IsSkinUnlocked(id))
            {
                _buyButton.style.display = DisplayStyle.None;
                _equipButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _buyButton.style.display = DisplayStyle.Flex;
                _equipButton.style.display = DisplayStyle.None;
            }
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


        private void SkinItemTemplateHandleCompleted(AsyncOperationHandle<VisualTreeAsset> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _skinItemTemplate = operation.Result;

                _isSkinItemTemplateLoaded = true;
            }
            else
            {
                _skinItemTemplateLoadingError = operation.OperationException;
            }

            _skinItemTemplateHandle.Completed -= SkinItemTemplateHandleCompleted;
        }

        public override void Dispose()
        {
            base.Dispose();

            Addressables.Release(_skinItemTemplateHandle);
        }
    }
}