using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Item
{
    /// <summary>
    /// 道具使用模块（核心玩法层）。
    /// 职责：订阅"使用道具"按键，命令背包尝试消耗绑定的道具，消耗成功后触发道具自身的效果。
    /// 不存道具数据本身——那是背包模块的职责，这里只协调"按键 -> 消耗 -> 生效"这一次流程。
    /// </summary>
    public class ItemUseHandler : MonoBehaviour
    {
        [Tooltip("Inspector 里拖要绑定到 J 键的道具资产进来。单件道具槽位，够用 jam 的 MVP 范围。")]
        [SerializeField] private ItemData boundItem;

        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 InputReader.Instance 已就绪。
            if (InputReader.Instance != null)
                InputReader.Instance.OnUseItemPressed += HandleUseItemPressed;
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
                InputReader.Instance.OnUseItemPressed -= HandleUseItemPressed;
        }

        private void HandleUseItemPressed()
        {
            if (boundItem == null) return;
            if (Inventory.Instance == null) return;

            if (Inventory.Instance.TryConsumeItem(boundItem))
                boundItem.Use();
        }
    }
}
