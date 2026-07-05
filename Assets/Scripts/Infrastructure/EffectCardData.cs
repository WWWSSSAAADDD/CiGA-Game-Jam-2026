using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// 效果卡要调整的具体数值项。前缀 Ship/Anchor 表明这项数值该结算到飞船还是船锚身上，
    /// 由查询方（CrusherController）翻译成对应的 ShipUpgradeType/AnchorUpgradeType 调用。
    /// 这里没有直接复用 ShipUpgradeType/AnchorUpgradeType 这两个枚举——它们分别定义在
    /// Game.Gameplay.Ship/Game.Gameplay.Anchor 里，基础设施层不能反过来依赖核心玩法层，
    /// 所以在这里自己重新声明一份等价的数值项。
    /// </summary>
    public enum EffectStatType
    {
        ShipThrust,
        ShipMaxHealth,
        ShipMaxEnergy,
        ShipMass,
        AnchorPenetration,
        AnchorMinSpeed,
        AnchorMass,
        AnchorChainLength,
        AnchorHeadCount
    }

    /// <summary>
    /// 效果卡配置数据（基础设施层，策划配置资产）。
    /// 一张卡描述"花多少某种资源能在商店买到、买到后给谁的哪项数值加多少"，纯数据，不涉及任何事件。
    /// 只被 EffectCardDatabase 收纳、被 CrusherController 查询/购买，自己不引用任何其他模块。
    /// </summary>
    [CreateAssetMenu(fileName = "EffectCardData", menuName = "锚叠世界/效果卡数据")]
    public class EffectCardData : ScriptableObject
    {
        [Header("购买成本")]
        [Tooltip("购买这张卡需要消耗哪种资源。")]
        [SerializeField] private ResourceType costResourceType;
        [Tooltip("购买这张卡需要消耗多少数量。")]
        [Min(0)]
        [SerializeField] private int costAmount;
        [Header("升级卡名称")]
        [SerializeField] private string displayName;
        
        [Header("效果")]
        [Tooltip("这张卡具体调整哪一项数值。")]
        [SerializeField] private EffectStatType statType;
        [Tooltip("数值变化量，正数是加成，负数是削弱。")]
        [SerializeField] private float delta;

        public ResourceType CostResourceType => costResourceType;
        public int CostAmount => costAmount;
        public EffectStatType StatType => statType;
        public float Delta => delta;
        public string  DisplayName => displayName;
    }
}
