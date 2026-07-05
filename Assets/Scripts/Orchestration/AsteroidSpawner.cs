using System.Collections.Generic;
using Game.Gameplay.Shop;
using UnityEngine;

namespace Game.Orchestration
{
    /// <summary>
    /// 小行星生成模块（编排层）。
    /// 职责：按生成密度，在指定范围内持续 Instantiate 小行星实例。
    /// 不是实体本身——删掉它，场上已经生成的小行星还在，只是不会再有新的。
    /// </summary>
    public class AsteroidSpawner : MonoBehaviour
    {
        [Header("生成对象与范围")]
        [SerializeField] private GameObject asteroidPrefab;
        public skinLib skins;
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10f, -10f);
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(10f, 10f);

        [Header("生成密度")]
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxAliveCount = 20;

        [Header("难度曲线（每完成一次粉碎，生成节奏加快一点）")]
        [Tooltip("每次粉碎后，spawnInterval 缩短的量。")]
        [SerializeField] private float intervalReductionPerCrush = 0.05f;
        [Tooltip("spawnInterval 不会低于这个值，避免生成频率失控。")]
        [SerializeField] private float minSpawnInterval = 0.3f;
        [Tooltip("每完成一定次数粉碎，maxAliveCount 增加 1。")]
        [SerializeField] private int crushesPerExtraAliveCount = 5;

        private float timer;
        private readonly List<GameObject> spawned = new List<GameObject>();

        private void Start()
        {
            if (CrusherController.Instance != null) ;
            // CrusherController.Instance.OnCrushCompleted += HandleCrushCompleted;
        }

        private void OnDestroy()
        {
            if (CrusherController.Instance != null) ;
            // CrusherController.Instance.OnCrushCompleted -= HandleCrushCompleted;
        }

        private void Update()
        {
            spawned.RemoveAll(item => item == null);

            if (spawned.Count >= maxAliveCount) return;

            timer += Time.deltaTime;
            if (timer < spawnInterval) return;

            timer = 0f;
            SpawnOne();
        }

        private void SpawnOne()
        {
            if (asteroidPrefab == null) return;

            Vector2 position = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y));

            GameObject instance = Instantiate(asteroidPrefab, position, Quaternion.identity);
            instance.GetComponent<SpriteRenderer>().sprite = skins.sprites[Random.Range(0, skins.sprites.Count)];
            spawned.Add(instance);
        }
    }
}
