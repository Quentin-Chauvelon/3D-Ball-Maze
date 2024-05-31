using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Events;
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
        private ScrollView _levelsSelectionContainerScrollView;
        private Dictionary<string, Action> _levelSelectionButtonsClickAction;
        private VisualElement _eventImage;
        private Label _eventName;

        // Adressable handle to load the default level selection template
        private AsyncOperationHandle<VisualTreeAsset> _defaultLevelSelectionTemplateHandle;
        private VisualTreeAsset _levelSelectionTemplate;

        // The height of the level relative to its container
        private const float LEVEL_HEIGHT_PERCENTAGE = 0.35f;


        public DefaultLevelSelectionView(VisualElement root) : base(root)
        {
            _levelSelectionButtonsClickAction = new Dictionary<string, Action>();

            _defaultLevelSelectionTemplateHandle = Addressables.LoadAssetAsync<VisualTreeAsset>("DefaultLevelSelectionTemplate");
            _defaultLevelSelectionTemplateHandle.Completed += DefaultLevelSelectionTemplateHandleCompleted;
        }


        protected override void SetVisualElements()
        {
            _levelsSelectionContainerScrollView = _root.Q<ScrollView>("default-level-selection__levels-container-scroll-view");
            _eventImage = _root.Q<VisualElement>("default-level-selection__event-image");
            _eventName = _root.Q<Label>("default-level-selection__event-name-label");

            // Update the size of the children when the size of the container changes
            _levelsSelectionContainerScrollView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            PlayerEvents.DefaultLevelUnlocked += UnlockLevel;
            PlayerEvents.DefaultLevelBestTimeUpdated += SetLevelTime;
        }


        public override void Show()
        {
            base.Show();

            // If the level selection files were not checked in the last 5 minutes, update the level selection
            if (!GameManager.Instance.defaultLevelSelection.LastDefaultLevelFilesModifiedCheck.DateInTimeframe(60))
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
        /// Populate the default level selection view with the given levels selection
        /// </summary>
        /// <param name="levelsSelection"></param>
        public void PopulateLevelSelectionView(LevelsSelection levelsSelection)
        {
            // Save the ids of the default levels
            DefaultLevelsLevelManager.LoadDefaultLevelsIds(levelsSelection);

            // Start by emptying the level selection view
            EmptyLevelSelectionView();

#if UNITY_EDITOR
            VisualTreeAsset levelSelectionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Templates/DefaultLevelSelectionTemplate.uxml");
#else
            VisualTreeAsset levelSelectionTemplate = _levelSelectionTemplate;
#endif

            // Create a level selection template for each level
            foreach (LevelSelection levelSelection in levelsSelection.levels)
            {
                // Clone the template
                TemplateContainer levelSelectionTemplateClone = levelSelectionTemplate.CloneTree();
                _levelsSelectionContainerScrollView.Add(levelSelectionTemplateClone);

                // Define the click event handler for the button (to launch the level)
                Action levelSelectionClickedHandler = () => LevelSelectionClicked(levelSelection.id);
                _levelSelectionButtonsClickAction[levelSelection.id] = levelSelectionClickedHandler;

                levelSelectionTemplateClone.AddToClassList("level-selection-item");

                if (PlayerManager.Instance.LevelDataManager.IsDefaultLevelUnlocked(levelSelection.id))
                {
                    levelSelectionTemplateClone.AddToClassList("level-selection-item-unlocked");

                    // Set the level's best time
                    levelSelectionTemplateClone.Q<Label>("default-level-selection-template__time-label").text = PlayerManager.Instance.LevelDataManager.GetDefaultLevelBestTime(levelSelection.id).ToString("00.00");
                }
                else
                {
                    levelSelectionTemplateClone.AddToClassList("level-selection-item-locked");
                }

                Button levelSelectionCloneBackground = levelSelectionTemplateClone.Q<Button>("default-level-selection-template__container-button");
                levelSelectionCloneBackground.clicked += levelSelectionClickedHandler;

                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__level-id").text = levelSelection.id;
                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__level-name").text = levelSelection.name;

            }
        }


        /// <summary>
        /// Empties the default level selection view by removing the content of the scroll view and unsubscribing all click events
        /// </summary>
        public void EmptyLevelSelectionView()
        {
            // Unsubscribe from the click event of each level selection button
            foreach (VisualElement child in _levelsSelectionContainerScrollView.Children())
            {
                child.Q<Button>("default-level-selection-template__container-button").clicked -= _levelSelectionButtonsClickAction[child.Q<Label>("default-level-selection-template__level-id").text];
            }

            // Empty the action dictionary and the scroll view content
            _levelSelectionButtonsClickAction.Clear();
            _levelsSelectionContainerScrollView.Clear();
        }


        /// <summary>
        /// Checks if the default level selection view contains the levels
        /// </summary>
        /// <returns></returns>
        public bool IsDefaultLevelSelectionViewLoaded()
        {
            return _levelsSelectionContainerScrollView.childCount > 0;
        }


        /// <summary>
        /// Start the level with the given id
        /// </summary>
        /// <param name="id"></param>
        public void LevelSelectionClicked(string id)
        {
            // If the level is locked, return
            if (!PlayerManager.Instance.LevelDataManager.IsDefaultLevelUnlocked(id))
            {
                return;
            }

            UIManager.Instance.Show(UIViewType.Playing);

            LevelManager.SwitchMode(LevelType.Default);

            LevelManager.Instance.LoadLevel(id);
        }


        public VisualElement GetLevelFromId(string levelId)
        {
            foreach (VisualElement child in _levelsSelectionContainerScrollView.Children())
            {
                if (child.Q<Label>("default-level-selection-template__level-id").text == levelId)
                {
                    return child;
                }
            }

            return null;
        }


        public void UnlockLevel(string levelId)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.RemoveFromClassList("level-selection-item-locked");
                level.AddToClassList("level-selection-item-unlocked");
            }
        }


        public void SetLevelTime(string levelId, float time)
        {
            VisualElement level = GetLevelFromId(levelId);

            if (level != null)
            {
                level.Q<Label>("default-level-selection-template__time-label").text = time.ToString("00.00");
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

            float height = (float)(_levelsSelectionContainerScrollView.resolvedStyle.height * LEVEL_HEIGHT_PERCENTAGE);

            if (height > 1)
            {
                foreach (VisualElement child in _levelsSelectionContainerScrollView.Children())
                {
                    child.style.width = height;
                    child.style.height = height;
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