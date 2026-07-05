using System.Collections.Generic;
using Game.Gameplay.Shop;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Gameplay.Anchor
{
    /// <summary>
    /// 船锚模块 —— 多锚链条的唯一协调者（核心玩法层，替代原来的 AnchorRelease）。
    /// 挂在链条头一节（直接连船的那一节）上，负责：
    ///   - 汇总整条链上每一节的拖拽质量，供 ShipController 查询；
    ///   - 命令链条末端追加新的一节（购买"锚头数量"升级卡后调用）；
    ///   - 统一处理空格/Shift+空格释放——只有这里订阅 InputReader 的释放事件，
    ///     避免链上每一节各自独立响应导致"按一次全松开"。
    /// </summary>
    [RequireComponent(typeof(AnchorController))]
    [RequireComponent(typeof(AnchorChainLink))]
    public class AnchorChainController : MonoBehaviour
    {
        private readonly List<AnchorController> anchors = new List<AnchorController>();
        private AnchorChainLink tipLink;
        private AnchorController firstAnchor;
        private bool shipInRange;

        /// <summary>查询：整条链当前拖拽的总质量（每一节各自锚定的小行星质量之和）。</summary>
        public float TotalDraggedMass
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < anchors.Count; i++)
                    total += anchors[i].TotalDraggedMass;
                return total;
            }
        }

        private void Awake()
        {
            firstAnchor = GetComponent<AnchorController>();
            anchors.Add(firstAnchor);
            tipLink = GetComponent<AnchorChainLink>();

            firstAnchor.GetComponent<AnchorStats>().OnHeadCountChanged += HandleAnchorHeadChanged;
        }

        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 InputReader.Instance 已就绪。
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed += HandleReleasePressed;
                InputReader.Instance.OnReleaseAllPressed += HandleReleaseAllPressed;
            }
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnReleasePressed -= HandleReleasePressed;
                InputReader.Instance.OnReleaseAllPressed -= HandleReleaseAllPressed;
            }
            firstAnchor.GetComponent<AnchorStats>().OnHeadCountChanged -= HandleAnchorHeadChanged;
        }

        /// <summary>命令：告知整条链"飞船当前是否在商店范围内"。调用方：ShopTriggerZone。</summary>
        public void SetShipInRange(bool inRange)
        {
            shipInRange = inRange;
        }

        /// <summary>
        /// 命令：在链条末端追加一节新锚。调用方：CrusherController（购买"锚头数量"效果卡后）。
        /// 失败（没配预制体，或预制体没挂对组件）时返回 null，调用方不需要特殊处理，只是这次升级没有实际长出新锚。
        /// </summary>
        public AnchorController AppendAnchor()
        {
            AnchorChainLink newLink = tipLink.SpawnNextLink(ComputeNextSpawnPosition());
            if (newLink == null) return null; // 没配 anchorPrefab，或预制体没挂 AnchorChainLink

            AnchorController newAnchor = newLink.GetComponent<AnchorController>();
            if (newAnchor == null)
            {
                Debug.LogWarning($"{nameof(AnchorChainController)} 生成的新锚没有 {nameof(AnchorController)} 组件，链条追加失败。");
                return null;
            }

            anchors.Add(newAnchor);
            tipLink = newLink;
            return newAnchor;
        }

        private void HandleAnchorHeadChanged(int delta)
        {
            for (int i = 0; i < delta; i++) AppendAnchor();
        }

        // 新的一节不能生成在当前链尾的正上方——两者之间没有拉力、也关掉了碰撞，会完全重叠到有外力拽开为止。
        // 沿着"上一节 -> 链尾"的方向往外延伸一个链节长度，让新锚一出生就自然排在链条延长线上；
        // 链尾就是链条头（没有上一节）时退化成固定方向。



    }
}
