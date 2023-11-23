using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class ScreenFade : MonoBehaviour
{
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    [YarnCommand("fadeOut")]
    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, duration));
    }

    [YarnCommand("fadeIn")]
    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, duration));
    }

    IEnumerator Fade(float target, float duration)
    {
        Color newColor = image.color;
        float initialAlpha = newColor.a;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            newColor.a = Mathf.Lerp(initialAlpha, target, timeElapsed / duration);
            image.color = newColor;

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        newColor.a = target;
        image.color = newColor;
    }
}
