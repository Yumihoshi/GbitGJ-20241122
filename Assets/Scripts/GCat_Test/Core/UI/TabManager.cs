using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class TabManager : MonoBehaviour
    {
        [System.Serializable]
        public struct TabLink
        {
            public GameObject tab;
            public GameObject page;
        }

        public List<TabLink> tabLinkList;
        public Color normalColor;
        public Color selectedColor;
        [ShowInInspector, ReadOnly] private GameObject _currentSelectedTab;
        [ShowInInspector, ReadOnly] private GameObject _currentDisplayPage;
        
        private void Start()
        {
            foreach (var tabLink in tabLinkList)
            {
                if (tabLink.tab.TryGetComponent(out TabItem tabItem))
                {
                    tabItem.onClick += ClickTab;
                }
            }
        }

        public void InitTab()
        {
            SwitchPage(tabLinkList[0].tab, tabLinkList[0].page);
        }

        private void ClickTab(GameObject selectedObject)
        {
            if (selectedObject != null)
            {
                var tabLink = tabLinkList.Find(x => x.tab == selectedObject);
                if (tabLink.tab != null)
                {
                    
                    SwitchPage(tabLink.tab, tabLink.page);
                }
            }
        }

        private void SwitchPage(GameObject tab, GameObject newPage)
        {
            if (_currentSelectedTab)
                _currentSelectedTab.GetComponent<Image>().color = normalColor;

            _currentSelectedTab = tab;
            _currentSelectedTab.GetComponent<Image>().color = selectedColor;
            
            if (_currentDisplayPage)
                _currentDisplayPage.SetActive(false);
            _currentDisplayPage = newPage;
            _currentDisplayPage.SetActive(true);
        }
    }
}