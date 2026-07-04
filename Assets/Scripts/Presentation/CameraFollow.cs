using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// 相机模块（表现层）。
    /// 职责：LateUpdate 里插值跟随目标（飞船）。纯消费者——不发布任何事件、不做单例，
    /// 没有任何模块需要反过来引用相机。
    /// 通信方式：直接引用 target（Inspector 里拖飞船 Transform 进来），只做查询（读 target.position）。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("跟随目标，Inspector 里拖飞船（挂了 ShipController 的物体）的 Transform 进来。")]
        [SerializeField] private Transform target;

        [Tooltip("相机相对目标的偏移。2D 场景一般只需要保留负的 Z，让相机停在目标后方。")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Tooltip("跟随平滑速度，越大跟得越紧；<= 0 视为瞬间跟随（无插值）。")]
        [SerializeField] private float smoothSpeed = 8f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            transform.position = smoothSpeed <= 0f
                ? desiredPosition
                : Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
