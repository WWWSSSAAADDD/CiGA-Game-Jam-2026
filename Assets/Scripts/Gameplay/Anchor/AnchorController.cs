using System;
using Game.Gameplay.Asteroid;
using Game.Infrastructure;
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
        private AnchorStats anchorStats;
        private AnchorChainLink anchorChainLink;
        private AsteroidController anchoredAsteroid;
        private bool isFirstAnchor = true;
        public void setIsFirstAnchor(bool value) { isFirstAnchor = value; }
        
        public Rigidbody2D AnchorRigidbody => anchorRigidbody;
        public AsteroidController AnchoredAsteroid => anchoredAsteroid;

        private AnchorController nextLinkAnchor;
        public float TotalMass
        {
            get
            {
                float res = anchorStats.Mass;
                if (anchoredAsteroid != null) res += anchoredAsteroid.Mass;
                if (nextLinkAnchor != null) res += nextLinkAnchor.TotalMass;
                return res;
            }
        }
        
        private void Awake()
        {
            anchorRigidbody = GetComponent<Rigidbody2D>();
            anchorStats = GetComponent<AnchorStats>();
            anchorChainLink = GetComponent<AnchorChainLink>();
        }

        private void Start()
        {
            // TODO: 检查rb的mass
            anchorRigidbody.mass = anchorStats.Mass;

            if (!isFirstAnchor) return;
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed += ReleaseCurrentAsteroid;
                //InputReader.Instance.OnUseItemPressed += SpawnNextAnchor;
            }
            SetupShipJoint();
        }

        private void OnDestroy()
        {
            if (!isFirstAnchor) return;
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed -= ReleaseCurrentAsteroid;
                //InputReader.Instance.OnUseItemPressed -= SpawnNextAnchor;
            }
        }

        private void SetupShipJoint()
        {
            var joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = shipRigidbody;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
            joint.autoConfigureDistance = false; // 必须在设置 distance 之前关闭，否则 Unity 会用当前实际间距覆盖下面的赋值
            joint.distance = anchorStats.ChainLength;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 已经锚定小行星了则不再锚定
            if (anchoredAsteroid != null) return;

            var asteroid = collision.collider.GetComponent<AsteroidController>();
            if (asteroid == null) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < anchorStats.MinAnchorSpeed) return; // 速度不够，什么都不做，物理自然弹开
            if (anchorStats.Penetration < asteroid.Hardness) return; // 强度不够，同样自然弹开

            if (asteroid.TryAnchor(anchorRigidbody))
            {
                anchoredAsteroid = asteroid;
            }
        }

        public void ReleaseCurrentAsteroid()
        {
            ReleaseCurrentAsteroidHelper();
        }
        
        // 按空格触发，释放锚定的小行星
        private bool ReleaseCurrentAsteroidHelper()  // Release成功则返回true
        {
            // 先尝试Release末尾的Link
            if (nextLinkAnchor != null && nextLinkAnchor.ReleaseCurrentAsteroidHelper()) return true;
            
            // 否则尝试Release自身的
            if (anchoredAsteroid == null) return false;
            anchoredAsteroid.ReleaseAnchor();
            anchoredAsteroid = null;
            return true;
        }
        
        public void SpawnNextAnchor()
        {
            if (nextLinkAnchor != null) nextLinkAnchor.SpawnNextAnchor();
            else nextLinkAnchor = anchorChainLink.SpawnNextAnchor(transform.position);
        }

        public void ApplyUpgrade(AnchorUpgradeType type, float delta)
        {
            anchorStats.ApplyUpgrade(type, delta);
            if (type == AnchorUpgradeType.Mass)
                anchorRigidbody.mass = anchorStats.Mass;
        }
    }
}
