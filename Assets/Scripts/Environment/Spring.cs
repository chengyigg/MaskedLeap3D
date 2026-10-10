using UnityEngine;

public class Spring : MonoBehaviour
{
    public float bounceForce = 15f;

    [Header("Ñ¹Ëõ¶¯»­")]
    public float compressAmount = 0.4f;
    public float compressDuration = 0.15f;

    private Vector3 originalScale;
    private float compressTimer = 0f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (compressTimer > 0f)
        {
            compressTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(compressTimer / compressDuration);
            float squashY = 1f - compressAmount * t;
            float stretchXZ = 1f + compressAmount * 0.4f * t;
            transform.localScale = new Vector3(
                originalScale.x * stretchXZ,
                originalScale.y * squashY,
                originalScale.z * stretchXZ
            );
        }
        else transform.localScale = originalScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ApplyBounce(bounceForce);
            compressTimer = compressDuration;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySpring();
        }
    }
}