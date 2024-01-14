using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.UI
{
    public abstract class UIControl : MonoBehaviour
    {
        public abstract string name { get; }
        public bool isEnabled { get; protected set; }


        private void Awake()
        {
            UIManager.RegisterUIControl(this);
        }


        private void OnDestroy()
        {
            UIManager.UnregisterUIControl(this);
        }


        /// <summary>
        /// Shows the UI element.
        /// </summary>
        public virtual void Show()
        {
            isEnabled = true;
            gameObject.SetActive(true);
        }


        /// <summary>
        /// Hides the UI element.
        /// </summary>
        public virtual void Hide()
        {
            isEnabled = false;
            gameObject.SetActive(false);
        }
    }
}