using System;
using System.Collections.Generic;
using Game.Gameplay.Anchor;
using Game.Gameplay.Asteroid;
using Game.Gameplay.Ship;
using Game.Infrastructure;
using UnityEngine;
using UnityEngine.Serialization;

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
        [Tooltip("Inspector 里拖飞船的 ShipStats 进来，玩家购买效果卡后给飞船结算升级用。")]
        [SerializeField] private ShipStats shipStats;
        [Tooltip("Inspector 里拖船锚的 AnchorStats 进来，玩家购买效果卡后给船锚结算升级用。")]
        [SerializeField] private AnchorStats anchorStats;

        [Header("效果卡配置表")]
        [Tooltip("Inspector 里拖效果卡数据库资产进来，构成商店里能买到的卡片目录。")]
        [SerializeField] private EffectCardDatabase effectCardDatabase;

        [Header("商店/粉碎等级（当前阶段仅记录，暂无消费方）")]
        [SerializeField] private int crushLevel = 1;
        [SerializeField] private int shopLevel = 1;
        
        [FormerlySerializedAs("reCrushCooldown")]
        [Header("粉碎小行星的CD")]
        [SerializeField] private float recrushCooldown = 0.5f;
        private float nextCrushAllowedTime;
        
        public int CrushLevel => crushLevel;
        public int ShopLevel => shopLevel;

        /// <summary>通知：完成一次粉碎结算。订阅方：小行星生成模块（用来推进难度曲线）。</summary>
        public event Action OnCrushCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{nameof(CrusherController)} 已存在一份单例实例，销毁多余的一份组件：{gameObject.name}");
                Destroy(this);
                return;
            }

            Instance = this;

            if (shipStats == null)
                Debug.LogWarning($"{nameof(CrusherController)} 没有指定 {nameof(shipStats)}，购买飞船相关的效果卡会静默不生效。");
            if (anchorStats == null)
                Debug.LogWarning($"{nameof(CrusherController)} 没有指定 {nameof(anchorStats)}，购买船锚相关的效果卡会静默不生效。");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 命令：粉碎指定船锚当前锚定的小行星，把资源结算进背包。
        /// 调用方：商店触发模块（玩家进入商店范围且按下开店键时）。
        /// 只结算资源，不自动应用任何效果卡——买卡是玩家在商店里的另一个主动操作，见 TryBuyCard()。
        /// </summary>
        public void CrushWithShip(AnchorController anchor)
        {
            if (Time.time < nextCrushAllowedTime) return;
            
            if (anchor == null) return;
            
            AsteroidController asteroid = anchor.AnchoredAsteroid;
            if (asteroid == null) return; // 链条上没有挂着小行星，没什么可粉碎的
            CrushWithoutShip(asteroid);
            
            anchor.ReleaseCurrentAsteroid();
        }

        public void CrushWithoutShip(AsteroidController asteriod)
        {
            if (Time.time < nextCrushAllowedTime) return;
            
            // 如果是小行星，且小行星处于非锚定状态，则可以Crush
            if (asteriod == null || asteriod.IsAnchored) return;
            asteriod.Crush();
            
            OnCrushCompleted?.Invoke();
            
            nextCrushAllowedTime = Time.time + recrushCooldown;
            ResourcePayload payload = asteriod.GetResourcePayload();
            Inventory.Instance?.AddResource(payload.Type, payload.Amount);
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

        // 把效果卡的 EffectStatType 翻译成具体的 ShipUpgradeType/AnchorUpgradeType 调用。
        private void ApplyEffectCard(EffectCardData card)
        {
            switch (card.StatType)
            {
                case EffectStatType.ShipThrust:
                    shipStats?.ApplyUpgrade(ShipUpgradeType.Thrust, card.Delta);
                    break;
                case EffectStatType.ShipMaxHealth:
                    shipStats?.ApplyUpgrade(ShipUpgradeType.MaxHealth, card.Delta);
                    break;
                case EffectStatType.ShipMaxEnergy:
                    shipStats?.ApplyUpgrade(ShipUpgradeType.MaxEnergy, card.Delta);
                    break;
                case EffectStatType.ShipMass:
                    shipStats?.ApplyUpgrade(ShipUpgradeType.Mass, card.Delta);
                    break;
                case EffectStatType.AnchorPenetration:
                    anchorStats?.ApplyUpgrade(AnchorUpgradeType.Penetration, card.Delta);
                    break;
                case EffectStatType.AnchorMinSpeed:
                    anchorStats?.ApplyUpgrade(AnchorUpgradeType.MinAnchorSpeed, card.Delta);
                    break;
                case EffectStatType.AnchorMass:
                    anchorStats?.ApplyUpgrade(AnchorUpgradeType.Mass, card.Delta);
                    break;
                case EffectStatType.AnchorChainLength:
                    anchorStats?.ApplyUpgrade(AnchorUpgradeType.ChainLength, card.Delta);
                    break;
                case EffectStatType.AnchorHeadCount:
                    anchorStats?.ApplyUpgrade(AnchorUpgradeType.HeadCount, card.Delta);
                    break;
                default:
                    Debug.LogWarning($"{nameof(CrusherController)} 遇到了没有翻译规则的 {nameof(EffectStatType)}：{card.StatType}");
                    break;
            }
        }
    }
}
