using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIParticleItem : MonoBehaviour
{
    private Image _particleImg;
    private RectTransform _rect;

    private float _totalLife;       // 总生命周期
    private float _currentLife;     // 当前存活时间
    private Vector2 _moveSpeed;     // XY方向移动速度
    private float _rotateSpeed;     // 旋转速度
    private Color _originColor;     // 初始颜色

    private void Awake()
    {
        _particleImg = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 初始化粒子所有参数
    /// </summary>
    public void Init(Sprite sprite, float lifeTime, Vector2 moveSpeed, float rotateSpeed, Color color, Vector2 sizeRange)
    {
        // 设置粒子样式
        _particleImg.sprite = sprite;
        _originColor = color;
        _particleImg.color = _originColor;

        // 随机大小
        float randomSize = Random.Range(sizeRange.x, sizeRange.y);
        _rect.sizeDelta = new Vector2(randomSize, randomSize);

        // 运动与生命周期参数
        _totalLife = lifeTime;
        _moveSpeed = moveSpeed;
        _rotateSpeed = rotateSpeed;
        _currentLife = 0;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _currentLife += Time.deltaTime;

        // 位移 + 旋转
        _rect.anchoredPosition += _moveSpeed * Time.deltaTime;
        _rect.Rotate(0, 0, _rotateSpeed * Time.deltaTime);

        // 生命周期进度 0~1
        float progress = _currentLife / _totalLife;

        // 透明度渐变：前10%淡入，后10%淡出
        float alpha = progress switch
        {
            < 0.1f => progress / 0.1f,
            > 0.9f => 1 - (progress - 0.9f) / 0.1f,
            _ => 1f
        };
        _particleImg.color = new Color(_originColor.r, _originColor.g, _originColor.b, _originColor.a * alpha);

        // 生命周期结束，回收到对象池
        if (_currentLife >= _totalLife)
        {
            UIParticlePool.Instance.ReturnParticle(this);
        }
    }
}