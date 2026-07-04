using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// 链条视觉模块（表现层）。
    /// 职责：每帧读两个端点的 Transform，用 LineRenderer 画出连线。纯消费者——不发布任何事件、
    /// 不做单例，没有任何模块需要反过来引用它。
    /// 通信方式：直接引用 PointA/PointB（Inspector 拖拽，或由动态创建这段绳子的一方在生成时赋值），
    /// 只做查询，不做任何命令/通知。
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RopeVisual : MonoBehaviour
    {
        [Tooltip("绳子起点，一般是飞船的 Transform。")]
        [SerializeField] private Transform pointA;

        [Tooltip("绳子终点，一般是船锚的 Transform。")]
        [SerializeField] private Transform pointB;

        private LineRenderer lineRenderer;

        /// <summary>供动态生成这段绳子的一方（例如多锚串联时逐节创建的一方）在运行时赋值端点。</summary>
        public Transform PointA
        {
            get => pointA;
            set => pointA = value;
        }

        public Transform PointB
        {
            get => pointB;
            set => pointB = value;
        }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
        }

        private void LateUpdate()
        {
            if (pointA == null || pointB == null) return;

            lineRenderer.SetPosition(0, pointA.position);
            lineRenderer.SetPosition(1, pointB.position);
        }
    }
}
