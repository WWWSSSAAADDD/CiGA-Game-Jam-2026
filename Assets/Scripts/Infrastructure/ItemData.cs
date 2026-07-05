using Game.Gameplay.Ship;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// 道具配置数据（基础设施层，策划配置资产）。
    /// 存道具的基本展示信息，暴露 Use() 让使用方触发效果。
    /// 效果本身的具体实现留到未来——设计文档的引用图里道具模块目前只连到背包模块（存取消耗），
    /// 没有连到飞船/船锚的数值模块，这里如实保留一个空实现的占位方法，不提前发明一套效果系统。
    /// SO 资产本身唯一，直接引用资产即可，不用额外套单例壳；背包用这个资产引用本身当字典 key。
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "锚叠世界/道具数据")]
    public class ItemData : ScriptableObject
    {
        [Header("基本信息")]
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        
        [Header("购买成本")]
        [Tooltip("购买这个道具需要消耗哪种资源。")]
        [SerializeField] private ResourceType costResourceType;
        [Tooltip("购买这张卡需要消耗多少数量。")]
        [Min(0)]
        [SerializeField] private int costAmount;
        public string DisplayName => displayName;
        public string Description => description;
        
        public ResourceType CostResourceType => costResourceType;
        public int CostAmount => costAmount;
        
        /// <summary>
        /// 占位：使用这件道具触发的效果。当前是空实现——效果系统是未来工作，
        /// ItemUseHandler 已经按"背包消耗成功才调用"接好了这个方法，以后实现效果时只需要填这里。
        /// </summary>
        public virtual void Use(ShipController shipController)
        {
        }
    }

    [CreateAssetMenu(fileName = "ItemData", menuName = "锚叠世界/道具数据/向前突进")]
    public class ImpulseItem : ItemData
    {
        [SerializeField] private float force = 50;
        public override void Use(ShipController shipController)
        {
            if (shipController.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 dir = InputReader.Instance.MoveInput.normalized;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
        }
    }
}
