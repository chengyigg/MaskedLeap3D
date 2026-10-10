using UnityEngine;

public class GoalVFX : MonoBehaviour
{
    [Header("脉冲发光")]
    public Color colorA = new Color(0.2f, 1f, 0.5f); // 暗绿
    public Color colorB = new Color(0.6f, 1f, 0.8f); // 亮绿
    public float pulseSpeed = 2f;

    [Header("粒子")]
    public float particleInterval = 0.4f;
    public Color particleColor = new Color(0.3f, 1f, 0.6f);

    private Renderer[] renderers;
    private float particleTimer = 0f;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        // 颜色脉冲：A 和 B 之间来回插值
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        Color currentColor = Color.Lerp(colorA, colorB, t);

        foreach (var r in renderers)
        {
            if (r.material != null)
            {
                r.material.color = currentColor;
                // 如果材质支持发光，顺便控制发光颜色
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.SetColor("_EmissionColor", currentColor * 1.5f);
                    r.material.EnableKeyword("_EMISSION");
                }
            }
        }

        // 定时粒子
        particleTimer += Time.deltaTime;
        if (particleTimer >= particleInterval)
        {
            particleTimer = 0f;
            // 在终点方块上方随机位置冒粒子
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            ParticleBurst.Play(spawnPos, particleColor, 8, 2f);
        }
    }
}