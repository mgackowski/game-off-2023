using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class ScreenFade : MonoBehaviour
{
    Image image;
    SpriteRenderer sprite;
    Mode mode;

    enum Mode
    {
        Image, SpriteRenderer
    }

    void Awake()
    {
        image = GetComponent<Image>();
        sprite = GetComponent<SpriteRenderer>();

        mode = image != null ? Mode.Image : Mode.SpriteRenderer;
    }

    [YarnCommand("fadeOut")]
    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, duration));
    }

    [YarnCommand("fadeIn")]
    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f, duration));
    }

    IEnumerator Fade(float from, float target, float duration)
    {
        Color newColor;
        if (mode == Mode.Image)
        {
            newColor = image.color;
        }
        else
        {
            newColor = sprite.color;
        }
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            newColor.a = Mathf.Lerp(from, target, timeElapsed / duration);
            if (mode == Mode.Image)
            {
                image.color = newColor;
            }
            else
            {
                sprite.color = newColor;
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        newColor.a = target;
        if (mode == Mode.Image)
        {
            image.color = newColor;
        }
        else
        {
            sprite.color = newColor;
        }
    }
}
