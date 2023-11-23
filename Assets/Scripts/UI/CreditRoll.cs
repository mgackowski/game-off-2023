using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CreditRoll : MonoBehaviour
{
    [SerializeField] float speed;

    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        StartCoroutine(ScrollCredits());
    }

    IEnumerator ScrollCredits()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        float target = (rectTransform.sizeDelta.y / 2f) + 540f;
        pos.y = -target;
        rectTransform.anchoredPosition = pos;

        while (pos.y < target)
        {
            pos.y += speed * Time.deltaTime;
            rectTransform.anchoredPosition = pos;
            yield return null;
        }

        StartCoroutine(ScrollCredits());

    }
}
