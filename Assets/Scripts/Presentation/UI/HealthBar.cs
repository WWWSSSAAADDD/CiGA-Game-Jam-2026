using Game.Gameplay.Ship;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI
{
    /// <summary>
    /// 血量条（表现层）。
    /// 职责：订阅飞船数值模块的血量变化通知，把血量比例刷新到一个 Slider 上。
    /// 纯展示——只订阅，不发布，不会被任何模块直接引用。
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Tooltip("Inspector 里拖飞船身上的 ShipStats 进来（ShipStats 不是单例，场上可能不止一艘船）。")]
        [SerializeField] private ShipStats shipStats;

        [Tooltip("Inspector 里拖用来显示血量的 Slider 进来。")]
        [SerializeField] private Slider slider;

        private void OnEnable()
        {
            // shipStats 是 Inspector 里直接拖进来的引用，不是单例查找：
            // 只要这个字段本身不为空，订阅它的事件不依赖 ShipStats 所在物体的 Awake 是否已经跑过，
            // 用 OnEnable/OnDisable 是安全的（区别于跨物体单例事件那种必须等 Awake 跑完的情况）。
            if (shipStats != null)
            {
                shipStats.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(shipStats.Health, shipStats.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (shipStats != null)
                shipStats.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float health, float maxHealth)
        {
            if (slider == null) return;

            slider.maxValue = maxHealth;
            slider.value = health;
        }
    }
}
