using Game.Gameplay.Anchor;
using Game.Gameplay.Ship;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Orchestration
{
    /// <summary>
    /// 商店触发模块（编排层）。
    /// 职责：用触发器维护"飞船是否在商店范围内"这个状态；范围内响应开店键，命令粉碎商店结算。
    /// 不是商店本身——粉碎/结算的实际逻辑都在 CrusherController 里，这里只负责"该不该触发"。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShopTriggerZone : MonoBehaviour
    {
        [Header("要结算的船锚")]
        [Tooltip("Inspector 里拖飞船身上的 AnchorController 进来，玩家开店时传给 CrusherController.Open()。")]
        [SerializeField] private AnchorController anchor;

        // 挂这个脚本的 Collider2D 需要在 Inspector 里勾上 isTrigger，否则 OnTriggerEnter2D/Exit2D 不会触发。
        private bool shipInRange;

        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 InputReader.Instance 已就绪。
            if (InputReader.Instance != null)
                InputReader.Instance.OnOpenShopPressed += HandleOpenShopPressed;
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
                InputReader.Instance.OnOpenShopPressed -= HandleOpenShopPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
                shipInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
                shipInRange = false;
        }

        private void HandleOpenShopPressed()
        {
            if (!shipInRange) return;

            CrusherController.Instance?.Open(anchor);
        }
    }
}
