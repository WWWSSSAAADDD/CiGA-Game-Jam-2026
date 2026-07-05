using Game.Gameplay.Shop;
using Game.Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI
{
    enum ShopSlotType
    {
        Card,
        Item
    }
    
    /// <summary>
    /// 商店卡片槽位（表现层）。
    /// 职责：显示一张效果卡的效果/花费，点击购买按钮时命令 CrusherController 尝试购买；
    /// 背包资源变化后自动刷新购买按钮是否可点（买不起时置灰）。
    ///
    /// 注意：这是整张通信图里少数会"命令"别的模块的表现层脚本——商店购买按钮本来就需要
    /// 主动发起购买（TryBuyCard），不是纯粹的只读展示，跟 ResourceHUD/HealthBar/ScanInfoPanel
    /// 那种"只进不出"的表现层不完全一样，这是商店 UI 天然的例外。
    ///
    /// 不做动态列表——每个槽位在 Inspector 里直接绑定一张固定的 EffectCardData，够用 jam 的 MVP 范围；
    /// 以后要做"卡池随机刷新"的动态列表，再在这之上包一层按 CrusherController.GetAvailableCards()
    /// 动态实例化槽位 Prefab 的管理脚本。
    /// </summary>
    public class ShopCardSlotUI : MonoBehaviour
    {
        [SerializeField] private ShopSlotType shopSlotType;
        
        [Tooltip("Inspector 里拖这个槽位对应的效果卡资产（Assets 里用 Create > 锚叠世界 > 效果卡数据 创建）。")]
        [SerializeField] private EffectCardData card;

        [Tooltip("item和card选择一个进行售卖")]
        [SerializeField] private ItemData item;
        
        [Header("展示与交互控件")]
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button buyButton;

        private void OnEnable()
        {
            RefreshLabels();
            RefreshInteractable();

            if (buyButton != null)
                buyButton.onClick.AddListener(HandleBuyClicked);

            // Inventory 是常驻单例，面板显示/隐藏时反复订阅/退订不依赖 Awake 先后顺序，用 OnEnable/OnDisable 安全。
            if (Inventory.Instance != null)
                Inventory.Instance.OnResourceChanged += HandleResourceChanged;
        }

        private void OnDisable()
        {
            if (buyButton != null)
                buyButton.onClick.RemoveListener(HandleBuyClicked);

            if (Inventory.Instance != null)
                Inventory.Instance.OnResourceChanged -= HandleResourceChanged;
        }

        private void HandleResourceChanged(ResourceType type, int amount)
        {
            RefreshInteractable();
        }

        private void HandleBuyClicked()
        {
            if (shopSlotType == ShopSlotType.Card)
            {
                if (card == null || CrusherController.Instance == null) return;

                CrusherController.Instance.TryBuyCard(card);
                // 买成功会触发 Inventory.OnResourceChanged，上面订阅的 HandleResourceChanged 自己会刷新按钮状态，
                // 这里不需要重复刷新；买不起时 TryBuyCard 内部什么都不会改，也就不会有事件，按钮状态本来就正确。
            }
            else if (shopSlotType == ShopSlotType.Item)
            {
                if (item == null || CrusherController.Instance == null) return;
                CrusherController.Instance.TryBuyItem(item);
            }
        }

        private void RefreshLabels()  
        {
            if (shopSlotType == ShopSlotType.Card)
            {
                if (card == null) return;
                if (effectText != null)
                    effectText.text = card.DisplayName;
                if (costText != null)
                    costText.text = $"{card.CostResourceType} x{card.CostAmount}";
            }
            else if (shopSlotType == ShopSlotType.Item)
            {
                if (item == null) return;
                if (effectText != null)
                    effectText.text = item.DisplayName;
                if (costText != null)
                    costText.text = $"{item.CostResourceType} x{item.CostAmount}";
            }
        }

        private void RefreshInteractable()
        {
            if (shopSlotType == ShopSlotType.Card)
            {
                if (buyButton == null || card == null) return;
                buyButton.interactable = CrusherController.Instance != null && CrusherController.Instance.CanAfford(card);
            }
            else if (shopSlotType == ShopSlotType.Item)
            {
                if (buyButton == null || item == null) return;
                buyButton.interactable = CrusherController.Instance != null && CrusherController.Instance.CanAfford(item);
                
            }
        }
    }
}
