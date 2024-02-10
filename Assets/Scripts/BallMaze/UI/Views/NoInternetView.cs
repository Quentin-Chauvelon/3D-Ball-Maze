using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BallMaze.UI
{
    public class NoInternetView : ModalView
    {
        public override bool isCloseable => false;

        private List<Action> callbacks;


        public NoInternetView(VisualElement root) : base(root)
        {
            callbacks = new List<Action>();
        }


        /// <summary>
        /// Displays the no internet UI based on the given internet availability.
        /// If a callback method is passed, add it the callback list
        /// so that it will be called when the player goes back online.
        /// </summary>
        /// <param name="internetAvailable">True if the player is online and the UI should be hidden, false otherwise</param>
        /// <param name="callback">The callback method to call when the player goes back online</param>
        public void DisplayNoInternetUI(bool internetAvailable, Action callback = null)
        {
            if (internetAvailable)
            {
                Hide();

                // Call all the callback methods
                foreach (Action c in callbacks)
                {
                    c();
                }
            }
            else
            {
                Show();

                // Add the callback method to the callback list
                if (callback != null && !callbacks.Contains(callback))
                {
                    callbacks.Add(callback);
                }
            }
        }


        public void Test()
        {

        }
    }
}