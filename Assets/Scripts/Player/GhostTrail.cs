using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    public Transform modelRoot;
    public float spawnInterval = 0.04f;
    public float ghostLifetime = 0.4f;
    public Color ghostColor = new Color(0.7f, 0.9f, 1f, 0.6f);

    private PlayerController player;
    private float timer = 0f;

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (player == null || !player.IsDashing) return;
        if (modelRoot == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        GameObject ghost = new GameObject("Ghost");

        // 普通 MeshRenderer
        MeshRenderer[] meshRenderers = modelRoot.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            CreateGhostChild(ghost, mf.transform, mf.sharedMesh);
        }

        // SkinnedMeshRenderer（GLB 模型）
        SkinnedMeshRenderer[] skinnedRenderers = modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var smr in skinnedRenderers)
        {
            Mesh bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh, true);
            CreateGhostChild(ghost, smr.transform, bakedMesh);
        }

        GhostFade fade = ghost.AddComponent<GhostFade>();
        fade.lifetime = ghostLifetime;
        fade.startColor = ghostColor;
    }

    void CreateGhostChild(GameObject parent, Transform srcTransform, Mesh mesh)
    {
        GameObject child = new GameObject("GhostMesh");
        child.transform.SetParent(parent.transform, false);   // worldPositionStays = false

        // 【核心修复】把 child 的世界坐标对齐到源模型
        child.transform.position = srcTransform.position;
        child.transform.rotation = srcTransform.rotation;
        child.transform.localScale = srcTransform.lossyScale;

        MeshFilter mf = child.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = child.AddComponent<MeshRenderer>();

        Shader s = Shader.Find("Sprites/Default");
        if (s == null) s = Shader.Find("Unlit/Transparent");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");

        Material mat = new Material(s);
        if (mat.HasProperty("_Color")) mat.color = ghostColor;
        if (mat.HasProperty("_BaseColor")) mat.color = ghostColor;
        mr.material = mat;
    }
}