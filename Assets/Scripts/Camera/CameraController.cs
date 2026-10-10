using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("引用设置")]
    public Transform player;
    public Transform cameraTransform;
    public PlayerController playerController;
    public Camera cam;

    [Header("灵敏度")]
    public float mouseSensitivityX = 150f;
    public float mouseSensitivityY = 150f;
    public float scrollSpeed = 10f;

    [Header("视角限制")]
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float minZoom = -2f;
    public float maxZoom = -10f;

    [Header("跟随")]
    public Vector3 pivotOffset = new Vector3(0f, 2.5f, 0f);

    [Header("FOV 冲刺")]
    public float normalFOV = 60f;
    public float dashFOV = 75f;
    public float fovLerpSpeed = 8f;

    [Header("落地微震")]
    public float shakeAmount = 0.15f;
    public float shakeDuration = 0.15f;

    private float xRotation = 0f;
    private float shakeTimer = 0f;
    private bool wasGrounded = true;
    private Vector3 baseCameraPos;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraTransform != null)
        {
            xRotation = cameraTransform.localEulerAngles.x;
            baseCameraPos = cameraTransform.localPosition;
        }
        if (cam == null && cameraTransform != null)
            cam = cameraTransform.GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0f) return;
        if (player != null)
            transform.position = player.position + pivotOffset;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        transform.Rotate(Vector3.up * mouseX);

        if (cameraTransform != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // 计算位置
            Vector3 cameraPos = baseCameraPos;
            cameraPos.z += scroll * scrollSpeed;
            cameraPos.z = Mathf.Clamp(cameraPos.z, maxZoom, minZoom);
            baseCameraPos.z = cameraPos.z; // 记录缩放后的位置

            // 微震
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float t = shakeTimer / shakeDuration;
                cameraPos.y -= shakeAmount * t;
            }

            cameraTransform.localPosition = cameraPos;
        }

        // 落地检测
        if (playerController != null)
        {
            bool grounded = playerController.IsGrounded;
            if (!wasGrounded && grounded)
                shakeTimer = shakeDuration;
            wasGrounded = grounded;
        }

        // FOV
        if (cam != null && playerController != null)
        {
            float targetFOV = playerController.IsDashing ? dashFOV : normalFOV;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }
    }
}