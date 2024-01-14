using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.UI
{
    /// <summary>
    /// This class is the base class for all popups (settings, pause, skip, etc...).
    /// Multiple popups can be active at a time and stack on top of each other.
    /// </summary>
    public abstract class PopUpUIControl : UIControl
    {
        public override void Show()
        {
            base.Show();
        }


        public override void Hide()
        {
            base.Hide();
        }


        /// <summary>
        /// Called when the first popup is opened.
        /// Adds an invisible button behind the popup to detect clicks outside of the popup (to close the more easily).
        /// The same button is used for all popups and the last popup to be opened is the one that will be closed when the button is clicked.
        /// </summary>
        public static void PopUpOpened()
        {

        }


        /// <summary>
        /// Called when the last popup is closed.
        /// Removes the invisible button behind the popup.
        /// </summary>
        public static void NoPopUpsOpened()
        {

        }
    }
}