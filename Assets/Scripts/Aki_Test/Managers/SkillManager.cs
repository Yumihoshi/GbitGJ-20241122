using System;
using System.Collections.Generic;
using Aki_Test.Common;
using Aki_Test.Skills;
using UnityEngine;

public enum SkillName
{
    Detect,
    Eliminate,
}
namespace Aki_Test.Managers
{
    public class SkillManager : Singleton<SkillManager>
    {
        [HideInInspector] public List<Skill> Skills = new List<Skill>();

        /// <summary>
        /// 按技能类型添加到对应list
        /// </summary>
        /// <param name="skill"></param>
        public void AddSkill(Skill skill)
        {
            Skills.Add(skill);
        }

        public void RemoveSkill(Skill skill)
        {
            Skills.Remove(skill);
        }

        private void OnDisable()
        {
            try
            {
                Skills.ForEach(skill =>
                {
                    if (skill == null) return;
                    skill.enabled = false;
                });
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}