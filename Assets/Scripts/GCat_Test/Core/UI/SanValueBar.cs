using System;
using UnityEngine;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class SanValueBar : MonoBehaviour
    {
        public Slider sanValueBar;

        private void Awake()
        {
            sanValueBar = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            sanValueBar.onValueChanged.AddListener(OnSanValueChange);
        }

        private void OnDisable()
        {
            sanValueBar.onValueChanged.RemoveListener(OnSanValueChange);
        }

        public void InitValue()
        {
            sanValueBar.value = 1;
        }
        
        public void SetValue(float value)
        {
            sanValueBar.value = value;
        }

        private void OnSanValueChange(float value)
        {
            
        }
    }
}