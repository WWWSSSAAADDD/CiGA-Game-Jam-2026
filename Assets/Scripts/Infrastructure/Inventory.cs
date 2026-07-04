using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Infrastructure
{
    /// <summary>
    /// 背包模块（基础设施层）。
    /// 职责：存资源/道具字典，暴露 AddResource()/AddItem()/TryConsumeItem() 命令方法；内部改完值后自己广播通知。
    /// 不知道"粉碎""升级""道具效果"这些具体玩法概念——只是一个通用的资源/道具存取服务。
    /// 单例（逻辑唯一）。
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        private readonly Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
        private readonly Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();

        /// <summary>资源变化通知：(资源种类, 变化后的总量)。订阅方：UI 模块（ResourceHUD）。</summary>
        public event Action<ResourceType, int> OnResourceChanged;

        /// <summary>道具数量变化通知：(道具, 变化后的数量)。当前暂无订阅方，未来道具 UI 可以接这个。</summary>
        public event Action<ItemData, int> OnItemChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"{nameof(Inventory)} 已存在一份单例实例，销毁多余的一份组件：{gameObject.name}");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>查询：某种资源当前的数量。</summary>
        public int GetResourceAmount(ResourceType type)
        {
            return resources.TryGetValue(type, out int amount) ? amount : 0;
        }

        /// <summary>
        /// 命令：增加资源（amount 传负数即扣除，结果不会低于 0）。
        /// 内部改完字典后自己广播 OnResourceChanged。
        /// </summary>
        public void AddResource(ResourceType type, int amount)
        {
            if (amount == 0) return;

            int updated = Mathf.Max(0, GetResourceAmount(type) + amount);
            resources[type] = updated;

            OnResourceChanged?.Invoke(type, updated);
        }

        /// <summary>查询：某件道具当前持有的数量。</summary>
        public int GetItemCount(ItemData item)
        {
            if (item == null) return 0;

            return items.TryGetValue(item, out int amount) ? amount : 0;
        }

        /// <summary>
        /// 命令：增加道具（amount 传负数即扣除，结果不会低于 0）。
        /// 内部改完字典后自己广播 OnItemChanged。
        /// </summary>
        public void AddItem(ItemData item, int amount)
        {
            if (item == null || amount == 0) return;

            int updated = Mathf.Max(0, GetItemCount(item) + amount);
            items[item] = updated;

            OnItemChanged?.Invoke(item, updated);
        }

        /// <summary>
        /// 命令：尝试消耗一件道具。数量不够时什么都不做，返回 false。
        /// 只负责"扣库存"，不负责触发道具效果——效果由调用方（ItemUseHandler）在消耗成功后自己调用 ItemData.Use()。
        /// </summary>
        public bool TryConsumeItem(ItemData item)
        {
            if (item == null) return false;
            if (GetItemCount(item) <= 0) return false;

            AddItem(item, -1);
            return true;
        }
    }
}
