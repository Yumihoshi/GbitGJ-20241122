using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GCat_Test.Core.UI
{
    public class PlayerStateDisplay : MonoBehaviour
    {
        public Sprite[] erosionImages;
        public Image erosionDisplay;
        public float fadeSpeed = 15f;

        private Coroutine _erosionAnimEffectCoroutine;

        private void Awake()
        {
            erosionDisplay = GetComponent<Image>();
        }

        public void ChangePlayerHpDisplayState(int degree)
        {
            Sprite sprite = null;
            switch (degree)
            {
                case 1:
                    sprite = erosionImages[0];
                    break;
                case 2:
                    sprite = erosionImages[1];
                    break;
                case 3:
                    sprite = erosionImages[2];
                    break;
            }

            erosionDisplay.gameObject.SetActive(sprite != null);
            erosionDisplay.sprite = sprite;
            if (_erosionAnimEffectCoroutine != null)
                StopCoroutine(_erosionAnimEffectCoroutine);
            if (degree == 0)
                return;
            gameObject.SetActive(true);
            _erosionAnimEffectCoroutine = StartCoroutine(ErosionAnimEffect(fadeSpeed));
        }

        private IEnumerator ErosionAnimEffect(float speed)
        {
            bool flag = false;
            Vector2 range = new Vector2(0.5f, 1f);
            // float vel = 0;
            while (erosionDisplay.sprite != null)
            {
                var c = erosionDisplay.color;
                if (flag)
                {
                    c.a = Mathf.MoveTowards(c.a, range.y, speed * Time.deltaTime);
                    // c.a = Mathf.SmoothDamp(c.a, range.y, ref vel, 0.01f, speed);
                    if (Mathf.Approximately(c.a, range.y))
                        flag = false;
                }
                else
                {
                    c.a = Mathf.MoveTowards(c.a, range.x, speed * Time.deltaTime);
                    // c.a = Mathf.SmoothDamp(c.a, range.x, ref vel, 0.01f, speed);
                    if (Mathf.Approximately(c.a, range.x))
                        flag = true;
                }

                erosionDisplay.color = c;   
                yield return null;
            }
        }

        
    }
}