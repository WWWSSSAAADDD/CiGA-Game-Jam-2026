using Game.Gameplay.Anchor;
using Game.Gameplay.Asteroid;
using Game.Gameplay.Shop;
using Game.Gameplay.Ship;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Orchestration
{
    /// <summary>
    /// 商店触发模块（编排层）。
    /// 职责：用触发器维护"飞船是否在商店范围内"这个状态并同步给船锚；范围内响应销毁键，命令粉碎商店结算；
    /// 范围内响应开店键，切换商店页面 UI 面板的显示/隐藏；飞船离开范围时顺带把面板收起来，
    /// 避免玩家飞走了商店页面还开着。
    /// 不是商店本身——粉碎/结算/购买的实际逻辑都在 CrusherController 里，这里只负责"该不该触发/该不该显示"。
    ///
    /// 空格键同时被 AnchorRelease（纯释放）和这里（范围内销毁结算）订阅，两边谁先响应都不影响正确性——
    /// 靠的不是订阅顺序，而是这里把 shipInRange 状态通过 SetShipInRange() 命令同步给 AnchorController，
    /// AnchorRelease 按键时会先查询这个状态自己决定要不要跳过。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShopTriggerZone : MonoBehaviour
    {
        [Header("要结算的船锚")]
        [Tooltip("Inspector 里拖飞船身上的 AnchorController 进来，玩家销毁时传给 CrusherController.Open()。")]
        [SerializeField] private AnchorController anchor;

        [Header("商店页面 UI")]
        [Tooltip("Inspector 里拖商店页面的根物体（Canvas 下的 ShopPanel）进来。M 键在范围内按下时切换显示/隐藏；" +
                 "飞船离开范围时自动隐藏。留空则 M 键在这个商店什么都不会发生。")]
        [SerializeField] private GameObject shopPanel;

        // 挂这个脚本的 Collider2D 需要在 Inspector 里勾上 isTrigger，否则 OnTriggerEnter2D/Exit2D 不会触发。
        private bool shipInRange;
        
        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 InputReader.Instance 已就绪。
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed += HandleReleasePressed;
                InputReader.Instance.OnOpenShopPressed += HandleOpenShopUIPressed;
            }
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed -= HandleReleasePressed;
                InputReader.Instance.OnOpenShopPressed -= HandleOpenShopUIPressed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
            {
                shipInRange = true;
                anchor?.SetShipInRange(true);
            }

            if (other.TryGetComponent(out AsteroidController asteroid))
            {
                CrusherController.Instance?.CrushWithoutShip(asteroid);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
            {
                shipInRange = false;
                anchor?.SetShipInRange(false);

                // 离开商店范围时顺带把面板收起来，避免飞走了商店页面还挂在屏幕上。
                if (shopPanel != null)
                    shopPanel.SetActive(false);
            }
        }

        /// <summary>空格键：范围内命令粉碎商店销毁结算；不在范围内什么都不做，交给 AnchorRelease 处理普通释放。</summary>
        private void HandleReleasePressed()
        {
            if (!shipInRange) return;

            CrusherController.Instance?.CrushWithShip(anchor);
        }

        /// <summary>M 键：范围内切换商店页面的显示/隐藏。</summary>
        private void HandleOpenShopUIPressed()
        {
            if (!shipInRange) return;

            if (shopPanel == null)
            {
                Debug.LogWarning($"{nameof(ShopTriggerZone)} 没有指定 {nameof(shopPanel)}，无法打开商店页面。");
                return;
            }

            shopPanel.SetActive(!shopPanel.activeSelf);
        }
    }
}
