// 玩家基本控制

using System;
using System.Collections.Generic;
using Aki_Test.Character_Stats;
using Aki_Test.Managers;
using Aki_Test.Skills;
using GCat_Test.Core.Manager;
using Unity.Mathematics;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Aki_Test.Player
{
    public class PlayerController : MonoBehaviour
    {
        // 性能优化：把要计算的max_height提前算出来
        private const float SQRT_MAX_HEIGHT = 0f;
        private float _sqrtGravity;
        private float _sqrtMaxHeight;
    
        public PlayerInputControl playerInputControl;
        private Rigidbody2D _rb;
        private Animator _animator;
        private Vector2 _movementDirection;
        private int _jumpCount;
        private Skill _skill;
        private float _footstepCountDown;
        
        public bool isGrounded;
        private bool _isJumping;                    // 判断是否在上升过程
        [NonSerialized] public bool isBraking;      // 判断是否正在刹车
        [NonSerialized] public bool canMove;        // 是否可以移动
        [NonSerialized] public bool canJump;        // 是否可以跳跃
        public bool doubleJump = false;                     // 是否可以二段跳
        private bool _isTurning;                    // 判断是否正在转向
        private bool _isMoving;                     // 是否正在主动移动
        private bool _isDead;                       // 判断死亡

        public CharacterStats characterStats; // 角色基本数据

        public UnityAction onDie;
        
        [Header("Movement")]
        public float acceleration;  // 开始移动时的力大小
        public float deceleration;  // 停止移动以及转向时的力大小
        public float maxHeight;     // Bounce时的最大跳跃高度
        public float movementSpeed;
        public float jumpForce;
        public Vector2 bounceForce;
    
        [Header("GroundCheck")]
        public Vector2 groundCheckOffset;
        [SerializeField] private Vector2 groundCheckSize;
    
        [Header("Physics Materials")]
        public PhysicsMaterial2D groundMaterial;
        public PhysicsMaterial2D wallMaterial;

        private void Awake()
        {
            playerInputControl = new PlayerInputControl();
            _rb = GetComponent<Rigidbody2D>();
            characterStats = GetComponent<CharacterStats>();
            _animator = GetComponent<Animator>();
            _sqrtMaxHeight = math.sqrt(maxHeight);
            _sqrtGravity = math.sqrt(math.abs(Physics2D.gravity.y * _rb.gravityScale));

            canMove = true;
            _isDead = false;
        
            playerInputControl.GamePlay.Move.started += MoveOnstarted;
            playerInputControl.GamePlay.Move.performed += MoveOnperformed;
            playerInputControl.GamePlay.Jump.started += OnJump;
            playerInputControl.GamePlay.MainSkill.started += MainSkillOnstarted;
            playerInputControl.GamePlay.MainSkill.performed += MainSkillOnperformed;
            playerInputControl.GamePlay.ChildSkill.started += ChildSkillOnstarted;
            playerInputControl.GamePlay.ChildSkill.performed += ChildSkillOnperformed;
            playerInputControl.GamePlay.DoubleJumpUnlock.started += context =>
            {
                doubleJump = !doubleJump;
                Debug.Log("二段跳权限：" + doubleJump);
            };
            
            characterStats.OnRespawn += Respawn;
        
            // 动态挂载技能代码
            AddSkill();
        }
    
        private void Start()
        {
        }

        private void OnEnable()
        {
            playerInputControl.Enable();
        }

        private void Update()
        {
            _movementDirection = playerInputControl.GamePlay.Move.ReadValue<Vector2>();
            if (characterStats.characterData.currentHealth <= 0)
            {
                Dead();
            }
            IsJumping();
            GroundedCheck();
            CheckMaterial();
            SwitchAnimation();
        }


        private void FixedUpdate()
        {
            if (canMove)
            {
                if (isBraking || !_isMoving)
                    Braking(); // 刹车
                else if (_isTurning)
                    Braking(); // 刹车
                else
                    Move();
            }
        }


        private void OnDisable()
        {
            playerInputControl.Disable();
        }


        private void SwitchAnimation()
        {
            _animator.SetFloat("XSpeed", Mathf.Abs(_rb.velocity.x));
            _animator.SetFloat("YSpeed", Mathf.Abs(_rb.velocity.y));
            _animator.SetBool("Die", _isDead);
            _animator.SetBool("Jump", _isJumping);
            _animator.SetBool("Grounded", isGrounded);
            _animator.SetBool("Moving", _isMoving);
        }
        private void Dead()
        {
            if (_isDead) return;
            // todo - 死亡
            characterStats.characterData.currentHealth = 0;
            _isDead = true;
            onDie?.Invoke();
            AudioManager.Ins.PlaySfx("die");
            playerInputControl.GamePlay.Disable();
            // todo - 游戏结束，UI更新
        }

        private void Respawn()
        {
            // todo - 复活逻辑
            playerInputControl.GamePlay.Enable();
            _isDead = false;
        }
        
        private void IsJumping()
        {
            if (_rb.velocity.y > 0)
            {
                _isJumping = true;
            }
            else
            {
                _isJumping = false;
            }
        }

        private void Move()
        {
            // todo - 优化手感
            ChangeFaceDir(_movementDirection.x);

            // _rb.velocity = new Vector2(_movementDirection.x * movementSpeed * Time.deltaTime, _rb.velocity.y);
            // 加速直到达到最大速度
            if (math.abs(_rb.velocity.x) < movementSpeed)
                _rb.AddForce(_movementDirection * acceleration, ForceMode2D.Force);
            else
            {
                _rb.velocity = new Vector2(movementSpeed * _movementDirection.x, _rb.velocity.y);
            }
            
            if (_isMoving && isGrounded)
            {
                while (_footstepCountDown <= 0)
                {
                    AudioManager.Ins.PlaySfx("footstep");
                    _footstepCountDown = 0.5f;
                }
                _footstepCountDown -= Time.deltaTime;
            }
        }

        private void ChangeFaceDir(float movDir)
        {
            int faceDir = (int)transform.localScale.x;
            var sign = (int)math.sign(movDir);
            if (faceDir == sign || sign == 0)
            {
                _isTurning = false;
            }
            else
            {
                _isTurning = true;
                // 人物翻转
                faceDir = sign;
                transform.localScale = new Vector3(faceDir, 1, 1);
            }
        }

        /// <summary>
        /// 对角色用deceleration的加速度执行刹车
        /// </summary>
        /// <param name="brakeDir">减速加速度的方向</param>
        public void Braking()
        {
            if (math.abs(_rb.velocity.x) <= 1)
            {
                _rb.velocity = new Vector2(0, _rb.velocity.y);
                _isTurning = false;
                isBraking = false;
            }
            else
            {
                //_rb.AddForce(new Vector2(brakeDir * deceleration, 0), ForceMode2D.Force);
                _rb.AddForce(new Vector2(-math.sign(_rb.velocity.x) * deceleration, 0) , ForceMode2D.Force);
            }
            
        }

        /// <summary>
        /// 跳跃到指定位置
        /// </summary>
        /// <param name="bounceTarget">目标位置</param>
        public void Bounce(Vector2 bounceTarget, bool bounceToNextPoint = true)
        {
            float deltaX = bounceTarget.x;
            float deltaY = bounceTarget.y;
            if (bounceToNextPoint)
            {
                // 计算初速度
                Vector2 initialVelocity = CalculateInitialSpeed(deltaX, deltaY);

                // 应用初速度
                _rb.velocity = Vector2.zero;
                _rb.velocity = initialVelocity;
                ChangeFaceDir(initialVelocity.x);
            }
            else
            {
                _rb.velocity = Vector2.zero;
                _rb.AddForce(bounceForce, ForceMode2D.Impulse);
            }
        }
    
        private Vector2 CalculateInitialSpeed(float deltaX, float deltaY)
        {
            int deltaXSign = (int)math.sign(deltaX);
            int deltaYSign = (int)math.sign(deltaY);
            var absX= Mathf.Abs(deltaX);
            var absY = Mathf.Abs(deltaY);
            // 计算速度，使得最大高度为给定值
            var gravity = math.abs(Physics2D.gravity.y * _rb.gravityScale);
            float timeToReachMaxHeight;
            float horizontalSpeed;
            float verticalSpeed;
            if (deltaYSign < 0)
            {
                timeToReachMaxHeight = math.sqrt(2 * maxHeight / gravity);
                horizontalSpeed =
                    absX / (math.SQRT2 / _sqrtGravity * (math.sqrt(maxHeight + absY) + _sqrtMaxHeight)) * deltaXSign;
                verticalSpeed = -gravity * timeToReachMaxHeight * deltaYSign;
            }
            else
            {
                timeToReachMaxHeight = math.sqrt(2 * absY / gravity);
                horizontalSpeed = absX / timeToReachMaxHeight * deltaXSign;
                verticalSpeed = gravity * timeToReachMaxHeight * deltaYSign;
            }

            Vector2 initialVelocity = new Vector2(horizontalSpeed, verticalSpeed);
            return initialVelocity;
        }

        private void GroundedCheck()
        {
            if (Physics2D.OverlapBox((Vector2)transform.position + groundCheckOffset, groundCheckSize, 0, LayerMask.GetMask("Ground")))
                isGrounded = true;
            else
                isGrounded = false;
        }

        // 根据情况选择合适的材质
        private void CheckMaterial()
        {
            _rb.sharedMaterial = isGrounded? groundMaterial : wallMaterial;
        }
    
        private void AddSkill()
        {
            // todo - 根据不同场景挂载不同的子技能代码，这里直接挂载Eliminate技能
            // gameObject.AddComponent<Eliminate>();
        
            string skillName = Enum.GetName(typeof(SkillName), SceneController.Instance.childSkillName);
            string namespaceName = "Aki_Test.Skills";
            Type skillType = Type.GetType(namespaceName + "." + skillName);
            if (skillType != null)
                gameObject.AddComponent(skillType);
            else
            {
                Debug.LogError($"Skill {skillName} not found");
            }
        }

        #region Events
        private void MoveOnstarted(InputAction.CallbackContext obj)
        {
            isBraking = false;
            _isMoving = true;
        }
    
        private void MoveOnperformed(InputAction.CallbackContext obj)
        {
            isBraking = true;
            _isMoving = false;
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
            if (!canJump) return;
            _isMoving = true;
            if (isGrounded)
                _jumpCount = 1;
            else
                _jumpCount++;
        
            // 在地面上或者在空中的上升阶段才能（二段）跳
            if (isGrounded || (_jumpCount <= (doubleJump ? 2 : 1) && _isJumping)) // _isJumping = true;
                _rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }

        private void ChildSkillOnstarted(InputAction.CallbackContext obj)
        {
            foreach (var i in SkillManager.Instance.Skills)
            {
                if (!i.isMainSkill)
                {
                    _skill = i;
                    break;
                }
            }
            _skill.Ready();
        }

        private void MainSkillOnstarted(InputAction.CallbackContext obj)
        {
            foreach (var i in SkillManager.Instance.Skills)
            {
                if (i.isMainSkill)
                {
                    _skill = i;
                    break;
                }
            }
            _skill.Ready();
        }
    
        private void ChildSkillOnperformed(InputAction.CallbackContext obj)
        {
            if (_skill != null)
                _skill.Engage();
        }

        private void MainSkillOnperformed(InputAction.CallbackContext obj)
        {        
            if (_skill != null)
                _skill.Engage();
        }

        #endregion
    
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckSize);
        }

        private void OnValidate()
        {
            _sqrtMaxHeight = Mathf.Sqrt(maxHeight);
#if !UNITY_EDITOR
        _sqrtGravity = math.sqrt(math.abs(Physics2D.gravity.y * _rb.gravityScale));
#endif
        }
    }
}
