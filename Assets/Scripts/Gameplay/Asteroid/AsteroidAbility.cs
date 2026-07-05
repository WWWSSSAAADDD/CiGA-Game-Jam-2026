using Game.Gameplay.Ship;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game.Gameplay.Asteroid
{
    public enum AbilityType
    {
        MaoDie, // 耄耋
        DingZhen, // 丁真
        DaGou // 大狗
    }

    /// <summary>
    /// 行星技能在Anchored的时候才触发
    /// </summary>
    public class AsteroidAbility : MonoBehaviour
    {
        [SerializeField] private AbilityType abilityType;
        [SerializeField] private float abilityCd;

        [Header("耄耋技能配置")] [Tooltip("耄耋给玩家造成的基础推力大小")] [SerializeField]
        private float maoDieForce;

        [FormerlySerializedAs("fluctuationRange")] [Tooltip("耄耋造成的推力的波动范围")] [SerializeField]
        private float maoDiefluctuationRange;

        [Header("大狗技能配置")] [Tooltip("大狗给自己施加的基础推力大小")] [SerializeField]
        private float daGouForce;

        [Tooltip("大狗推力的波动范围")] [SerializeField]
        private float daGoufluctuationRange;

        private Rigidbody2D shipRb;
        private float nextAbilityAllowedTime;
        private AsteroidController asteroidController;

        private void Awake()
        {
            asteroidController = GetComponent<AsteroidController>();
        }

        private void Start()
        {
            ShipController ship = FindObjectOfType<ShipController>();
            if (ship != null)
                shipRb = ship.GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (asteroidController.IsAnchored) TryTriggerAbility();
        }

        public void HandleAsteroidAnchored()
        {
            // TODO：被钩住时的表现效果
        }

        public void HandleAsteroidUnanchored()
        {
            // TODO: 脱钩的表现效果
        }

        public void TryTriggerAbility()
        {
            if (Time.time < nextAbilityAllowedTime) return;
            switch (abilityType)
            {
                case AbilityType.MaoDie:
                    TriggerMaoDieAbility();
                    break;
                case AbilityType.DingZhen:
                    TriggerDingZhenAbility();
                    break;
                case AbilityType.DaGou:
                    TriggerDaGouAbility();
                    break;
            }
        }

        // 被钩住时，间隔几秒给玩家施加随机方向的Force，同时播放音效
        private void TriggerMaoDieAbility()
        {
            if (shipRb != null)
            {
                Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
                float force = Random.Range(maoDieForce - maoDiefluctuationRange, maoDieForce + maoDiefluctuationRange);
                shipRb.AddForce(dir * force, ForceMode2D.Impulse);
                nextAbilityAllowedTime = Time.time + abilityCd;
            }
        }

        // 被钩住时，间隔几秒产生白色烟雾遮蔽玩家视野
        private void TriggerDingZhenAbility()
        {
            // TODO:释放烟雾效果

            nextAbilityAllowedTime = Time.time + abilityCd;
        }

        // 被玩家钩住时会脱钩跑开
        private void TriggerDaGouAbility()
        {
            nextAbilityAllowedTime = Time.time + abilityCd;

            // 脱钩 
            if (asteroidController != null)
            {
                asteroidController.EscapeFromAnchor();
            }

            // 给自己推力逃跑
            if (TryGetComponent(out Rigidbody2D asteroidRb))
            {
                Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
                float force = Random.Range(daGouForce - daGoufluctuationRange, daGouForce + daGoufluctuationRange);
                asteroidRb.AddForce(dir * force, ForceMode2D.Impulse);
                nextAbilityAllowedTime = Time.time + abilityCd;
            }
        }
    }
}