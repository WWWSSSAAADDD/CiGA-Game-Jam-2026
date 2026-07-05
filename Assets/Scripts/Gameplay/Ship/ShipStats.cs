using System;
using UnityEngine;

namespace Game.Gameplay.Ship
{
    /// <summary>
    /// 可升级的数值类型，供粉碎商店模块（CrusherController）结算升级时按类型调整对应数值。
    /// </summary>
    public enum ShipUpgradeType
    {
        Thrust,
        MaxHealth,
        MaxEnergy,
        Mass,
        MaxSpeed
    }
    
    public class ShipStats : MonoBehaviour
    {
        [Header("血量")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float health = 100f;

        [Header("推力 / 质量 / 能源")]
        [SerializeField] private float thrust = 10f;
        [SerializeField] private float mass = 1f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float energy = 100f;
        [SerializeField] private float maxSpeed = 50f;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Thrust => thrust;
        public float Mass => mass;
        public float MaxSpeed => maxSpeed;

        /// <summary>血量变化通知：(当前血量, 最大血量)。订阅方：UI 模块（HealthBar）。</summary>
        public event Action<float, float> OnHealthChanged;

        
        public event Action OnDied;
        
        /// <summary>数值变化通知（升级生效后）。订阅方：任何需要感知飞船数值变化的模块。</summary>
        public event Action OnStatsChanged;

        private void Awake()
        {
            health = Mathf.Clamp(health, 0f, maxHealth);
        }

        /// <summary>命令：受到伤害。内部改完 health 后自己广播 OnHealthChanged。</summary>
        public void ApplyDamage(float amount)
        {
            if (amount <= 0f) return;

            health = Mathf.Clamp(health - amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(health, maxHealth);
            
            if (health <= 0f) OnDied?.Invoke();
        }

        /// <summary>命令：应用一次升级（粉碎商店升级飞船时时调用）。内部改完值后自己广播 OnStatsChanged。</summary>
        internal void ApplyUpgrade(ShipUpgradeType type, float delta)
        {
            switch (type)
            {
                case ShipUpgradeType.Thrust:
                    thrust = Mathf.Max(0f, thrust + delta);
                    break;
                case ShipUpgradeType.MaxHealth:
                    maxHealth = Mathf.Max(1f, maxHealth + delta);
                    health = Mathf.Min(health + Mathf.Max(delta, 0f), maxHealth);
                    // 这条分支顺带改了 health，UI 的 HealthBar 只订阅 OnHealthChanged（不订阅 OnStatsChanged），
                    // 所以这里需要额外广播一次，否则升级最大血量后血条不会刷新。
                    OnHealthChanged?.Invoke(health, maxHealth);
                    break;
                case ShipUpgradeType.MaxEnergy:
                    maxEnergy = Mathf.Max(0f, maxEnergy + delta);
                    energy = Mathf.Min(energy + Mathf.Max(delta, 0f), maxEnergy);
                    break;
                case ShipUpgradeType.Mass:
                    mass = Mathf.Max(0.01f, mass + delta);
                    break;
            }

            OnStatsChanged?.Invoke();
        }
    }
}
