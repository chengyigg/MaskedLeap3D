using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("模式开关")]
    public bool isRotating = false;
    public bool isMoving = false;
    public bool isToggling = false;

    [Header("初始延迟（秒）")]
    [Tooltip("游戏开始后，等多少秒才开始动作。用于错开多个陷阱的节奏")]
    public float startDelay = 0f;

    [Header("旋转设置")]
    public Vector3 rotateSpeed = new Vector3(0, 100, 0);

    [Header("移动设置")]
    public Vector3 pointA = new Vector3(0, 1, 0);
    public Vector3 pointB = new Vector3(0, 1, 5);
    public float moveSpeed = 2f;

    [Header("显隐设置")]
    public float showDuration = 2f;
    public float hideDuration = 2f;
    [Tooltip("初始是否可见。勾上=先显示后隐藏；取消=先隐藏后显示")]
    public bool startVisible = true;

    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;
    private float toggleTimer = 0f;
    private bool isVisible = true;
    private float delayTimer = 0f;
    private bool started = false;
    private float moveStartTime = 0f;

    private HashSet<GameObject> hitCache = new HashSet<GameObject>();

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();

        // 初始化显隐状态
        isVisible = startVisible;
        SetVisibility(startVisible);

        delayTimer = startDelay;
        moveStartTime = Time.time + startDelay;
    }

    void Update()
    {
        // 【核心】延迟期间什么都不做
        if (!started)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer > 0f) return;
            started = true;
        }

        if (isRotating) transform.Rotate(rotateSpeed * Time.deltaTime);

        if (isMoving)
        {
            // 从"延迟结束"那一刻开始算移动进度，保证从 pointA 开始
            float t = Mathf.PingPong((Time.time - moveStartTime) * moveSpeed, 1f);
            transform.position = Vector3.Lerp(pointA, pointB, t);
        }

        if (isToggling)
        {
            toggleTimer += Time.deltaTime;

            if (isVisible)
            {
                if (toggleTimer >= showDuration)
                {
                    isVisible = false;
                    toggleTimer = 0f;
                    SetVisibility(false);
                }
            }
            else
            {
                if (toggleTimer >= hideDuration)
                {
                    isVisible = true;
                    toggleTimer = 0f;
                    SetVisibility(true);
                    hitCache.Clear();
                }
            }
        }
    }

    void SetVisibility(bool visible)
    {
        foreach (var r in meshRenderers) if (r != null) r.enabled = visible;
        foreach (var c in colliders) if (c != null) c.enabled = visible;
    }

    void OnTriggerEnter(Collider other) { HandleHit(other); }
    void OnTriggerStay(Collider other) { HandleHit(other); }

    void HandleHit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hitCache.Contains(other.gameObject)) return;
        hitCache.Add(other.gameObject);

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.Respawn();
            ParticleBurst.Play(transform.position, Color.red, 15, 3f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHurt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        hitCache.Remove(other.gameObject);
    }
}