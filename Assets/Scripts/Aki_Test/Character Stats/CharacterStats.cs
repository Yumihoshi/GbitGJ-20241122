// 角色血量等基础数据操作

using Aki_Test.Character_Stats.ScriptableObject;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Aki_Test.Character_Stats
{
    public class CharacterStats : MonoBehaviour
    {
        public CharacterData_SO characterData;
        /// <summary>
        /// 形参：当前血量（int），最大血量（int）
        /// </summary>
        public UnityAction<int, int> OnHealthChanged;
        public UnityAction OnRespawn;
        #region HealthLossTimer
        private float _healthLossTimer;
        #endregion
        private void Start()
        {
            InitPlayer();
        }

        private void InitPlayer()
        {
            characterData.currentHealth = characterData.maxHealth;
            characterData.healthLossNum = characterData.defaultHealthLossNum;
            characterData.healthLossTime = characterData.defaultHealthLossTime;
            _healthLossTimer = characterData.healthLossTime;
        }

        private void Update()
        {
            if (gameObject.CompareTag("Player"))
            {
                HealthLoss();
            }
        }
    
        private void HealthLoss()
        {
            _healthLossTimer -= Time.deltaTime;
            if (_healthLossTimer <= 0)
            {
                characterData.currentHealth -= characterData.healthLossNum;
                _healthLossTimer = characterData.healthLossTime;
                OnHealthChanged?.Invoke(characterData.currentHealth, characterData.maxHealth);
            }
        }

        public void RestoreFullHealth()
        {
            characterData.currentHealth = characterData.maxHealth;
            ResetHealthLossTime();
            OnHealthChanged?.Invoke(characterData.currentHealth, characterData.maxHealth);
        }

        public void TakeDamage(int damage)
        {
            characterData.currentHealth = math.max(characterData.currentHealth - damage, 0);
            OnHealthChanged?.Invoke(characterData.currentHealth, characterData.maxHealth);

            // SetHealthLossTime(2);
        }

        /// <summary>
        /// 重置血量流失速度为默认值，重置计时器
        /// </summary>
        public void ResetHealthLossTime()
        {
            characterData.healthLossTime = characterData.defaultHealthLossTime;
            _healthLossTimer = characterData.healthLossTime;
        }
        /// <summary>
        /// 设置血量流失速度
        /// </summary>
        /// <param name="healthLossTime"></param>
        public void SetHealthLossTime(float healthLossTime)
        {
            _healthLossTimer = healthLossTime;
            characterData.healthLossTime = healthLossTime;
        }

        /// <summary>
        /// 玩家重生并初始化数据
        /// </summary>
        public void RespawnPlayer()
        {
            InitPlayer();
            OnRespawn?.Invoke();
            OnHealthChanged?.Invoke(characterData.maxHealth, characterData.maxHealth);
            Debug.Log("玩家重生");
        }
    }
}
