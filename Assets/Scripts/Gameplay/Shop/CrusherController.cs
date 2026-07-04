using System;
using System.Collections.Generic;
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

        [Header("粉碎充能池：有上限的剩余可粉碎次数，每隔固定时间恢复 1 点，销毁一次扣 1 点")]
        [SerializeField] private int maxCrushCharges = 5;
        [SerializeField] private float chargeRegenInterval = 0.5f;
        private int currentCrushCharges;
        private float chargeRegenTimer;

        public int CrushLevel => crushLevel;
        public int ShopLevel => shopLevel;
        public int CurrentCrushCharges => currentCrushCharges;
        public int MaxCrushCharges => maxCrushCharges;

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
            currentCrushCharges = maxCrushCharges;

            if (shipStats == null)
                Debug.LogWarning($"{nameof(CrusherController)} 没有指定 {nameof(shipStats)}，购买飞船相关的效果卡会静默不生效。");
            if (anchorStats == null)
                Debug.LogWarning($"{nameof(CrusherController)} 没有指定 {nameof(anchorStats)}，购买船锚相关的效果卡会静默不生效。");
        }

        private void Update()
        {
            if (currentCrushCharges >= maxCrushCharges) return;

            chargeRegenTimer += Time.deltaTime;
            if (chargeRegenTimer < chargeRegenInterval) return;

            chargeRegenTimer = 0f;
            currentCrushCharges++;
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
        public bool CrushWithShip(AnchorController anchor)
        {
            if (currentCrushCharges < 1) return false;

            if (anchor == null) return false;

            AsteroidController asteroid = anchor.AnchoredAsteroid;
            if (asteroid == null) return false; // 链条上没有挂着小行星，没什么可粉碎的

            // 注意：这里不能走 CrushWithoutShip()——那边专门守着"必须是未锚定状态"，
            // 而这里的小行星此刻恰恰还锚在船锚上，直接调用会被那道门槛拦下，
            // 变成"白放（不结算资源、不扣充能）"。销毁+结算的实际操作走共用的 CrushAndGrantResources()。
            anchor.ReleaseCurrentAsteroid();
            CrushAndGrantResources(asteroid);
            return true;
        }

        /// <summary>
        /// 命令：粉碎任意一颗未被锚定的小行星（通常是漂进商店范围、没人拖着的），把资源结算进背包。
        /// 调用方：商店触发模块（`OnTriggerEnter2D` 检测到未锚定的小行星进入范围时）。
        /// </summary>
        public void CrushWithoutShip(AsteroidController asteroid)
        {
            if (currentCrushCharges < 1) return;

            // 如果是小行星，且小行星处于非锚定状态，则可以Crush
            if (asteroid == null || asteroid.IsAnchored) return;

            CrushAndGrantResources(asteroid);
        }

        // 实际执行销毁 + 资源结算 + 扣充能 + 广播通知，CrushWithShip()/CrushWithoutShip() 两个入口共用。
        private void CrushAndGrantResources(AsteroidController asteroid)
        {
            ResourcePayload payload = asteroid.GetResourcePayload();

            asteroid.Crush();
            OnCrushCompleted?.Invoke();

            currentCrushCharges--;
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
