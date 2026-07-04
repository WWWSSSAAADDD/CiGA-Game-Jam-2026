using UnityEngine;

namespace Game.Gameplay.Ship
{
    /// <summary>
    /// 飞船模块 —— 受损部分（核心玩法层）。
    /// 职责：碰撞发生时，根据冲击强度算出伤害，命令 ShipStats.ApplyDamage()。
    /// 不直接改 ShipStats 内部字段——改谁的数据就调用谁暴露的命令方法。
    /// </summary>
    [RequireComponent(typeof(ShipStats))]
    public class ShipDamage : MonoBehaviour
    {
        [Tooltip("撞击相对速度低于这个值不造成伤害，避免轻微擦碰也扣血。")]
        [SerializeField] private float minImpactSpeedForDamage = 1f;

        [Tooltip("撞击相对速度 -> 伤害的换算系数。")]
        [SerializeField] private float damagePerImpactSpeed = 2f;

        private ShipStats stats;

        private void Awake()
        {
            stats = GetComponent<ShipStats>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < minImpactSpeedForDamage) return;

            float damage = impactSpeed * damagePerImpactSpeed;
            stats.ApplyDamage(damage);
        }
    }
}
