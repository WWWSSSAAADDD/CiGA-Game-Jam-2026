using System;
using Game.Gameplay.Asteroid;
using UnityEngine;

namespace Game.Gameplay.Anchor
{
    /// <summary>
    /// 船锚模块 —— 行为部分（核心玩法层）。
    /// 职责：
    ///   - 启动时挂一根连到飞船的 DistanceJoint2D（maxDistanceOnly，只做最大距离约束），
    ///     之后船锚的所有摆动/甩动完全交给物理引擎解算，这里不写一行运动学代码。
    ///   - 碰撞时判定"穿透强度是否够、速度是否够"，够就命令小行星侧的 TryAnchor()。
    ///   - 暴露当前拖拽质量查询，供飞船模块计算加速度。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AnchorStats))]
    public class AnchorController : MonoBehaviour
    {
        [Tooltip("Inspector 里拖飞船（挂了 Rigidbody2D 的物体）进来，船锚靠这根 DistanceJoint2D 连在飞船后方。")]
        [SerializeField] private Rigidbody2D shipRigidbody;

        private Rigidbody2D anchorRigidbody;
        private AnchorStats stats;
        private AsteroidController anchoredAsteroid;

        public Rigidbody2D AnchorRigidbody => anchorRigidbody;
        public AsteroidController AnchoredAsteroid => anchoredAsteroid;

        /// <summary>查询：这一节自己拖拽的质量。单节最多同时拖一颗，没锚到东西时为 0。
        /// 整条链的总拖拽质量由 AnchorChainController 汇总，这里只负责自己这一节。</summary>
        public float TotalDraggedMass => anchoredAsteroid != null ? anchoredAsteroid.Mass : 0f;

        /// <summary>通知：锚定成功。订阅方：UI 模块（显示扫描信息等）。</summary>
        public event Action<AsteroidController> OnAsteroidAnchored;

        /// <summary>通知：释放/交付后恢复空载。</summary>
        public event Action OnAsteroidReleased;

        private void Awake()
        {
            anchorRigidbody = GetComponent<Rigidbody2D>();
            stats = GetComponent<AnchorStats>();
        }

        private void Start()
        {
            anchorRigidbody.mass = stats.Mass;
            SetupShipJoint();
        }

        private void SetupShipJoint()
        {
            if (shipRigidbody == null)
            {
                // 链条非头节点故意留空 shipRigidbody（这一节靠 AnchorChainLink 的 DistanceJoint2D 连上一节，
                // 不连船）——AnchorChainLink.SpawnNextLink() 会在这个物体的 Start() 跑之前就同步设好
                // previousLink，所以这里能可靠地区分"这是链条节点"还是"真的漏配了"，避免非头节点每次都刷警告。
                var chainLink = GetComponent<AnchorChainLink>();
                if (chainLink == null || chainLink.PreviousLink == null)
                    Debug.LogWarning($"{nameof(AnchorController)} 没有指定 {nameof(shipRigidbody)}，船锚不会跟随飞船。");

                return;
            }

            var joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = shipRigidbody;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
            joint.autoConfigureDistance = false; // 必须在设置 distance 之前关闭，否则 Unity 会用当前实际间距覆盖下面的赋值
            joint.distance = stats.ChainLength;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (anchoredAsteroid != null) return; // 这一节已经锚着东西时不再判定新的碰撞（每节独立，最多同时拖一颗）

            var asteroid = collision.collider.GetComponent<AsteroidController>();
            if (asteroid == null) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < stats.MinAnchorSpeed) return; // 速度不够，什么都不做，物理自然弹开
            if (stats.Penetration < asteroid.Hardness) return; // 强度不够，同样自然弹开

            if (asteroid.TryAnchor(anchorRigidbody))
            {
                anchoredAsteroid = asteroid;
                OnAsteroidAnchored?.Invoke(asteroid);
            }
        }

        /// <summary>
        /// 命令：释放当前锚定的小行星，恢复空载状态。
        /// 调用方：AnchorChainController（玩家主动按键，或商店粉碎结算完之后）。
        /// </summary>
        public bool ReleaseCurrentAsteroid()
        {
            if (anchoredAsteroid == null) return false;

            anchoredAsteroid.ReleaseAnchor();
            anchoredAsteroid = null;
            OnAsteroidReleased?.Invoke();
            return true;
        }
    }
}
