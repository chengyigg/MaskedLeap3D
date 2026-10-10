using UnityEngine;

public class GhostFade : MonoBehaviour
{
    public float lifetime = 0.4f;
    public Color startColor = new Color(0.7f, 0.9f, 1f, 0.6f);

    private float timer = 0f;
    private MeshRenderer[] renderers;
    private Material[] mats;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            mats[i] = renderers[i].material;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;
        if (t >= 1f) { Destroy(gameObject); return; }

        float alpha = Mathf.Lerp(startColor.a, 0f, t);
        Color c = startColor;
        c.a = alpha;

        foreach (var m in mats)
        {
            if (m == null) continue;
            if (m.HasProperty("_Color")) m.color = c;
            if (m.HasProperty("_BaseColor")) m.color = c;
        }
    }
}