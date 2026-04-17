using System;
using Cysharp.Threading.Tasks;
using GCat_Test.Core.Camera;
using GCat_Test.Core.Manager;
using GCFramework.Runtime.UI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GCat_Test.Core.Levels
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TransportPoint : MonoBehaviour
    {
        [LabelText("传送ID")]
        public int transportId;
        [LabelText("目的地ID")]
        public int destinationId;
        [LabelText("着陆点")]
        public Transform landingPoint;
        [LabelText("所在房间")]
        public LevelRoom levelRoom;
        [LabelText("房间内相机边界")]
        public PolygonCollider2D roomBoundingShape;
        private BoxCollider2D _boxCollider2D;
        private bool _isTransfer;

        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider2D.isTrigger = true;
            landingPoint = transform.Find("land_point");
        }

        private void Start()
        {
            GameLevelManager.Ins.RegisterTransportPoint(this);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (Keyboard.current.eKey.wasPressedThisFrame && !_isTransfer)
                {
                    Transfer(other.transform).Forget();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _isTransfer = false;
        }

        private async UniTask Transfer(Transform player)
        {
            _isTransfer = true;
            // TODO:: 传送到目的地
            TransportPoint destPoint = GameLevelManager.Ins.GetDestination(destinationId);
            if (destPoint == null)
                return;
            GameLevelManager.Ins.Player.canMove = false;
            GameLevelManager.Ins.Player.canJump = false;
            await TransitionManager.Ins.EnterTransition(TransitionManager.TransitionEffect.FadeInOut_Scene);

            CameraCtr.Ins.SetBoundingShape(destPoint.roomBoundingShape);
            player.position = destPoint.landingPoint == null ? player.position : destPoint.landingPoint.position;
            
            await TransitionManager.Ins.ExitTransition(TransitionManager.TransitionEffect.FadeInOut_Scene);
            GameLevelManager.Ins.Player.canMove = true;
            GameLevelManager.Ins.Player.canJump = true;
        }
    }
}