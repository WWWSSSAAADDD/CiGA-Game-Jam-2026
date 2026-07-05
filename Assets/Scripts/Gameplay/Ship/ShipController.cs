using System.Collections;
using Game.Gameplay.Anchor;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Ship
{
    /// <summary>
    /// 飞船模块 —— 行为部分（核心玩法层）。
    /// 职责：任何外部模块与Ship模块交互，都通过ShipController接口
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ShipStats))]
    public class ShipController : MonoBehaviour
    {
        [Tooltip("Inspector 里拖同一艘飞船挂着的船锚物体进来，用来查询当前拖拽质量。留空则视为拖拽质量恒为 0。")]
        [SerializeField] private AnchorController anchor;

        private Rigidbody2D rb;
        private ShipStats stats;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<ShipStats>();
        }

        private void OnEnable()
        {
            stats.OnStatsChanged += SyncRigidbodyMass;
        }

        private void OnDisable()
        {
            stats.OnStatsChanged -= SyncRigidbodyMass;
        }

        private void Start()
        {
            SyncRigidbodyMass();
        }

        // 粉碎商店模块升级 ShipStats.Mass 后会广播 OnStatsChanged，这里跟着同步一次 rb.mass，
        // 避免"数值面板显示质量变了，但 Joint2D/碰撞动量等真实物理表现没变"的不一致。
        private void SyncRigidbodyMass()
        {
            rb.mass = stats.Mass;
        }

        /// <summary>拖拽质量查询：转发船锚当前拖拽的总质量；没连船锚时视为 0。</summary>
        private float DraggedMass => anchor != null ? anchor.TotalMass : 0f;

        private void FixedUpdate()
        {
            if (InputReader.Instance == null) return;

            Vector2 input = InputReader.Instance.MoveInput;
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            if (rb.velocity.magnitude >= stats.MaxSpeed) return;
            
            // 实际加速度 = 推力 /（飞船质量 + 拖拽质量），拖得越重、加速越慢。
            float totalMass = Mathf.Max(0.0001f, stats.Mass + DraggedMass);
            Vector2 acceleration = input * (stats.Thrust / totalMass);
            rb.AddForce(acceleration * rb.mass, ForceMode2D.Force);
        }

        public void ApplyUpgrade(ShipUpgradeType type, float delta)
        {
            stats.ApplyUpgrade(type, delta);
        }

        public void AddTemporaryThrust(float delta, float duration)
        {
            StartCoroutine(TemporaryThrustRoutine(delta, duration));
        }

        private IEnumerator TemporaryThrustRoutine(float amout, float duration)
        {
            ApplyUpgrade(ShipUpgradeType.Thrust, amout);
            yield return new WaitForSeconds(duration);
            ApplyUpgrade(ShipUpgradeType.Thrust, -amout);
        }
    }
}
