using System;
using System.Collections.Generic;
using Aki_Test.Player;
using GCFramework.Singleton;

namespace GCat_Test.Core.StoryModule
{
    public class StoryManager : SingletonMono<StoryManager>
    {
        public List<StoryTrigger> storyTriggerCollections = new List<StoryTrigger>();
        
        private readonly Dictionary<string, Action> _storyEvents = new Dictionary<string, Action>();

        private void OnEnable()
        {
            _storyEvents.Add("DisablePlayerMove", DisablePlayerMoveEvent);
            _storyEvents.Add("EnablePlayerMove", EnablePlayerMoveEvent);
            _storyEvents.Add("DisablePlayerJump", DisablePlayerJumpEvent);
            _storyEvents.Add("EnablePlayerJump", EnablePlayerJumpEvent);
        }

        public void FireEvent(string eventName)
        {
            if (_storyEvents.TryGetValue(eventName, out var action))
            {
                action?.Invoke();
            }
        }

        public void OnBeginStory()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canJump = false;
        }

        public void OnEndStory()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canJump = true;
        }

        #region Events

        private void DisablePlayerMoveEvent()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canMove = false;
        }

        private void EnablePlayerMoveEvent()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canMove = true;
        }

        private void DisablePlayerJumpEvent()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canJump = false;
        }
        
        private void EnablePlayerJumpEvent()
        {
            var player = FindObjectOfType<PlayerController>();
            player.canJump = true;
        }

        #endregion
    }
}