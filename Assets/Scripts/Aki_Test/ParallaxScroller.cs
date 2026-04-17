using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aki_Test
{
    [Serializable]
    public struct LayerInfo
    {
        public Transform layer;         // 层的位置
        public float parallaxScales;    // 每个背景层的速度比例
    }
    
    public class ParallaxScroller : MonoBehaviour
    {
        public LayerInfo[] layers;
        public float smoothing = 1f;          // 平滑度

        private Transform cam;
        private Vector3 previousCamPos;

        void Start()
        {
            cam = Camera.main.transform;
            previousCamPos = cam.position;
        }

        void Update()
        {
            for (int i = 0; i < layers.Length; i++)
            {
                // 计算当前帧摄像机的偏移量
                float parallax = (previousCamPos.x - cam.position.x) * layers[i].parallaxScales;

                // 计算目标位置
                float backgroundTargetPosX = layers[i].layer.position.x + parallax;

                // 使用Lerp函数平滑移动
                Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, layers[i].layer.position.y, layers[i].layer.position.z);

                layers[i].layer.position = Vector3.Lerp(layers[i].layer.position, backgroundTargetPos, smoothing * Time.deltaTime);
            }

            // 更新前一帧的摄像机位置
            previousCamPos = cam.position;
        }
    }
}