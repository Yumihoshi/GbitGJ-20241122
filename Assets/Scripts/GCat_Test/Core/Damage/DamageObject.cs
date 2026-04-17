using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GCat_Test.Core.Damage
{
    [System.Serializable]
    public class HitProperties
    {
        public string[] hitTags;
        public LayerMask hitLayer;
    }
    
    public class DamageObject : MonoBehaviour
    {
        public HitProperties hitProperties;
        public UnityEvent<Collider2D> onDamageHit;

        private bool CanHit(Collider2D hitCollider)
        {
            return hitProperties.hitTags.Contains(hitCollider.tag)
                   && (hitProperties.hitLayer & (1 << hitCollider.gameObject.layer)) != 0;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CanHit(other))
            {
                Debug.Log($"对{other.name}造成伤害！");
                onDamageHit?.Invoke(other);
            }
        }
    }
}