namespace Game.Infrastructure
{
    /// <summary>
    /// 资源种类。小行星粉碎后产出的资源类型，同时也是背包模块存资源用的 key。
    /// 纯数据、不含任何行为，小行星模块和背包模块都直接复用这一个枚举。
    /// </summary>
    public enum ResourceType
    {
        Gold,
        Metal,
        Ice,
        Crystal
    }
}
