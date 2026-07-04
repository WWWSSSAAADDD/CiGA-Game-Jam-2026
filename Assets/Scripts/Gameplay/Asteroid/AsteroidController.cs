using Game.Gameplay.Anchor;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Asteroid
{
    /// <summary>
    /// 小行星粉碎后产出的资源载荷：种类 + 数量。
    /// </summary>
    public struct ResourcePayload
    {
        public ResourceType Type;
        public int Amount;

        public ResourcePayload(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    /// <summary>
    /// 小行星模块（核心玩法层）。
    /// 只存硬度/质量/资源类型数量这些数值，暴露 TryAnchor()/ReleaseAnchor() 命令方法和
    /// GetResourcePayload() 查询方法；外部不直接改内部字段。
    /// 场上会同时存在很多颗实例，不是单例。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AsteroidController : MonoBehaviour
    {
        [Header("物理数值")]
        [SerializeField] private float hardness = 1f;
        [SerializeField] private float mass = 1f;

        [Header("资源")]
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int resourceAmount = 10;

        private bool isAnchored;
        private AnchorController anchorController;  // 之前锚定该小行星的船锚
        private float nextAnchorAllowedTime;
        
        public float Hardness => hardness;
        public float Mass => mass;
        public bool IsAnchored => isAnchored;

        private void Awake()
        {
            GetComponent<Rigidbody2D>().mass = mass;
        }

        /// <summary>
        /// 命令：尝试被船锚锚定。调用方（AnchorController）负责先判定穿透强度/速度是否够，
        /// 这里只处理"锚定"这个动作本身：挂一个 FixedJoint2D 连到锚的刚体上。
        /// </summary>
        public bool TryAnchor(Rigidbody2D anchorRb)
        {
            if (isAnchored) return false;

            var joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = anchorRb;
            joint.autoConfigureConnectedAnchor = true;
            joint.enableCollision = false;

            isAnchored = true;
            anchorController = anchorRb.GetComponent<AnchorController>();
            return true;
        }

        /// <summary>命令：解除锚定，销毁挂在自己身上的 FixedJoint2D，恢复自由飘浮状态。</summary>
        public void ReleaseAnchor()
        {
            if (!isAnchored) return;

            var joint = GetComponent<FixedJoint2D>();
            if (joint != null)
                Destroy(joint);

            isAnchored = false;
            return;
        }

        public void CrushAsteroid()
        {
            // 更新船锚的状态
            if (anchorController != null)
            {
                anchorController.ReleaseCurrentAsteroid();
            }
            
            // 更新行星的状态
            Destroy(gameObject);
        }
        
        /// <summary>查询：粉碎这颗小行星能拿到的资源载荷。</summary>
        public ResourcePayload GetResourcePayload()
        {
            return new ResourcePayload(resourceType, resourceAmount);
        }
    }
}
