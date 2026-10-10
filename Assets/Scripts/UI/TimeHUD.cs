using UnityEngine;
using TMPro;

public class TimeHUD : MonoBehaviour
{
    public TextMeshProUGUI timeHUDText;

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (timeHUDText == null) return;

        float t = GameManager.Instance.GetElapsedTime();
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        timeHUDText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}