using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    /// <summary>
    /// This class is the base class for all modal views (settings, pause, skip, etc...).
    /// Multiple modal views can be active at a time and stack on top of each other.
    /// </summary>
    public abstract class ModalView : UIView
    {
        public override bool isModal => true;
        
        // Some modal views should not be closeable (e.g. no internet, download progress...).
        // This usually happens for modal views that are displayed automatically and are waiting for a specific event to happen.
        public abstract bool isCloseable { get; }

        protected ModalView(VisualElement root) : base(root)
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