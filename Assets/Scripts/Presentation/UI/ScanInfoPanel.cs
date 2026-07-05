using Game.Gameplay.Anchor;
using Game.Gameplay.Asteroid;
using TMPro;
using UnityEngine;

namespace Game.Presentation.UI
{
    /// <summary>
    /// 扫描信息面板（表现层）。
    /// 职责：订阅船锚模块的锚定/释放通知，锚定成功时把小行星的硬度/质量/资源信息显示出来，
    /// 释放后隐藏面板。纯展示——只订阅，不发布，不会被任何模块直接引用。
    /// </summary>
    public class ScanInfoPanel : MonoBehaviour
    {
        [Tooltip("Inspector 里拖飞船身上的 AnchorController 进来（AnchorController 不是单例）。")]
        [SerializeField] private AnchorController anchor;

        [Header("面板与文本控件")]
        [Tooltip("整个面板的根物体，没锚到东西时隐藏。")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI hardnessText;
        [SerializeField] private TextMeshProUGUI massText;
        [SerializeField] private TextMeshProUGUI resourceText;

        private void OnEnable()
        {
            // anchor 是 Inspector 里直接拖进来的引用，不是单例查找，用 OnEnable/OnDisable 订阅是安全的
            // （道理同 HealthBar：不依赖 AnchorController 所在物体的 Awake 是否已经跑过）。
            if (anchor != null)
            {
                // TODO:在这里添加新的委托
                // anchor.OnAsteroidAnchored += HandleAsteroidAnchored;
                // anchor.OnAsteroidReleased += HandleAsteroidReleased;

                // 面板可能在锚已经带着小行星之后才被启用（比如某个 UI 层整体开关），
                // 这里主动查一次当前状态，而不是无条件假设"刚启用=空载"。
                if (anchor.AnchoredAsteroid != null)
                {
                    HandleAsteroidAnchored(anchor.AnchoredAsteroid);
                    return;
                }
            }

            SetPanelVisible(false);
        }

        private void OnDisable()
        {
            if (anchor != null)
            {
                // TODO:在这里添加新的委托
                //anchor.OnAsteroidAnchored -= HandleAsteroidAnchored;
                //anchor.OnAsteroidReleased -= HandleAsteroidReleased;
            }
        }

        private void HandleAsteroidAnchored(AsteroidController asteroid)
        {
            if (asteroid == null) return;

            ResourcePayload payload = asteroid.GetResourcePayload();

            if (hardnessText != null) hardnessText.text = asteroid.Hardness.ToString("0.0");
            if (massText != null) massText.text = asteroid.Mass.ToString("0.0");
            if (resourceText != null) resourceText.text = $"{payload.Type} x{payload.Amount}";

            SetPanelVisible(true);
        }

        private void HandleAsteroidReleased()
        {
            SetPanelVisible(false);
        }

        private void SetPanelVisible(bool visible)
        {
            if (panelRoot != null)
                panelRoot.SetActive(visible);
        }
    }
}
