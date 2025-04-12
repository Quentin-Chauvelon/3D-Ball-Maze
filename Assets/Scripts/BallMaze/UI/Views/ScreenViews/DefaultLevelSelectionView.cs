using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
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
    public class DefaultLevelSelectionView : ScreenView
    {
        // Visual Elements
        private VisualElement _levelsZoneContainer; // Container of the pages + left/right arrow buttons
        private VisualElement _levelsContainer;
        private VisualElement _levelsContainerSize;
        private Button _leftPageArrowButton;
        private Button _rightPageArrowButton;
        private Dictionary<string, Action> _levelSelectionButtonsClickAction;
        private VisualElement _eventImage;
        private Label _eventName;

        // Adressable handle to load the default level selection template
        private AsyncOperationHandle<VisualTreeAsset> _defaultLevelSelectionTemplateHandle;
        private VisualTreeAsset _levelSelectionTemplate;

        private int _currentLevelsPage = -1;
        private LevelsSelection _levelsSelection;
        private bool _populateOnLoad = false;

        // The height of the level relative to its container
        private const float LEVEL_HEIGHT_PERCENTAGE = 0.35f;

        // Aspect ratio of the level (width of 1 and height of 1.23)
        private const float LEVEL_ASPECT_RATIO = 1.23f;

        private const int NUMBER_OF_ROWS_PER_PAGE = 2;
        private const float MIN_MARGIN_PERCENTAGE = 0.1f;


        public DefaultLevelSelectionView(VisualElement root) : base(root)
        {
            _levelSelectionButtonsClickAction = new Dictionary<string, Action>();

            _defaultLevelSelectionTemplateHandle = Addressables.LoadAssetAsync<VisualTreeAsset>("DefaultLevelSelectionTemplate");
            _defaultLevelSelectionTemplateHandle.Completed += DefaultLevelSelectionTemplateHandleCompleted;
        }


        protected override void SetVisualElements()
        {
            _levelsZoneContainer = _root.Q<VisualElement>("default-level-selection__levels-zone-container");
            _levelsContainer = _root.Q<VisualElement>("default-level-selection__levels-container");
            _levelsContainerSize = _root.Q<VisualElement>("default-level-selection__levels-container-size");
            _leftPageArrowButton = _root.Q<Button>("default-level-selection__left-page-arrow-button");
            _rightPageArrowButton = _root.Q<Button>("default-level-selection__right-page-arrow-button");
            _eventImage = _root.Q<VisualElement>("default-level-selection__event-image");
            _eventName = _root.Q<Label>("default-level-selection__event-name-label");

            PlayerEvents.DefaultLevelUnlocked += UnlockLevel;
            LevelEvents.DefaultLevelBestTimeUpdated += SetLevelTime;
        }


        protected override void RegisterButtonCallbacks()
        {
            _leftPageArrowButton.clicked += () => MoveToPage(_currentLevelsPage - 1);
            _rightPageArrowButton.clicked += () => MoveToPage(_currentLevelsPage + 1);
        }


        public async override void Show()
        {
            base.Show();

            // Wait one frame for the UI to be visible before populating the level selection
            // since we the size of the container
            await UniTask.Yield();

            if (_populateOnLoad)
            {
                PopulateLevelSelectionView(_levelsSelection);
                _populateOnLoad = false;
            }

            // If the level selection files were not checked in the last 5 minutes, update the level selection
            if (!GameManager.Instance.defaultLevelSelection.LastDefaultLevelFilesModifiedCheck.DateInTimeframe(300))
            {
                GameManager.Instance.defaultLevelSelection.LoadDefaultLevelSelection();
                return;
            }

            // If the level selection is not loaded, load it
            if (!IsDefaultLevelSelectionViewLoaded())
            {
                GameManager.Instance.defaultLevelSelection.LoadDefaultLevelSelection();
            }
        }


        /// <summary>
        /// Get the size of a level item based on the container's height
        /// </summary>
        /// <returns></returns>
        private Vector2 GetLevelItemSize()
        {
            // Calculate the height of the level based on the container's height
            float levelHeight = _levelsContainerSize.resolvedStyle.height * LEVEL_HEIGHT_PERCENTAGE;
            // Claculate the width of the level based on the aspect ratio
            float levelWidth = levelHeight / LEVEL_ASPECT_RATIO;

            return new Vector2(levelWidth, levelHeight);
        }


        private int GetMarginPerItem(float levelWidth, int itemsPerRow)
        {
            int totalMargin = (int)(_levelsContainerSize.resolvedStyle.width - (itemsPerRow * levelWidth));
            return totalMargin / (itemsPerRow + 1);
        }


        /// <summary>
        /// Get the number of levels that can fit in a single page.
        /// </summary>
        /// <returns></returns>
        private int GetNumberOfLevelsPerPage()
        {
            Vector2 levelSize = GetLevelItemSize();

            // Calculate the number of levels that can fit on a page based on the width of the container, the width of the level and the margin
            int itemsPerRow = Mathf.FloorToInt(_levelsContainerSize.resolvedStyle.width / levelSize.x);
            int marginPerItem = GetMarginPerItem(levelSize.x, itemsPerRow);

            // If the margin per item is less than a threshold of the level's width, reduce the number of items per row
            // Times NUMBER_OF_ROWS_PER_PAGE for multiple rows
            if (marginPerItem < levelSize.x * MIN_MARGIN_PERCENTAGE)
            {
                return (itemsPerRow - 1) * NUMBER_OF_ROWS_PER_PAGE;
            }
            else
            {
                return itemsPerRow * NUMBER_OF_ROWS_PER_PAGE;
            }
        }


        private void RegisterClicksForPage(int page)
        {
            if (_levelsContainer.childCount <= _currentLevelsPage || _currentLevelsPage == -1)
            {
                return;
            }

            // Subscribe to the click event of each level button
            foreach (VisualElement child in _levelsContainer.ElementAt(page).Children())
            {
                // If the level is not unlocked, don't register click events
                if (child.ClassListContains("level-selection-item-locked"))
                {
                    continue;
                }

                string id = child.Q<Label>("default-level-selection-template__level-id").text;
                Action levelSelectionClickedHandler = () => LevelSelectionClicked(id);

                _levelSelectionButtonsClickAction[id] = levelSelectionClickedHandler;
                child.Q<Button>("default-level-selection-template__container-button").clicked += levelSelectionClickedHandler;
            }
        }


        private void UnregisterClicksFromPage()
        {
            if (_levelsContainer.childCount <= _currentLevelsPage || _currentLevelsPage == -1)
            {
                return;
            }

            // Unsubscribe from the click event of each level button
            foreach (VisualElement child in _levelsContainer.ElementAt(_currentLevelsPage).Children())
            {
                if (_levelSelectionButtonsClickAction.ContainsKey(child.Q<Label>("default-level-selection-template__level-id").text))
                {
                    child.Q<Button>("default-level-selection-template__container-button").clicked -= _levelSelectionButtonsClickAction[child.Q<Label>("default-level-selection-template__level-id").text];
                }
            }

            _levelSelectionButtonsClickAction.Clear();
        }


        /// <summary>
        /// Populate the default level selection view with the given levels selection
        /// </summary>
        /// <param name="levelsSelection"></param>
        public void PopulateLevelSelectionView(LevelsSelection levelsSelection)
        {
            if (!isEnabled)
            {
                _populateOnLoad = true;
                _levelsSelection = levelsSelection;
                return;
            }

            // Start by emptying the level selection view
            EmptyLevelSelectionView();

#if UNITY_EDITOR && !LOAD_FROM_ADRESSABLES
            Debug.Log("Loading level selection template from UXML file");
            VisualTreeAsset levelSelectionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Templates/DefaultLevelSelectionTemplate.uxml");
#else
            Debug.Log("Loading level selection template from addressables?");
            VisualTreeAsset levelSelectionTemplate = _levelSelectionTemplate;
#endif

            UnregisterClicksFromPage();

            int numberOfLevelsPerPage = GetNumberOfLevelsPerPage();
            Vector2 itemSize = GetLevelItemSize();
            int marginPerItem = GetMarginPerItem(itemSize.x, numberOfLevelsPerPage / NUMBER_OF_ROWS_PER_PAGE);
            int page = 0;

            for (int i = 0; i < levelsSelection.levels.Length; i += numberOfLevelsPerPage)
            {
                // Create a new page
                VisualElement pageElement = new VisualElement();
                pageElement.AddToClassList("default-level-selection__page");
                pageElement.style.left = page * _levelsZoneContainer.resolvedStyle.width;
                // pageElement.style.translate = new StyleTranslate(new Translate(Length.Percent(page * 50), 0));
                _levelsContainer.Add(pageElement);

                for (int j = i; j < i + numberOfLevelsPerPage && j < levelsSelection.numberOfLevels; j++)
                {
                    LevelSelection levelSelection = levelsSelection.levels[j];

                    // Clone the template and add it to the page
                    TemplateContainer levelSelectionTemplateClone = levelSelectionTemplate.CloneTree();
                    pageElement.Add(levelSelectionTemplateClone);

                    levelSelectionTemplateClone.style.flexGrow = 0;
                    levelSelectionTemplateClone.style.width = itemSize.x;
                    levelSelectionTemplateClone.style.height = itemSize.y;

                    // Margin around element acts as alternative to flex gap
                    levelSelectionTemplateClone.style.marginTop = marginPerItem;
                    // levelSelectionTemplateClone.style.marginRight = marginPerItem;
                    levelSelectionTemplateClone.style.marginBottom = marginPerItem;
                    levelSelectionTemplateClone.style.marginLeft = marginPerItem;

                    if (PlayerManager.Instance.DefaultLevelsDataManager.IsLevelUnlocked(levelSelection.id))
                    {
                        levelSelectionTemplateClone.AddToClassList("level-selection-item-unlocked");

                        float bestTime = PlayerManager.Instance.DefaultLevelsDataManager.GetLevelBestTime(levelSelection.id);

                        // Set the level's best time
                        levelSelectionTemplateClone.Q<Label>("default-level-selection-template__time-label").text = $"{bestTime.ToString("00.00")}s";

                        // Set the level's stars
                        for (int k = 3; k > 3 - LevelManager.Instance.GetNumberOfStarsForLevel(levelSelection.id, bestTime); k--)
                        {
                            levelSelectionTemplateClone.Q<VisualElement>($"default-level-selection-template__star-{k}-image").AddToClassList("star-active");
                        }
                    }
                    // If the level is locked, set the current page to this level's page, so that the player doesn't
                    // have to scroll all the way to the end to get to the locked levels
                    else
                    {
                        levelSelectionTemplateClone.AddToClassList("level-selection-item-locked");

                        // Set the current page to the first page that is not fully unlocked
                        if (_currentLevelsPage == -1)
                        {
                            _currentLevelsPage = page;
                        }
                    }

                    levelSelectionTemplateClone.Q<Label>("default-level-selection-template__level-id").text = levelSelection.id;
                    levelSelectionTemplateClone.Q<Label>("default-level-selection-template__level-name").text = levelSelection.name;

                }

                page++;
            }

            // If the current levels page is still -1, set it to the last page
            if (_currentLevelsPage == -1)
            {
                _currentLevelsPage = Mathf.CeilToInt(levelsSelection.numberOfLevels / numberOfLevelsPerPage);
            }

            // Set the transition duration to 0 to disable the animation when moving to the correct page after loading the levels.
            // This prevents the player from seeing the transition when loading the levels as it should directly display the page
            _levelsContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(0f, TimeUnit.Millisecond) };
            MoveToPage(_currentLevelsPage);
            _levelsContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(500f, TimeUnit.Millisecond) };
        }


        /// <summary>
        /// Move the given page to the center of the screen
        /// </summary>
        /// <param name="page"></param>
        private void MoveToPage(int page)
        {
            if (page < 0 || page >= _levelsContainer.childCount)
            {
                return;
            }

            UnregisterClicksFromPage();

            _currentLevelsPage = page;

            _levelsContainer.style.left = Length.Percent(-page * 100 + 50);

            if (page == 0)
            {
                _leftPageArrowButton.style.visibility = Visibility.Hidden;
            }
            else
            {
                _leftPageArrowButton.style.visibility = Visibility.Visible;
            }

            if (page == _levelsContainer.childCount - 1)
            {
                _rightPageArrowButton.style.visibility = Visibility.Hidden;
            }
            else
            {
                _rightPageArrowButton.style.visibility = Visibility.Visible;
            }

            RegisterClicksForPage(_currentLevelsPage);
        }


        /// <summary>
        /// Empties the default level selection view by removing the content of the scroll view and unsubscribing all click events
        /// </summary>
        public void EmptyLevelSelectionView()
        {
            UnregisterClicksFromPage();

            foreach (VisualElement page in _levelsContainer.Children())
            {
                page.Clear();
            }

            _levelsContainer.Clear();
            _currentLevelsPage = -1;
        }


        /// <summary>
        /// Checks if the default level selection view contains the levels
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultLevelSelectionViewLoaded()
        {
            return _levelsContainer.childCount > 0;
        }


        /// <summary>
        /// Start the level with the given id
        /// </summary>
        /// <param name="id"></param>
        public void LevelSelectionClicked(string id)
        {
            Debug.Log("clicked!");

            // If the level is locked, return
            if (!PlayerManager.Instance.DefaultLevelsDataManager.IsLevelUnlocked(id))
            {
                return;
            }

            UIManager.Instance.Show(UIViewType.Playing);

            LevelEvents.LevelModeUpdated?.Invoke(LevelType.Default);

            LevelManager.Instance.LoadLevel(id);
        }


        /// <summary>
        /// Get the visual element of the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public VisualElement GetLevelFromId(string levelId)
        {
            foreach (VisualElement page in _levelsContainer.Children())
            {
                foreach (VisualElement child in page.Children())
                {
                    if (child.Q<Label>("default-level-selection-template__level-id").text == levelId)
                    {
                        return child;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Removes the lock icon for the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        public void UnlockLevel(string levelId)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.RemoveFromClassList("level-selection-item-locked");
                level.AddToClassList("level-selection-item-unlocked");
            }
        }


        /// <summary>
        /// Update the time and stars of the level with the given id
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="time"></param>
        public void SetLevelTime(string levelId, float time)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.Q<Label>("default-level-selection-template__time-label").text = $"{time.ToString("00.00")}s";
            }

            // Set the level's stars
            for (int i = 3; i > 3 - LevelManager.Instance.GetNumberOfStarsForLevel(levelId); i--)
            {
                level.Q<VisualElement>($"default-level-selection-template__star-{i}-image").AddToClassList("star-active");
            }
        }


        /// <summary>
        /// Update the UI based on the selected mode (classic or event)
        /// </summary>
        public void SetEventModeSelected(bool isEventModeSelected)
        {
            if (isEventModeSelected)
            {
                _eventImage.style.display = DisplayStyle.Flex;
                _eventName.style.display = DisplayStyle.Flex;
            }
            else
            {
                _eventImage.style.display = DisplayStyle.None;
                _eventName.style.display = DisplayStyle.None;
            }
        }


        /// <summary>
        /// Update the size of all levels so that they are always square which the height taking up 40% of the container (2 per column)
        /// </summary>
        /// <param name="evt"></param>
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.oldRect.size == evt.newRect.size)
            {
                return;
            }

            Vector2 levelSize = GetLevelItemSize();

            for (int i = 0; i < _levelsContainer.childCount; i++)
            {
                VisualElement page = _levelsContainer.ElementAt(i);
                page.style.left = _levelsZoneContainer.resolvedStyle.width;

                foreach (VisualElement child in page.Children())
                {
                    child.style.width = levelSize.x;
                    child.style.height = levelSize.y;
                }
            }
        }


        private void DefaultLevelSelectionTemplateHandleCompleted(AsyncOperationHandle<VisualTreeAsset> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                _levelSelectionTemplate = operation.Result;
            }
            else
            {
                ExceptionManager.ShowExceptionMessage(operation.OperationException, $"Couldn't load default level selection template: {operation.OperationException}");
            }

            _defaultLevelSelectionTemplateHandle.Completed -= DefaultLevelSelectionTemplateHandleCompleted;
        }

        public override void Dispose()
        {
            base.Dispose();

            Addressables.Release(_defaultLevelSelectionTemplateHandle);
        }
    }
}