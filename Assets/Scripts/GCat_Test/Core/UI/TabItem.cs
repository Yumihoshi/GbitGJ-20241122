using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GCat_Test.Core.UI
{
    public class TabItem : MonoBehaviour, IPointerClickHandler
    {
        public Action<GameObject> onClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this.gameObject);
        }
    }
}