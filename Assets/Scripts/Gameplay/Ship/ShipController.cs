using Game.Gameplay.Anchor;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Ship
{
    /// <summary>
    /// 飞船模块 —— 行为部分（核心玩法层）。
    /// 职责：读输入、AddForce 推动飞船。数值只读同物体上的 ShipStats（GetComponent），
    /// 不自己存推力/质量这些数值（XxxController + XxxStats 拆分模式）。
    ///
    /// 通信方式：
    ///   - 查询 InputReader.Instance.MoveInput 得到移动方向；
    ///   - 查询 <see cref="DraggedMass"/>，即整条船锚链当前拖拽的总质量。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(ShipStats))]
    public class ShipController : MonoBehaviour
    {
        [Tooltip("Inspector 里拖同一艘飞船挂着的船锚链条控制器进来（链条头一节上的 AnchorChainController），" +
                 "用来查询当前拖拽质量（自动覆盖整条链）。留空则视为拖拽质量恒为 0。")]
        [SerializeField] private AnchorChainController anchorChain;

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

        /// <summary>拖拽质量查询：转发整条船锚链当前拖拽的总质量；没连船锚链时视为 0。</summary>
        private float DraggedMass => anchorChain != null ? anchorChain.TotalDraggedMass : 0f;

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

            // Rigidbody2D.AddForce(ForceMode2D.Force) 内部会再除一次 rb.mass，
            // 这里乘回 rb.mass 抵消掉，使最终产生的加速度精确等于上面算出的 acceleration，
            // 不受 rb.mass 实际取值影响（即便以后 rb.mass 和 stats.Mass 不同步也不会算错）。
            rb.AddForce(acceleration * rb.mass, ForceMode2D.Force);
        }
    }
}
