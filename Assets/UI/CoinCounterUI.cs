using UnityEngine;
using TMPro;
using System.Collections;

public class CoinCounterUI : MonoBehaviour
{
    public static CoinCounterUI Instance;

    public TextMeshProUGUI coinText;
    public int totalCoins = 10;
    private int currentCoins = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GameEvents.OnCoinCollected += HandleCoinCollected;
    }

    void OnDisable()
    {
        GameEvents.OnCoinCollected -= HandleCoinCollected;
    }

    void Start()
    {
        UpdateText();
    }

    void HandleCoinCollected()
    {
        currentCoins++;
        UpdateText();
        if (coinText != null) StartCoroutine(BounceEffect());
    }

    void UpdateText()
    {
        if (coinText != null)
            coinText.text = $"{currentCoins} / {totalCoins}";
    }

    IEnumerator BounceEffect()
    {
        if (coinText == null) yield break;
        RectTransform rt = coinText.rectTransform;
        Vector3 originalScale = rt.localScale;
        rt.localScale = originalScale * 1.35f;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(originalScale * 1.35f, originalScale, t / 0.15f);
            yield return null;
        }
        rt.localScale = originalScale;
    }
}