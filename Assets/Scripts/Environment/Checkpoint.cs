using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnPoint;
    public Color activatedColor = Color.green;
    public bool oneTime = false;
    private bool isActivated = false;
    private Renderer[] renderers;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneTime && isActivated) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.spawnPoint = respawnPoint != null ? respawnPoint : transform;

        if (!isActivated)
        {
            isActivated = true;
            foreach (var r in renderers)
                if (r.material != null) r.material.color = activatedColor;

            ParticleBurst.Play(transform.position + Vector3.up * 0.5f, activatedColor, 25, 4f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCheckpoint();
        }
    }
}