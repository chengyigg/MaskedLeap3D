using UnityEngine;

public class ChaseEnemy : PooledObject
{
    [Header("巡逻")]
    public Vector3 patrolPointA = new Vector3(0, 1, 0);
    public Vector3 patrolPointB = new Vector3(0, 1, 5);
    public float patrolSpeed = 1.5f;

    [Header("追击")]
    public float chaseSpeed = 3.5f;
    public float detectRange = 8f;
    public float returnSpeed = 3f;

    [Header("受击")]
    public float bounceForce = 10f;

    [Header("朝向")]
    [Tooltip("模型正面不是 +Z 时，用这个修正。Blender导入的通常填 180")]
    public float modelRotationOffset = 0f;
    public float turnSpeed = 10f;

    private enum State { Patrol, Chase, Return }
    private State state = State.Patrol;

    private Transform playerTransform;
    private float patrolT = 0f;

    public override void OnSpawn() { state = State.Patrol; patrolT = 0f; }
    public override void OnDespawn() { state = State.Patrol; patrolT = 0f; }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        switch (state)
        {
            case State.Patrol:
                if (dist < detectRange) state = State.Chase;
                else DoPatrol();
                break;
            case State.Chase:
                if (dist > detectRange * 1.2f) state = State.Return;
                else DoChase();
                break;
            case State.Return:
                DoReturn();
                break;
        }
    }

    void DoPatrol()
    {
        patrolT += patrolSpeed * Time.deltaTime * 0.5f;
        float t = Mathf.PingPong(patrolT, 1f);
        transform.position = Vector3.Lerp(patrolPointA, patrolPointB, t);

        bool goingForward = Mathf.FloorToInt(patrolT) % 2 == 0;
        Vector3 moveDir = goingForward ? (patrolPointB - patrolPointA) : (patrolPointA - patrolPointB);
        FaceDirection(moveDir);
    }

    void DoChase()
    {
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        dir.Normalize();
        transform.position += dir * chaseSpeed * Time.deltaTime;
        FaceDirection(dir);
    }

    void DoReturn()
    {
        float t = Mathf.PingPong(patrolT, 1f);
        Vector3 targetPatrolPos = Vector3.Lerp(patrolPointA, patrolPointB, t);
        Vector3 moveDir = targetPatrolPos - transform.position;
        moveDir.y = 0f;

        transform.position = Vector3.MoveTowards(transform.position, targetPatrolPos, returnSpeed * Time.deltaTime);

        if (moveDir.sqrMagnitude > 0.01f) FaceDirection(moveDir.normalized);

        if (Vector3.Distance(transform.position, targetPatrolPos) < 0.1f) state = State.Patrol;
    }

    void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized) *
                               Quaternion.Euler(0f, modelRotationOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        bool isFalling = pc.velocity.y <= 0;
        bool isAbove = other.transform.position.y > transform.position.y + 0.3f;

        if (isFalling && isAbove)
        {
            pc.ApplyBounce(bounceForce);
            ParticleBurst.Play(transform.position, Color.red, 20, 3f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyStomp();
            ReturnToPool();
        }
        else pc.Respawn();
    }
}