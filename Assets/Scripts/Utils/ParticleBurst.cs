using UnityEngine;

public static class ParticleBurst
{
    private static Material cachedMaterial;
    private static Texture2D cachedCircleTex;

    public static void Play(Vector3 position, Color color, int particleCount = 15, float speed = 3f)
    {
        GameObject go = new GameObject("ParticleBurst");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startLifetime = 0.4f;
        main.startSpeed = speed;
        main.startSize = 0.2f;
        main.startColor = color;
        main.gravityModifier = 0.5f;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.maxParticles = particleCount + 5;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        // 【关键】给粒子系统指定材质
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.material = GetOrCreateMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        ps.Play();
        ps.Emit(particleCount);

        Object.Destroy(go, 1.5f);
    }

    static Material GetOrCreateMaterial()
    {
        if (cachedMaterial != null) return cachedMaterial;

        // 尝试多个 shader，兼容 Built-in 和 URP
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            Debug.LogWarning("【ParticleBurst】找不到任何可用 shader，粒子会变成紫色方块。");
            return null;
        }

        cachedMaterial = new Material(shader);
        cachedMaterial.mainTexture = GetOrCreateCircleTexture();
        return cachedMaterial;
    }

    static Texture2D GetOrCreateCircleTexture()
    {
        if (cachedCircleTex != null) return cachedCircleTex;

        int size = 64;
        cachedCircleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        cachedCircleTex.wrapMode = TextureWrapMode.Clamp;
        cachedCircleTex.filterMode = FilterMode.Bilinear;

        float r = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(1f - (d / r)); // 边缘渐变
                a = a * a; // 更柔和的边缘
                cachedCircleTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        cachedCircleTex.Apply();
        return cachedCircleTex;
    }
}