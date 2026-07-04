using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Anchor
{
    /// <summary>
    /// 船锚模块 —— 玩家主动释放部分（核心玩法层）。
    /// 职责：订阅输入模块的释放按键事件，命令 AnchorController 释放当前锚定的小行星。
    /// 状态机里唯一两条"主动控制代码"转移之一（另一条是碰撞触发锚定，走 AnchorController 自己的碰撞回调），
    /// 其余所有摆动/甩动都是物理引擎被动解算，这里不涉及。
    ///
    /// 注意：在商店范围内，同一个空格键会先被 ShopTriggerZone 拦截去做"销毁结算"（见该类的
    /// DefaultExecutionOrder 说明），这里的 ReleaseCurrentAsteroid() 到时候读到的锚已经是空的，
    /// 单纯是个安全的空操作，不会重复处理，这个类本身不需要、也不应该知道商店的存在。
    /// </summary>
    [RequireComponent(typeof(AnchorController))]
    public class AnchorRelease : MonoBehaviour
    {
        private AnchorController anchor;

        private void Awake()
        {
            anchor = GetComponent<AnchorController>();
        }

        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 InputReader.Instance 已就绪。
            if (InputReader.Instance != null)
                InputReader.Instance.OnReleasePressed += HandleReleasePressed;
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
                InputReader.Instance.OnReleasePressed -= HandleReleasePressed;
        }

        private void HandleReleasePressed()
        {
            anchor.ReleaseCurrentAsteroid();
        }
    }
}
