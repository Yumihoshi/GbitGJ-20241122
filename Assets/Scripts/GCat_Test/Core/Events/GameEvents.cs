namespace GCat_Test.Core.Events
{
    /// <summary>
    /// 剧情触发事件
    /// </summary>
    public struct StoryTriggerEvent
    {
    }

    /// <summary>
    /// 剧情结束事件
    /// </summary>
    public struct StoryEndEvent
    {
    }

    /// <summary>
    /// 相机震动事件
    /// </summary>
    public struct ShakeCameraEvent
    {
        public float duration;
        public float amplitude;
        public float frequency;
    }
}