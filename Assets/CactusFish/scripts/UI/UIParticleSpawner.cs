using UnityEngine;

public class UIParticleSpawner : MonoBehaviour
{
    [Header("粒子样式库")]
    public Sprite[] particleSprites;   // 所有粒子样式，直接拖图即可

    [Header("生成参数")]
    public float spawnInterval = 0.2f; // 生成间隔，值越小粒子越密
    public int maxActiveCount = 100;   // 同屏最大粒子数
    public Vector2 lifeTimeRange = new Vector2(2f, 5f);  // 生命周期范围
    public Vector2 sizeRange = new Vector2(10f, 30f);    // 粒子大小范围
    public Vector2 speedXRange = new Vector2(-20f, 20f); // X轴速度范围
    public Vector2 speedYRange = new Vector2(-20f, 20f); // Y轴速度范围
    public float maxRotateSpeed = 30f; // 最大旋转速度
    public Color[] colorPool;          // 粒子颜色池，留空则默认白色

    private RectTransform _canvasRect;
    private float _spawnTimer;
    private int _currentActiveCount;

    private void Start()
    {
        // 获取Canvas尺寸，用于全屏位置计算
        _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        // 订阅粒子回收事件
        UIParticlePool.Instance.OnParticleRecycled += OnParticleRecycled;
    }

    private void Update()
    {
        _spawnTimer += Time.deltaTime;
        // 达到间隔且未超上限时生成粒子
        if (_spawnTimer >= spawnInterval && _currentActiveCount < maxActiveCount)
        {
            _spawnTimer = 0;
            SpawnOneRandomParticle();
        }
    }

    private void SpawnOneRandomParticle()
    {
        // 1. 从对象池获取粒子
        UIParticleItem particle = UIParticlePool.Instance.GetParticle();

        // 2. 随机全屏位置（适配任意分辨率和锚点）
        Vector2 randomScreenPos = new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            randomScreenPos,
            null,
            out Vector2 localPos
        );
        particle.GetComponent<RectTransform>().anchoredPosition = localPos;

        // 3. 随机样式
        Sprite randomSprite = particleSprites[Random.Range(0, particleSprites.Length)];

        // 4. 随机运动与外观参数
        float life = Random.Range(lifeTimeRange.x, lifeTimeRange.y);
        Vector2 speed = new Vector2(Random.Range(speedXRange.x, speedXRange.y), Random.Range(speedYRange.x, speedYRange.y));
        float rotate = Random.Range(-maxRotateSpeed, maxRotateSpeed);
        Color color = colorPool.Length > 0 ? colorPool[Random.Range(0, colorPool.Length)] : Color.white;

        // 5. 初始化并启动粒子
        particle.Init(randomSprite, life, speed, rotate, color, sizeRange);

        _currentActiveCount++;
    }

    private void OnParticleRecycled()
    {
        _currentActiveCount = Mathf.Max(0, _currentActiveCount - 1);
    }

    private void OnDestroy()
    {
        UIParticlePool.Instance.OnParticleRecycled -= OnParticleRecycled;
    }
}