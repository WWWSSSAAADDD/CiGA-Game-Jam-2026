using System;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// 输入模块（基础设施层）。
    /// 职责：每帧读取原始硬件输入，转成属性（查询）和"按下瞬间"事件（通知）。
    /// 不知道、也不应该知道"飞船""船锚"这些具体玩法概念——把这些词从这个类里划掉，它还能正常工作。
    ///
    /// 单例（逻辑唯一）。
    /// 通信方式：
    ///   - <see cref="MoveInput"/> 被查询（例如 ShipController 在 FixedUpdate 里读）。
    ///   - 三个按键事件各自被订阅（例如 AnchorRelease / ShopTriggerZone / ItemUseHandler）。
    /// 场景里只应该挂一份（建议挂在一个常驻的 Bootstrap/Managers 物体上）。
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        public static InputReader Instance { get; private set; }

        /// <summary>当前帧的移动输入方向（未归一化，X/Y 各自落在 [-1, 1]）。查询用，只读。</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>空格键按下的瞬间触发一次——对应"释放船锚"。订阅方：AnchorRelease。</summary>
        public event Action OnReleasePressed;

        /// <summary>M 键按下的瞬间触发一次——对应"打开商店"。订阅方：ShopTriggerZone。</summary>
        public event Action OnOpenShopPressed;

        /// <summary>J 键按下的瞬间触发一次——对应"使用道具"。订阅方：ItemUseHandler。</summary>
        public event Action OnUseItemPressed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{nameof(InputReader)} 已存在一份单例实例，销毁多余的一份组件：{gameObject.name}");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (Input.GetKeyDown(KeyCode.Space))
                OnReleasePressed?.Invoke();

            if (Input.GetKeyDown(KeyCode.M))
                OnOpenShopPressed?.Invoke();

            if (Input.GetKeyDown(KeyCode.J))
                OnUseItemPressed?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
