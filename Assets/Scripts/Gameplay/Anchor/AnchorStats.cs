using System;
using UnityEngine;

namespace Game.Gameplay.Anchor
{
    /// <summary>
    /// 可升级的数值类型，供粉碎商店模块结算升级时按类型调整对应数值。
    /// </summary>
    public enum AnchorUpgradeType
    {
        Penetration,
        MinAnchorSpeed,
        Mass,
        ChainLength,
        HeadCount
    }

    /// <summary>
    /// 船锚模块 —— 数值部分（核心玩法层）。
    /// 只存数值（穿透强度/最小锚定速度/质量/船锚长度/锚头数量），暴露 ApplyUpgrade() 命令方法；
    /// 内部改完值后自己触发通知。不写任何物理/碰撞逻辑——那是 AnchorController 的职责。
    /// </summary>
    public class AnchorStats : MonoBehaviour
    {
        [Header("锚定判定")]
        [Tooltip("穿透强度：要 >= 小行星硬度才能锚定成功。")]
        [SerializeField] private float penetration = 1f;
        [Tooltip("最小锚定速度：撞击相对速度要 >= 这个值才会触发锚定判定，太轻的碰撞直接弹开。")]
        [SerializeField] private float minAnchorSpeed = 1f;

        [Header("物理数值")]
        [SerializeField] private float mass = 1f;
        [SerializeField] private float chainLength = 5f;

        [Header("多锚（船锚串联用，P2 才会用到）")]
        [SerializeField] private int headCount = 1;

        public float Penetration => penetration;
        public float MinAnchorSpeed => minAnchorSpeed;
        public float Mass => mass;
        public float ChainLength => chainLength;

        /// <summary>命令：应用一次升级（粉碎商店结算时调用）。内部改完值后自己广播 OnStatsChanged。</summary>
        internal void ApplyUpgrade(AnchorUpgradeType type, float delta)
        {
            switch (type)
            {
                case AnchorUpgradeType.Penetration:
                    penetration = Mathf.Max(0f, penetration + delta);
                    break;
                case AnchorUpgradeType.MinAnchorSpeed:
                    minAnchorSpeed = Mathf.Max(0f, minAnchorSpeed + delta);
                    break;
                case AnchorUpgradeType.Mass:
                    mass = Mathf.Max(0.01f, mass + delta);
                    break;
                case AnchorUpgradeType.ChainLength:
                    chainLength = Mathf.Max(0.1f, chainLength + delta);
                    break;
                case AnchorUpgradeType.HeadCount:
                    headCount = Mathf.Max(1, headCount + Mathf.RoundToInt(delta));
                    break;
            }
        }
    }
}
