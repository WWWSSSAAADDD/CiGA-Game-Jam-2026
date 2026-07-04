using System.Collections.Generic;
using UnityEngine;
using System;

public class UIParticlePool : MonoBehaviour
{
    public static UIParticlePool Instance { get; private set; }

    [Header("基础配置")]
    public UIParticleItem particlePrefab;  // 粒子预制体
    public Transform container;            // 粒子父容器（控制UI层级）
    public int preloadCount = 30;          // 预加载粒子数量

    // 粒子回收事件，供生成器计数用
    public event Action OnParticleRecycled;

    private readonly Queue<UIParticleItem> _pool = new Queue<UIParticleItem>();

    private void Awake()
    {
        Instance = this;
        // 预填充对象池
        for (int i = 0; i < preloadCount; i++)
        {
            UIParticleItem particle = CreateNewParticle();
            particle.gameObject.SetActive(false);
            _pool.Enqueue(particle);
        }
    }

    private UIParticleItem CreateNewParticle()
    {
        return Instantiate(particlePrefab, container);
    }

    /// <summary>
    /// 从池中取出一个可用粒子
    /// </summary>
    public UIParticleItem GetParticle()
    {
        return _pool.Count > 0 ? _pool.Dequeue() : CreateNewParticle();
    }

    /// <summary>
    /// 归还用完的粒子到池中
    /// </summary>
    public void ReturnParticle(UIParticleItem particle)
    {
        particle.gameObject.SetActive(false);
        _pool.Enqueue(particle);
        OnParticleRecycled?.Invoke();
    }
}