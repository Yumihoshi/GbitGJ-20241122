using Aki_Test.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Aki_Test.Skills
{
    public abstract class Skill : MonoBehaviour
    {
        public enum SkillShape // 设置技能范围的形状
        {
            Target, // 指定目标型
            Circle, // 圆形
            Sector  // 扇形
        }
        public SkillShape skillShape;
        public string skillName;
        public float cooldown;  // 0时为瞬发型技能
        public bool isReady = false; // 根据CD时间判断是否就绪
        public float skillRadius; // 技能施放范围（skillShape == Target时无效）
        [HideInInspector] public bool isMainSkill;
        
        public UnityAction OnSkillEngaged;
    
        [SerializeField] protected GameObject _player;
        private float _cooldownTimer;
        private GameObject _region;

        protected void Awake()
        {
            SkillManager.Instance.AddSkill(this);
            _cooldownTimer = 0;
            _player = gameObject;
        }

        private void OnDisable()
        {
            if (SkillManager.Instance != null) 
                SkillManager.Instance.RemoveSkill(this);
        }

        protected virtual void Update()
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0)
            {
                isReady = true;
            }
            else
            {
                isReady = false;
            }
        }

        /// <summary>
        /// 当技能准备好的时候，按下释放键不放，显示范围
        /// </summary>
        public virtual void Ready()
        {
            switch (skillShape)
            {
                case SkillShape.Circle:
                    _region = _player.transform.Find("SkillRegionCircle").gameObject;
                    _region.SetActive(true);
                    _region.transform.localScale = new Vector3(skillRadius * 2, skillRadius * 2, skillRadius * 2);
                    break;
            }
        }
    
        public virtual void Engage()
        {
            // 若技能还没准备好，不释放技能，取消技能范围显示
            // todo - 也可以在没有准备好的时候直接不显示范围，根据需求调整
            if (!isReady)
            {
                Debug.Log(skillName + " Not Engaged(not ready)");
                _region?.SetActive(false);
                return;
            } 
            ResetCooldown();
            if (_region != null) _region.SetActive(false);
            Debug.unityLogger.Log(skillName + " Engaged");
            OnSkillEngaged?.Invoke();
        }
    
        protected void ResetCooldown()
        {
            _cooldownTimer = cooldown;
        }
    }
}
