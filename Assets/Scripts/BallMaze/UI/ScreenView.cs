using BallMaze.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    /// <summary>
    /// This class is the base class for all screen views (main menu, levels, multiplayer, etc...).
    /// screen views are usually UIs that take up the whole screen.
    /// Only one screen view can be active at a time.
    /// </summary>
    public abstract class ScreenView : UIView
    {
        public override bool isModal => false;


        protected ScreenView(VisualElement root) : base(root)
        {

        }


        public override void Show()
        {
            base.Show();
        }


        public override void Hide()
        {
            base.Hide();
        }
    }
}