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
    /// 注意：在商店范围内，空格键的"销毁结算"改由 ShopTriggerZone/CrusherController 接管——这里按下前
    /// 先查询 AnchorController.IsShipInRange，为真就直接跳过，不会跟对方抢着处理同一次按键，
    /// 这个判断不依赖两个订阅者谁先谁后，这个类本身也不需要、不应该知道商店的存在。
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
            if (anchor.IsShipInRange) return; // 商店范围内的销毁结算交给 ShopTriggerZone/CrusherController，这里不重复处理

            anchor.ReleaseCurrentAsteroid();
        }
    }
}
