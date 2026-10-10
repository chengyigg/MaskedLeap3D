using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialHint : MonoBehaviour
{
    public TextMeshProUGUI hintText;
    public float displayDuration = 4f;
    public float fadeDuration = 1f;

    void Start()
    {
        if (hintText == null) return;
        hintText.text = "WASD Move   Space Jump   Shift Dash   Mouse Look";
        hintText.color = new Color(hintText.color.r, hintText.color.g, hintText.color.b, 1f);
        StartCoroutine(ShowAndFade());
    }

    IEnumerator ShowAndFade()
    {
        yield return new WaitForSeconds(displayDuration);

        float t = 0f;
        Color c = hintText.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            hintText.color = c;
            yield return null;
        }
        hintText.gameObject.SetActive(false);
    }
}