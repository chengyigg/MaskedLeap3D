using UnityEngine;

public class Enemy : PooledObject
{
    [Header("巡逻设置")]
    public Vector3 pointA = new Vector3(0, 1, 5);
    public Vector3 pointB = new Vector3(0, 1, 10);
    public float speed = 2f;

    [Header("受击设置")]
    public float bounceForce = 10f;

    [Header("朝向")]
    [Tooltip("模型正面不是 +Z 时，用这个修正。Blender导入的通常填 180")]
    public float modelRotationOffset = 0f;
    public float turnSpeed = 10f; // 转身平滑度

    private float patrolT = 0f;

    public override void OnSpawn() { patrolT = 0f; }
    public override void OnDespawn() { patrolT = 0f; }

    void Update()
    {
        patrolT += speed * Time.deltaTime * 0.5f;
        float t = Mathf.PingPong(patrolT, 1f);
        transform.position = Vector3.Lerp(pointA, pointB, t);

        // 判断当前是正向还是反向移动
        bool goingForward = Mathf.FloorToInt(patrolT) % 2 == 0;
        Vector3 moveDir = goingForward ? (pointB - pointA) : (pointA - pointB);
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized) *
                                   Quaternion.Euler(0f, modelRotationOffset, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        bool isFalling = pc.velocity.y <= 0;
        bool isAboveEnemy = other.transform.position.y > transform.position.y + 0.1f;

        if (isFalling && isAboveEnemy)
        {
            pc.ApplyBounce(bounceForce);
            ParticleBurst.Play(transform.position, Color.red, 20, 3f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyStomp();
            ReturnToPool();
        }
        else pc.Respawn();
    }
}