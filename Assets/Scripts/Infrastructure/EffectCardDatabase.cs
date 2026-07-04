using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// 效果卡配置表（基础设施层，策划配置资产）。
    /// 职责：收纳一批 EffectCardData，暴露"取出全部可购买的卡"的查询方法。
    /// 具体"买不买得起、买了之后怎么生效"是 CrusherController 的事，这里只管配置数据本身。
    /// SO 资产本身唯一，直接引用资产即可，不用额外套单例壳。
    /// </summary>
    [CreateAssetMenu(fileName = "EffectCardDatabase", menuName = "锚叠世界/效果卡数据库")]
    public class EffectCardDatabase : ScriptableObject
    {
        [Tooltip("策划在 Inspector 里配置的全部效果卡，构成商店里能买到的卡片目录。")]
        [SerializeField] private List<EffectCardData> cards = new List<EffectCardData>();

        /// <summary>查询：商店目录里全部效果卡（过滤掉 Inspector 里没配置的空槽位）。</summary>
        public List<EffectCardData> GetAllCards()
        {
            var result = new List<EffectCardData>();

            foreach (var card in cards)
            {
                if (card != null)
                    result.Add(card);
            }

            return result;
        }
    }
}
