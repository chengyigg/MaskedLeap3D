using UnityEngine;
using UnityEngine.Pool;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public ObjectPool SourcePool;

    /// <summary>
    /// 物体从池中取出时调用，子类重写做初始化
    /// </summary>
    public virtual void OnSpawn() { }

    /// <summary>
    /// 物体放回池时调用，子类重写做清理
    /// </summary>
    public virtual void OnDespawn() { }

    /// <summary>
    /// 用完后调用这个把自己还回去
    /// </summary>
    public void ReturnToPool()
    {
        if (SourcePool != null)
            SourcePool.Return(this);
        else
            Destroy(gameObject);
    }
}