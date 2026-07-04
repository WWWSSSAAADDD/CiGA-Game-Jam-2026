using Game.Infrastructure;
using TMPro;
using UnityEngine;

namespace Game.Presentation.UI
{
    /// <summary>
    /// 资源显示（表现层）。
    /// 职责：订阅背包模块的资源变化通知，把数字刷新到对应的文本控件上。
    /// 纯展示——只订阅，不发布，不会被任何模块直接引用。
    /// </summary>
    public class ResourceHUD : MonoBehaviour
    {
        [Header("三种资源各自的文本控件")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI metalText;
        [SerializeField] private TextMeshProUGUI iceText;
        [SerializeField] private TextMeshProUGUI crystalText;

        private void Start()
        {
            // 用 Start（而不是 OnEnable）订阅跨物体的单例事件：Unity 不保证不同物体的 Awake 先后顺序，
            // 但保证所有物体的 Awake 都执行完之后才会开始跑任何一个 Start，这里才能确定 Inventory.Instance 已就绪。
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnResourceChanged += HandleResourceChanged;

                // 事件只在数值变化时触发，进场时先主动查一次当前值，避免开局文本一直显示初始占位符。
                RefreshLabel(ResourceType.Gold, Inventory.Instance.GetResourceAmount(ResourceType.Gold));
                RefreshLabel(ResourceType.Metal, Inventory.Instance.GetResourceAmount(ResourceType.Metal));
                RefreshLabel(ResourceType.Ice, Inventory.Instance.GetResourceAmount(ResourceType.Ice));
                RefreshLabel(ResourceType.Crystal, Inventory.Instance.GetResourceAmount(ResourceType.Crystal));
            }
        }

        private void OnDestroy()
        {
            if (Inventory.Instance != null)
                Inventory.Instance.OnResourceChanged -= HandleResourceChanged;
        }

        private void HandleResourceChanged(ResourceType type, int amount)
        {
            RefreshLabel(type, amount);
        }

        private void RefreshLabel(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Gold:
                    if (goldText != null) goldText.text = amount.ToString();
                    break;
                case ResourceType.Metal:
                    if (metalText != null) metalText.text = amount.ToString();
                    break;
                case ResourceType.Ice:
                    if (iceText != null) iceText.text = amount.ToString();
                    break;
                case ResourceType.Crystal:
                    if (crystalText != null) crystalText.text = amount.ToString();
                    break;
            }
        }
    }
}
