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
        private Label _skinTextureLoading;
        private Label _skinTextureLoadingError;
        private Button _buyButton;
        private Button _equipButton;
        private Label _buyButtonPriceLabel;

        // GameObject on which the skins are applied to preview them
        private GameObject _previewBall;
        private MeshRenderer _previewBallMeshRenderer;

        // Dictionary to keep track of the callbacks for each skin card click
        private Dictionary<int, Action> _skinItemClicksCallbacks;

        // Cache the images for the skins, so that we don't have to load them each time we populate a category
        private Dictionary<int, Texture2D> _skinImages = new Dictionary<int, Texture2D>();
        // Dictionary of handles to keep track of the images that are being loaded
        private Dictionary<int, AsyncOperationHandle<Texture2D>> _skinImagesHandles = new Dictionary<int, AsyncOperationHandle<Texture2D>>();

        private SkinCategory _lastSelectedCategory = SkinCategory.Common;

        // Handle to load the skin item card template
        private AsyncOperationHandle<VisualTreeAsset> _skinItemTemplateHandle;
        private VisualTreeAsset _skinItemTemplate;
        private bool _isSkinItemTemplateLoaded = false;
        private Exception _skinItemTemplateLoadingError = null;

        private int _selectedSkinId = -1;

        private bool firstTimeOpened = true;

        private const int NUMBER_OF_ITEMS_PER_ROW = 4;

        private const int SKIN_ITEM_IMAGE_MARGIN_PERCENT = 15;
        private const int SKINS_ITEMS_CONTAINER_PADDING_PERCENT = 2;

        public SkinsView(VisualElement root) : base(root)
        {
            _skinItemClicksCallbacks = new Dictionary<int, Action>();

            _previewBall = GameObject.Find("PreviewBall");
            _previewBallMeshRenderer = _previewBall.GetComponent<MeshRenderer>();
        }


        protected override void SetVisualElements()
        {
            _skinsContainer = _root.Q<VisualElement>("skins__skins-horizontal-container");
            _skinsScrollViewContainer = _root.Q<VisualElement>("skins__skins-vertical-container");
            _skinTextureLoading = _root.Q<Label>("skins__skin-texture-loading-label");
            _skinTextureLoadingError = _root.Q<Label>("skins__skin-texture-loading-error-label");
            _buyButton = _root.Q<Button>("skins__buy-button");
            _equipButton = _root.Q<Button>("skins__equip-button");
            _buyButtonPriceLabel = _buyButton.Q<Label>("skins__skin-price");

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

            _equipButton.clicked += () =>
            {
                PlayerManager.Instance.SkinManager.EquipSkin(_selectedSkinId);
                UIManager.Instance.Show(UIViewType.MainMenu);
            };
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

            _previewBallMeshRenderer.enabled = true;

            // If the player is opening the skins UI for the first time, preview their equipped skin
            if (firstTimeOpened)
            {
                firstTimeOpened = false;

                PreviewSkin(PlayerManager.Instance.SkinManager.GetSkinFromId(PlayerManager.Instance.SkinManager.EquippedSkin));
            }
        }


        public override void Hide()
        {
            base.Hide();

            _previewBallMeshRenderer.enabled = false;
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

            OnGeometryChanged(null);
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

            // If the image is already cached, use it, otherwise load it from adressables
            if (_skinImages.ContainsKey(skin.id))
            {
                card.Q<VisualElement>("skins__skin-item-image").style.backgroundImage = _skinImages[skin.id];
            }
            else
            {
                // Remove the image from the card, so that it doesn't show the previous image while the new one is loading
                card.Q<VisualElement>("skins__skin-item-image").style.backgroundImage = null;

                AsyncOperationHandle<Texture2D> skinImageHandle = Addressables.LoadAssetAsync<Texture2D>($"Images/{skin.imagePath}");
                skinImageHandle.Completed += (operation) => UpdateCardImageFromHandle(cardBackground, operation);
                _skinImagesHandles.Add(skin.id, skinImageHandle);
            }

            cardBackground.CustomValue = skin.id;

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
        /// Update the image on the given card with the texture from the given handle once it has been loaded
        /// </summary>
        /// <param name="card"></param>
        /// <param name="operation"></param>
        private void UpdateCardImageFromHandle(ButtonWithValue card, AsyncOperationHandle<Texture2D> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _skinImages.Add(card.CustomValue, operation.Result);

                card.Q<VisualElement>("skins__skin-item-image").style.backgroundImage = operation.Result;
            }
            else
            {
                card.Q<VisualElement>("skins__skin-item-image").style.backgroundImage = null;
            }


            _skinImagesHandles[card.CustomValue].Completed -= (operation) => UpdateCardImageFromHandle(card, operation);
            _skinImagesHandles.Remove(card.CustomValue);

            Addressables.Release(operation);
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
            Action skinItemClickedHandler = () => { PreviewSkin(PlayerManager.Instance.SkinManager.GetSkinFromId(card.CustomValue)); };
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


        private void PreviewSkin(Skin skin)
        {
            _selectedSkinId = skin.id;

            _skinTextureLoading.style.display = DisplayStyle.None;
            _skinTextureLoadingError.style.display = DisplayStyle.None;

            _buyButtonPriceLabel.text = skin.price.ToString();

            // Update the buy button based on the player's coins
            if (PlayerManager.Instance.CoinsManager.HasEnoughCoins(skin.price))
            {
                _buyButton.RemoveFromClassList("button-disabled");
                _buyButton.AddToClassList("button-enabled");
            }
            else
            {
                _buyButton.RemoveFromClassList("button-enabled");
                _buyButton.AddToClassList("button-disabled");
            }

            // Show the buy or equip button based on whether the skin is unlocked
            if (PlayerManager.Instance.SkinManager.IsSkinUnlocked(skin.id))
            {
                _buyButton.style.display = DisplayStyle.None;
                _equipButton.style.display = DisplayStyle.Flex;
            }
            else
            {
                _buyButton.style.display = DisplayStyle.Flex;
                _equipButton.style.display = DisplayStyle.None;
            }

            // If the material is already cached, use it, otherwise load it from adressables
            if (SkinManager.IsMaterialCached(skin.id))
            {
                UpdatePreviewBallMaterial(true, SkinManager.GetMaterialFromCache(skin.id));
            }
            else
            {
                _previewBallMeshRenderer.enabled = false;
                _skinTextureLoading.style.display = DisplayStyle.Flex;

                SkinManager.LoadMaterialFromAddressables(skin.id, skin.materialPath, UpdatePreviewBallMaterial);
            }
        }


        /// <summary>
        /// Update the material on the preview ball with the given material
        /// </summary>
        /// <param name="material"></param>
        private void UpdatePreviewBallMaterial(bool materialLoaded, Material material)
        {
            if (materialLoaded)
            {
                _previewBallMeshRenderer.material = material;
                _previewBallMeshRenderer.enabled = true;

                _skinTextureLoading.style.display = DisplayStyle.None;
            }
            else
            {
                _skinTextureLoading.style.display = DisplayStyle.None;
                _skinTextureLoadingError.style.display = DisplayStyle.Flex;
            }
        }


        /// <summary>
        /// Update the size of the skin cards and their container.
        /// Called when the UI is resized or after populating the skins
        /// </summary>
        /// <param name="evt"></param>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // evt can be null when the method is called manually (eg: after populating the skins)
            if (evt != null && evt.oldRect.size == evt.newRect.size)
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

                item.style.width = itemSize;

                // Set the margin of each item
                item.style.marginTop = itemMargin;
                item.style.marginRight = itemMargin;
                item.style.marginBottom = itemMargin;
                item.style.marginLeft = itemMargin;

                // Resize the image to take into account the margin around it
                float imageSize = itemSize - 2 * (SKIN_ITEM_IMAGE_MARGIN_PERCENT / 100f * itemSize);

                // Set the size of each item
                VisualElement itemImage = item.Q<VisualElement>("skins__skin-item-image");
                itemImage.style.minWidth = imageSize;
                itemImage.style.maxWidth = imageSize;
                itemImage.style.minHeight = imageSize;
                itemImage.style.maxHeight = imageSize;

                itemImage.style.marginTop = Length.Percent(SKIN_ITEM_IMAGE_MARGIN_PERCENT);
                itemImage.style.marginRight = Length.Percent(SKIN_ITEM_IMAGE_MARGIN_PERCENT);
                itemImage.style.marginBottom = Length.Percent(SKIN_ITEM_IMAGE_MARGIN_PERCENT / 2);
                itemImage.style.marginLeft = Length.Percent(SKIN_ITEM_IMAGE_MARGIN_PERCENT);
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