using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class BackgroundView : UIView
    {
        public override bool isModal => false;


        public BackgroundView(VisualElement root) : base(root)
        {

        }
    }
}