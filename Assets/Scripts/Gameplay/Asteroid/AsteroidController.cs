using Game.Gameplay.Anchor;
using Game.Infrastructure;
using UnityEngine;
using UnityEngine.Serialization;

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
    /// 职责：任何外部模块与小行星模块交互，都通过AsteroidController接口
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

        [FormerlySerializedAs("reanchoredAllowedTime")] 
        [SerializeField] private float reanchoredCooldown = 1;
        
        private AsteroidAbility asteroidAbility;
        
        private bool isAnchored;
        private AnchorController anchorController;  // 之前锚定该小行星的船锚
        private float nextAnchorAllowedTime;
        
        public float Hardness => hardness;
        public float Mass => mass;
        public bool IsAnchored => isAnchored;

        private void Awake()
        {
            GetComponent<Rigidbody2D>().mass = mass;
            asteroidAbility = GetComponent<AsteroidAbility>();
        }
        
        public bool TryAnchor(Rigidbody2D anchorRb)
        {
            if (isAnchored) return false;
            if (Time.time < nextAnchorAllowedTime) return false;
            
            var joint = gameObject.AddComponent<FixedJoint2D>();
            joint.connectedBody = anchorRb;
            joint.autoConfigureConnectedAnchor = true;
            joint.enableCollision = false;

            isAnchored = true;
            anchorController = anchorRb.GetComponent<AnchorController>();
            asteroidAbility?.HandleAsteroidAnchored();
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
            anchorController = null;
            asteroidAbility?.HandleAsteroidUnanchored();
            nextAnchorAllowedTime = Time.time + reanchoredCooldown;
            return;
        }

        public void EscapeFromAnchor()
        {
            if (anchorController != null)
            {
                anchorController.ReleaseCurrentAsteroid();
            }
        }
        
        public void CrushAsteroid()
        {
            // 更新当前船锚的状态
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
