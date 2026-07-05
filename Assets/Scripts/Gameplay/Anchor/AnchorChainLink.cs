using Game.Presentation;
using UnityEngine;

namespace Game.Gameplay.Anchor
{
    /// <summary>
    /// 船锚模块 —— 多锚自关联部分（核心玩法层，P2）。
    /// 职责：在链条末端动态 Instantiate 下一节锚，把新锚用 DistanceJoint2D 接到"这一节"（不是飞船）身上，
    /// 并顺手创建这一节到下一节之间的绳子视觉。飞船算链条的第 0 节，不是 AnchorChainLink 实例。
    ///
    /// 层间依赖说明：这里直接引用并 Instantiate 表现层的 RopeVisual，是对"核心玩法层不依赖表现层"这条
    /// 一般规则的一个窄口径例外——因为"谁创建谁负责设两个端点"是本模块的核心职责本身（让 RopeVisual
    /// 永远不用知道链条现在有几节），不是可以绕开的实现细节。
    ///
    /// 挂这个组件的锚预制体，Inspector 里的 AnchorController.shipRigidbody 应该留空——链条节点不直接连船，
    /// 靠这里的 DistanceJoint2D 连上一节；只有链条头一节（直接连船的那一节）的 shipRigidbody 才需要填。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AnchorChainLink : MonoBehaviour
    {
        [Header("串联下一节要用到的预制体")]
        [Tooltip("Inspector 里拖锚的预制体进来（应同时挂了 AnchorController/AnchorStats/AnchorRelease/AnchorChainLink）。")]
        [SerializeField] private GameObject anchorPrefab;
        [Tooltip("Inspector 里拖链条视觉的预制体进来（挂了 RopeVisual）。")]
        [SerializeField] private GameObject ropeVisualPrefab;
        [Tooltip("新生成的一节到这一节之间的最大距离。")]
        [SerializeField] private float chainLinkLength = 4f;

        private Rigidbody2D selfRigidbody;
        
        private void Awake()
        {
            selfRigidbody = GetComponent<Rigidbody2D>();
        }
        
        internal AnchorController SpawnNextAnchor(Vector2 spawnPosition)
        {
            GameObject spawned = Instantiate(anchorPrefab, spawnPosition, Quaternion.identity);
            AnchorChainLink nextLink = spawned.GetComponent<AnchorChainLink>();

            DistanceJoint2D joint = spawned.AddComponent<DistanceJoint2D>();
            joint.connectedBody = selfRigidbody; // 连到"这一节"，不是飞船
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
            joint.autoConfigureDistance = false; // 必须在设置 distance 之前关闭，否则 Unity 会用实际生成间距覆盖 chainLinkLength
            joint.distance = chainLinkLength;
            

            // 这一段绳子视觉的生命周期跟这一节锚绑在一起，谁创建谁负责设两个端点。
            RopeVisual rope = Instantiate(ropeVisualPrefab).GetComponent<RopeVisual>();
            rope.PointA = transform;
            rope.PointB = nextLink.transform;

            AnchorController  anchorController = spawned.GetComponent<AnchorController>();
            anchorController.setIsFirstAnchor(false);
            return anchorController;
        }
    }
}
