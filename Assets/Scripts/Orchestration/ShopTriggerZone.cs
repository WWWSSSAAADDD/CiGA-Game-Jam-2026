using Game.Gameplay.Anchor;
using Game.Gameplay.Asteroid;
using Game.Gameplay.Shop;
using Game.Gameplay.Ship;
using Game.Infrastructure;
using UnityEngine;

namespace Game.Orchestration
{
    /// <summary>
    /// 粉碎商店触发模块（编排层）。
    /// 职责：专门负责调用粉碎机的TryCrushAsteroid、接收M按键输入打开shopPanel
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(Collider2D))]
    public class ShopTriggerZone : MonoBehaviour
    {

        [Header("商店页面 UI")]
        [Tooltip("Inspector 里拖商店页面的根物体（Canvas 下的 ShopPanel）进来。M 键在范围内按下时切换显示/隐藏；" +
                 "飞船离开范围时自动隐藏。留空则 M 键在这个商店什么都不会发生。")]
        [SerializeField] private GameObject shopPanel;

        // 挂这个脚本的 Collider2D 需要在 Inspector 里勾上 isTrigger，否则 OnTriggerEnter2D/Exit2D 不会触发。
        private bool shipInRange;

        private void Start()
        {
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnOpenShopPressed += HandleOpenShopUIPressed;
            }
        }

        private void OnDestroy()
        {
            if (InputReader.Instance != null)
            {
                InputReader.Instance.OnOpenShopPressed -= HandleOpenShopUIPressed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
                shipInRange = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<ShipController>() != null)
            {
                shipInRange = false;
                // 飞船离开商店范围时也不关闭商店UI页面
            }

            if (other.TryGetComponent(out AsteroidController asteroidController) && CrusherController.Instance != null)
            {
                CrusherController.Instance.TryCrushAsteroid(asteroidController);
            }
        }

        /// <summary>M 键：范围内切换商店页面的显示/隐藏。</summary>
        private void HandleOpenShopUIPressed()
        {
            if (!shipInRange) return;
            if (shopPanel == null)
            {
                return;
            }
            shopPanel.SetActive(!shopPanel.activeSelf);
        }
    }
}
