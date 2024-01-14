using BallMaze.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.UI
{
    /// <summary>
    /// This class is the base class for all main UIs (main menu, levels, multiplayer, etc...).
    /// Main UIs are usually UIs that take up the whole screen.
    /// Only one main UI can be active at a time.
    /// </summary>
    public abstract class MainUIControl : UIControl
    {
        public bool isEnabled { get; protected set; }

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