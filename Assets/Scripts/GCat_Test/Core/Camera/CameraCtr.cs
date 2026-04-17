using System;
using System.Collections;
using Cinemachine;
using GCat_Test.Core.Events;
using GCFramework.Runtime.MessageCenter;
using GCFramework.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GCat_Test.Core.Camera
{
    /// <summary>
    /// 相机控制类
    /// </summary>
    public class CameraCtr : SingletonMono<CameraCtr>
    {
        public Transform followTarget;
        public float lookAtSmoothTime = 10f;
        public Vector2 lookAtOffsetDist = new Vector2(3, 3);
        public bool enableLookAt;
        public bool inMenu;

        [SerializeField, ReadOnly] private CinemachineVirtualCamera virtualCamera;

        #region Cinemachine Components

        private CinemachineFramingTransposer _framingTransposer;
        private CinemachineConfiner2D _confiner2D;
        private CinemachineBasicMultiChannelPerlin _basicMultiChannelPerlin;

        #endregion
        private Vector2 _currentLookAtOffset;
        private float _lookAtY;

        private PlayerAgent _player;

        protected override void Awake()
        {
            if (inMenu)
            {
                if (instance != null)
                    Destroy(gameObject);
            }
            else
            {
                base.Awake();
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
                _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                _confiner2D = virtualCamera.GetComponent<CinemachineConfiner2D>();
                _basicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        private void OnEnable()
        {
            EventCenter.Subscribe<ShakeCameraEvent>(ShakeCamera);
        }

        private void OnDisable()
        {
            EventCenter.UnSubscribe<ShakeCameraEvent>(ShakeCamera);
        }

        private void Start()
        {
            if (inMenu)
                return;
            _player = FindObjectOfType<PlayerAgent>();
            if (_player != null)
            {
                followTarget = _player.transform;
                virtualCamera.Follow = followTarget;
                virtualCamera.LookAt = followTarget;
            }
            FindBoundingShape();
        }

        private void Update()
        {
            if (inMenu || virtualCamera == null || followTarget == null)
                return;
            
            ControlCameraLookAt();
        }

        /// <summary>
        /// 控制相机看向某处
        /// </summary>
        private void ControlCameraLookAt()
        {
            if (!enableLookAt || _framingTransposer == null || PlayerAgent.Ins == null)
                return;

            var agent = PlayerAgent.Ins;
            _currentLookAtOffset.x = Mathf.Lerp(_currentLookAtOffset.x, agent.Orientation * lookAtOffsetDist.x, lookAtSmoothTime * Time.deltaTime);
            if (Keyboard.current.sKey.isPressed)
                _lookAtY = -lookAtOffsetDist.y;
            else
                _lookAtY = 0;
            _currentLookAtOffset.y = Mathf.Lerp(_currentLookAtOffset.y, _lookAtY, lookAtSmoothTime * Time.deltaTime);
            _framingTransposer.m_TrackedObjectOffset = _currentLookAtOffset;
        }

        private void FindBoundingShape()
        {
            var boundCollider = GameObject.Find("CameraCollider");
            if (boundCollider != null)
            {
                var c = boundCollider.GetComponent<PolygonCollider2D>();
                SetBoundingShape(c);
            }
        }
        
        public void SetBoundingShape(PolygonCollider2D boundCollider)
        {
            _confiner2D.m_BoundingShape2D = boundCollider;
        }

        private Coroutine _shakeCoroutine;
        /// <summary>
        /// 相机震动触发接口，可使用事件触发，触发形式：
        /// EventCenter.Fire(new ShakeCameraEvent
        /// {
        ///     duration = 0,
        ///     amplitude = 0,   
        ///     frequency = 0
        /// });
        /// </summary>
        /// <param name="shakeCameraEvent"></param>
        public void ShakeCamera(ShakeCameraEvent shakeCameraEvent)
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeCameraEvent.duration, shakeCameraEvent.amplitude, shakeCameraEvent.frequency));
        }
        
        private IEnumerator ShakeCoroutine(float duration, float amplitude, float frequency)
        {
            _basicMultiChannelPerlin.m_AmplitudeGain = amplitude;
            _basicMultiChannelPerlin.m_FrequencyGain = frequency;
            yield return new WaitForSeconds(duration);
            _basicMultiChannelPerlin.m_AmplitudeGain = 0;
            _basicMultiChannelPerlin.m_FrequencyGain = 0;
        }
    }
        
}