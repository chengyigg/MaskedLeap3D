using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "游戏配置/玩家配置")]
public class PlayerConfig : ScriptableObject
{
    [Header("移动")]
    public float moveSpeed = 6f;
    [Range(0f, 1f)] public float airControlMultiplier = 0.6f;

    [Header("跳跃")]
    public float jumpHeight = 2f;
    public float doubleJumpHeight = 1.5f;   // 【新增】二段跳高度，比一段矮
    public float gravity = -20f;
    public int maxJumpCount = 2;

    [Header("手感")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    [Range(0f, 1f)] public float variableJumpMultiplier = 0.5f;

    [Header("冲刺")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
}