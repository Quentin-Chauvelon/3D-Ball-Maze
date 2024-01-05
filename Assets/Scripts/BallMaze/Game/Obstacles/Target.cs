using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BallMaze.Obstacles
{
    public class Target : MonoBehaviour, ITrigerrable
    {
        public GameObject targetParent;
        public Action triggerAction { get; set; }

        public void TriggerEntered()
        {
            triggerAction?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Ball"))
            {
                TriggerEntered();
            }
        }
    }
}