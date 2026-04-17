using System;
using System.Collections;
using GCat_Test.Core.StoryModule;
using UnityEngine;

namespace GCat_Test.Core.Levels
{
    public class Level1Entrance : MonoBehaviour
    {
        public StoryTrigger storyTrigger;

        private void Start()
        {
            StartCoroutine(nameof(DelayTrigger));
        }

        private IEnumerator DelayTrigger()
        {
            yield return new WaitForSeconds(1f);
            storyTrigger.TriggerStory();
        }
    }
}