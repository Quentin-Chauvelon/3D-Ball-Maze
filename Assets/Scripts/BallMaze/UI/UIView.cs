using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public abstract class UIView : IDisposable
    {
        //public abstract string name { get; }
        public bool isEnabled { get; protected set; }

        protected VisualElement _root;

        public abstract bool isModal { get; }

        protected bool _hideOnAwake = true;


        /// <summary>
        /// Initialize an instance of the UIView class.
        /// </summary>
        /// <param name="root">The root of UXML hierarchy</param>
        public UIView(VisualElement root)
        {
            _root = root;
            Initialize();
        }


        /// <summary>
        /// Initializes the UI element.
        /// </summary>
        protected virtual void Initialize()
        {
            if (_hideOnAwake)
            {
                Hide();
            }

            SetVisualElements();
            RegisterButtonCallbacks();
        }


        /// <summary>
        /// Sets the visual elements of the UI element. Can be overriden by child classes.
        /// </summary>
        protected virtual void SetVisualElements()
        {
            
        }


        /// <summary>
        /// Registers the callbacks for the buttons of the UI element. Can be overriden by child classes.
        /// </summary>
        protected virtual void RegisterButtonCallbacks()
        {
            
        }


        /// <summary>
        /// Unregisters the callbacks for the buttons of the UI element. Can be overriden by child classes.
        /// </summary>
        protected virtual void UnregisterButtonCallbacks()
        {
            
        }


        /// <summary>
        /// Shows the UI element. Can be overriden by child classes.
        /// </summary>
        public virtual void Show()
        {
            isEnabled = true;

            _root.style.display = DisplayStyle.Flex;
        }


        /// <summary>
        /// Hides the UI element. Can be overriden by child classes.
        /// </summary>
        public virtual void Hide()
        {
            isEnabled = false;
            
            _root.style.display = DisplayStyle.None;
        }


        /// <summary>
        /// Disposes the UI element. Can be overriden by child classes.
        /// </summary>
        public virtual void Dispose()
        {
            
        }
    }
}