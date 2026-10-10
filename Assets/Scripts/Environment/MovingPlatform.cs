using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 pointA = new Vector3(0, 0, 0);
    public Vector3 pointB = new Vector3(0, 0, 10);
    public float speed = 2f;

    // 供玩家读取的当前平台速度
    [HideInInspector] public Vector3 platformVelocity;

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        Vector3 targetPos = Vector3.Lerp(pointA, pointB, t);

        // 核心：计算这一帧平台移动的速度
        if (Time.deltaTime > 0f)
        {
            platformVelocity = (targetPos - transform.position) / Time.deltaTime;
        }

        transform.position = targetPos;
    }
}