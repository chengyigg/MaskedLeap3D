using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject winPanel;
    public TextMeshProUGUI timeText;   // 【新增】通关面板里的时间文字

    private float elapsedTime = 0f;
    private bool isTimerRunning = true;
    public float GetElapsedTime() => elapsedTime;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        if (isTimerRunning)
            elapsedTime += Time.deltaTime;
    }

    public void WinGame()
    {
        // 【核心】停止计时，把最终时间显示出来
        isTimerRunning = false;

        if (winPanel != null) winPanel.SetActive(true);
        if (timeText != null) timeText.text = "用时: " + FormatTime(elapsedTime);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinTrigger();
            AudioManager.Instance.PlayWinFanfare();
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 把秒数格式化成 "01:23.45"
    string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((totalSeconds * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, centiseconds);
    }
}