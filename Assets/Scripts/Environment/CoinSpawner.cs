using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public ObjectPool coinPool;
    public Transform[] spawnPoints;

    void Start()
    {
        foreach (var sp in spawnPoints)
            coinPool.Get(sp.position, sp.rotation);
    }
}