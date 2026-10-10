using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathFade : MonoBehaviour
{
    public static DeathFade Instance;

    public Image fadeImage;
    public float fadeDuration = 0.25f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public IEnumerator DeathSequence(System.Action onMidpoint)
    {
        yield return FadeTo(1f);
        onMidpoint?.Invoke();
        yield return new WaitForSeconds(0.1f);
        yield return FadeTo(0f);
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        Color finalC = fadeImage.color;
        finalC.a = targetAlpha;
        fadeImage.color = finalC;
    }
}