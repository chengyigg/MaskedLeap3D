using UnityEngine;

public class Coin : PooledObject
{
    [Header("ÊÓ¾õ")]
    public float rotateSpeed = 120f;
    public float floatAmplitude = 0.15f;
    public float floatFrequency = 2f;
    private Vector3 initialPos;

    [Header("´ÅÎü")]
    public float magnetRadius = 3f;
    public float magnetSpeed = 15f;
    private Transform playerTransform;
    private bool isMagnetized = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public override void OnSpawn()
    {
        initialPos = transform.position;
        isMagnetized = false;
        transform.rotation = Quaternion.identity;
    }

    public override void OnDespawn()
    {
        isMagnetized = false;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        float newY = initialPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (!isMagnetized && playerTransform != null)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) < magnetRadius)
                isMagnetized = true;
        }

        if (isMagnetized && playerTransform != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position + Vector3.up * 1f,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();
        GameEvents.CoinCollected();
        ReturnToPool();
    }
}