using BallMaze;
using BallMaze.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityExtensionMethods;
using Debug = UnityEngine.Debug;


namespace BallMaze
{
    /// <summary>
    /// The possible states of the game
    /// </summary>
    public enum GameState
    {
        MainMenu,
        LevelSelection,
        Playing
    }

    public class GameManager : MonoBehaviour
    {
        // Singleton pattern
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("GameManager is null!");
                }
                return _instance;
            }
        }

        private GameState _gameState;

        private DateTime _lastUnfocus;

        public DefaultLevelSelection defaultLevelSelection;


        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _gameState = GameState.MainMenu;
            _lastUnfocus = DateTime.UnixEpoch;

            defaultLevelSelection = GetComponent<DefaultLevelSelection>();
        }


        private void OnApplicationPause(bool pause)
        {
            // If the game is paused, save the time it was paused at, this will allow to know how much time the game was paused for
            if (pause)
            {
                _lastUnfocus = DateTime.UtcNow;
            }
            else
            {
                // If the game has been paused for less than 1 hour and the player is in the level selection screen, bring him back to the main menu
                // This way, when the player clicks on the level selection button, levels update and UI populate will be handled by the LevelSelection script
                if (_lastUnfocus.DateInTimeframe(3600))
                {
                    UIManager.Instance.Show(UIViews.MainMenu);
                }
                // If the levels have not been populated yet, load them
                else if (!UIManager.Instance.IsDefaultLevelSelectionViewLoaded())
                {
                    defaultLevelSelection.LoadDefaultLevelSelection();
                }
            }
        }
    }
}