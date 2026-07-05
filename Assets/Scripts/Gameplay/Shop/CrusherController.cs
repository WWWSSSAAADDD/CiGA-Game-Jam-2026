using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Game.Gameplay.Anchor;
using Game.Gameplay.Asteroid;
using Game.Gameplay.Ship;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Shop
{
    /// <summary>
    /// 粉碎商店模块（核心玩法层）。
    /// 职责：把船锚上挂着的小行星依次粉碎、把资源结算进背包；另外提供"用资源购买效果卡"的商店购买逻辑。
    /// 不是实体本身，但协调的是"小行星、背包、飞船、船锚、效果卡配置表"这几个已有实体之间的业务流程，
    /// 且被 ShopTriggerZone（编排层）直接命令调用，划入核心玩法层是为了让编排层只留"该不该触发"这一层薄壳。
    /// 单例（逻辑唯一）。
    /// </summary>
    public class CrusherController : MonoBehaviour
    {
        public static CrusherController Instance { get; private set; }
        
        [Header("升级结算目标")]
        [Tooltip("Inspector 里拖飞船的 shipController 进来，玩家购买效果卡后给飞船结算升级用。")]
        [SerializeField] private ShipController shipController;
        [Tooltip("Inspector 里拖船锚的 anchorController 进来，玩家购买效果卡后给船锚结算升级用。")]
        [SerializeField] private AnchorController anchorController;
        
        [Header("效果卡配置表")]
        [Tooltip("Inspector 里拖效果卡数据库资产进来，构成商店里能买到的卡片目录。")]
        [SerializeField] private EffectCardDatabase effectCardDatabase;

        /// <summary>通知：完成一次粉碎结算。订阅方：小行星生成模块（用来推进难度曲线）。</summary>
        public event Action OnCrushCompleted;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void TryCrushAsteroid(AsteroidController asteroid)
        {
            if (asteroid == null) return;
            
            ResourcePayload payload = asteroid.GetResourcePayload();
            Inventory.Instance?.AddResource(payload.Type, payload.Amount);  // 销毁小行星向背包添加资源
            
            // 先添加资源再销毁小行星，防止丢失引用
            asteroid.CrushAsteroid();
            
            OnCrushCompleted?.Invoke();
        }
        
        /// <summary>查询：商店目录里全部效果卡。调用方：商店购买界面（列出可买的卡）。</summary>
        public List<EffectCardData> GetAvailableCards()
        {
            return effectCardDatabase != null ? effectCardDatabase.GetAllCards() : new List<EffectCardData>();
        }

        /// <summary>查询：背包里对应资源是否够买这张卡。调用方：商店购买界面（判断卡片能不能点）。</summary>
        public bool CanAfford(EffectCardData card)
        {
            if (card == null || Inventory.Instance == null) return false;

            return Inventory.Instance.GetResourceAmount(card.CostResourceType) >= card.CostAmount;
        }

        public bool CanAfford(ItemData item)
        {
            if (item == null || Inventory.Instance == null) return false;
            return Inventory.Instance.GetResourceAmount(item.CostResourceType) >= item.CostAmount;
        }
        
        /// <summary>
        /// 命令：尝试购买一张效果卡——买得起就扣掉对应资源、应用卡片效果；买不起什么都不做。
        /// 调用方：商店购买界面（点击"购买"按钮）。
        /// </summary>
        public bool TryBuyCard(EffectCardData card)
        {
            if (!CanAfford(card)) return false;

            Inventory.Instance.AddResource(card.CostResourceType, -card.CostAmount);
            ApplyEffectCard(card);
            return true;
        }

        public bool TryBuyItem(ItemData item)
        {
            if (!CanAfford(item)) return false;
            
            Inventory.Instance.AddResource(item.CostResourceType, -item.CostAmount);
            Inventory.Instance.AddItem(item, 1);
            return true;
        }

        // 把效果卡的 EffectStatType 翻译成具体的 ShipUpgradeType/AnchorUpgradeType 调用。
        private void ApplyEffectCard(EffectCardData card)
        {
            switch (card.StatType)
            {
                case EffectStatType.ShipThrust:
                    shipController?.ApplyUpgrade(ShipUpgradeType.Thrust, card.Delta);
                    break;
                case EffectStatType.ShipMaxHealth:
                    shipController?.ApplyUpgrade(ShipUpgradeType.MaxHealth, card.Delta);
                    break;
                case EffectStatType.ShipMaxEnergy:
                    shipController?.ApplyUpgrade(ShipUpgradeType.MaxEnergy, card.Delta);
                    break;
                case EffectStatType.ShipMass:
                    shipController?.ApplyUpgrade(ShipUpgradeType.Mass, card.Delta);
                    break;
                case EffectStatType.AnchorPenetration:
                    anchorController?.ApplyUpgrade(AnchorUpgradeType.Penetration, card.Delta);
                    break;
                case EffectStatType.AnchorMinSpeed:
                    anchorController?.ApplyUpgrade(AnchorUpgradeType.MinAnchorSpeed, card.Delta);
                    break;
                case EffectStatType.AnchorMass:
                    anchorController?.ApplyUpgrade(AnchorUpgradeType.Mass, card.Delta);
                    break;
                case EffectStatType.AnchorChainLength:
                    anchorController?.ApplyUpgrade(AnchorUpgradeType.ChainLength, card.Delta);
                    break;
                case EffectStatType.AnchorHeadCount:
                    anchorController?.ApplyUpgrade(AnchorUpgradeType.HeadCount, card.Delta);
                    break;
                default:
                    Debug.LogWarning($"{nameof(CrusherController)} 遇到了没有翻译规则的 {nameof(EffectStatType)}：{card.StatType}");
                    break;
            }
        }
    }
}
