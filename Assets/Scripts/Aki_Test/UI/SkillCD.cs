using System;
using Aki_Test.Managers;
using Aki_Test.Skills;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Aki_Test.UI
{
    public class SkillCD : MonoBehaviour
    {
        public SkillName skillName;
        private Skill _skill;
        private Image _skillMask;

        private void Start()
        {
            _skillMask = transform.Find("Mask").GetComponent<Image>();
            _skill = SkillManager.Instance.Skills.Find(skill =>
            {
                var s = Enum.GetName(typeof(SkillName), skillName);
                return skill.GetType().Name == s;
            });
            if (_skill.cooldown == 0)
            {
                _skillMask.enabled = false;
            }
            else
                _skill.OnSkillEngaged += OnSkillEngaged;
        }

        private void OnSkillEngaged()
        {
            float totalTime = _skill.cooldown;
            float duration = _skill.cooldown;
            DOTween.To(() => duration, x => _skillMask.fillAmount = x / totalTime, 0, duration);
        }
    }
}