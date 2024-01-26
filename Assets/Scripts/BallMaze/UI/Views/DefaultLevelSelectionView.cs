using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class DefaultLevelSelectionView : ScreenView
    {
        // Visual Elements
        private Button _backButton;
        private ScrollView _levelsSelectionContainerScrollView;
        private Dictionary<string, Action> _levelSelectionButtonsClickAction;


        public DefaultLevelSelectionView(VisualElement root) : base(root)
        {
            _levelSelectionButtonsClickAction = new Dictionary<string, Action>();
        }


        protected override void SetVisualElements()
        {
            _backButton = _root.Q<Button>("default-level-selection__back-button");
            _levelsSelectionContainerScrollView = _root.Q<ScrollView>("default-level-selection__levels-container-scroll-view");
        }


        protected override void RegisterButtonCallbacks()
        {
            // Go back to the previous screen view
            _backButton.clickable.clicked += () => { UIManager.Instance.Back(); };
        }


        /// <summary>
        /// Populate the default level selection view with the given levels selection
        /// </summary>
        /// <param name="levelsSelection"></param>
        public void PopulateLevelSelectionView(LevelsSelection levelsSelection)
        {
            // Start by emptying the level selection view
            EmptyLevelSelectionView();

            VisualTreeAsset levelSelectionTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/Templates/DefaultLevelSelectionTemplate.uxml");

            // Create a level selection template for each level
            foreach (LevelSelection levelSelection in levelsSelection.levels)
            {
                // Clone the template
                TemplateContainer levelSelectionTemplateClone = levelSelectionTemplate.CloneTree();
                _levelsSelectionContainerScrollView.Add(levelSelectionTemplateClone);

                // Define the click event handler for the button (to launch the level)
                Action levelSelectionClickedHandler = () => LevelSelectionClicked(levelSelection.id);
                _levelSelectionButtonsClickAction[levelSelection.id] = levelSelectionClickedHandler;

                Button levelSelectionCloneBackground = levelSelectionTemplateClone.Q<Button>("default-level-selection-template__container-button");
                levelSelectionTemplateClone.AddToClassList("level-selection-item");
                levelSelectionCloneBackground.clicked += levelSelectionClickedHandler;

                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__level-id").text = levelSelection.id;
                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__level-name").text = levelSelection.name;
                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__star-1").text = levelSelection.times[0].ToString();
                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__star-2").text = levelSelection.times[1].ToString();
                levelSelectionCloneBackground.Q<Label>("default-level-selection-template__star-3").text = levelSelection.times[2].ToString();
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
            
        }
    }
}