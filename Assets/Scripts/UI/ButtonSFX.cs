using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonHover();
    }
}