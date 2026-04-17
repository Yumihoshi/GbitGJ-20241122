using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GCat_Test.Core.StoryModule
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DreamLandPoint : MonoBehaviour
    {
        public StoryTrigger storyTrigger;

        private BoxCollider2D _pointCollider;
        private bool _flag;

        private void Awake()
        {
            _pointCollider = GetComponent<BoxCollider2D>();
            _pointCollider.isTrigger = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Keyboard.current.eKey.wasPressedThisFrame && !_flag)
                {
                    storyTrigger.TriggerStory();
                    _flag = true;
                }
            }
        }
    }
}