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
    /// 职责：用触发器维护"飞船是否在商店范围内"这个状态并同步给船锚链条；范围内响应开店键，
    /// 切换商店页面 UI 面板的显示/隐藏；飞船离开范围时顺带把面板收起来，避免玩家飞走了商店页面还开着；
    /// 另外对任何进入商店范围、且未被锚定的小行星命令粉碎商店自动销毁结算。
    /// 不是商店本身——粉碎/结算/购买的实际逻辑都在 CrusherController 里，这里只负责"该不该触发/该不该显示"。
    ///
    /// 销毁键（空格/Shift+空格）不再由这里处理——"在商店范围内就结算、不在范围内就纯释放"这个判断，
    /// 连同"该释放/结算链条上哪一节"，现在整个交给 AnchorChainController 统一处理（见该类），
    /// 这里只需要把 shipInRange 状态通过 SetShipInRange() 命令同步给它。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShopTriggerZone : MonoBehaviour
    {
        [Header("要结算的船锚链")]
        [Tooltip("Inspector 里拖飞船身上的船锚链条控制器（AnchorChainController）进来，用来同步" +
                 "\"是否在商店范围内\"这个状态。")]
        [SerializeField] private AnchorChainController anchorChain;

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
                InputReader.Instance.OnOpenShopPressed += HandleOpenShopUIPressed;
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
                InputReader.Instance.OnOpenShopPressed -= HandleOpenShopUIPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
            {
                shipInRange = true;
                anchorChain?.SetShipInRange(true);
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
                anchorChain?.SetShipInRange(false);

                // 离开商店范围时顺带把面板收起来，避免飞走了商店页面还挂在屏幕上。
                if (shopPanel != null)
                    shopPanel.SetActive(false);
            }
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
