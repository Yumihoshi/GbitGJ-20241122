using Aki_Test.Player;
using GCFramework.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GCat_Test.Core
{
    public class PlayerAgent : SingletonMono<PlayerAgent>
    {
        [ReadOnly] public PlayerController playerCtr;
        public int Orientation => (int)playerCtr.transform.localScale.x;
        
        protected override void Awake()
        {
            base.Awake();

            playerCtr = GetComponent<PlayerController>();
        }

        public Vector2 GetMoveInput()
        {
            var input = playerCtr.playerInputControl.GamePlay.Move.ReadValue<Vector2>();
            return input;
        }
    }
}
