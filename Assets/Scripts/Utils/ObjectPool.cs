using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public PooledObject prefab;
    public int prewarmCount = 10;

    private Queue<PooledObject> available = new Queue<PooledObject>();
    private List<PooledObject> allObjects = new List<PooledObject>();

    void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError($"【对象池】{name} 的 prefab 槽位未赋值！");
            return;
        }
        for (int i = 0; i < prewarmCount; i++)
            CreateNew();
    }

    PooledObject CreateNew()
    {
        PooledObject obj = Instantiate(prefab, transform);
        obj.SourcePool = this;
        obj.gameObject.SetActive(false);
        available.Enqueue(obj);
        allObjects.Add(obj);
        return obj;
    }

    public PooledObject Get(Vector3 position, Quaternion rotation)
    {
        PooledObject obj;
        if (available.Count == 0) obj = CreateNew();
        else obj = available.Dequeue();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        obj.OnSpawn();
        return obj;
    }

    public void Return(PooledObject obj)
    {
        if (obj == null) return;
        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        available.Enqueue(obj);
    }
}