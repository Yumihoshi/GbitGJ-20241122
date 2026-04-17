using System;
using GCat_Test.Core.Manager;
using UnityEngine;

namespace GCat_Test.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class OptionTips : MonoBehaviour
    {
        [Multiline(2)]
        public string optionTips;
        private BoxCollider2D _boxCollider2D;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                GameLevelManager.Ins.OnSetOptionTips(optionTips);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                GameLevelManager.Ins.OnSetOptionTips("");
        }
    }
}