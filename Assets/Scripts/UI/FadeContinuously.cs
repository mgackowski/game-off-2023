using UnityEngine;
using UnityEngine.UI;

/**
 * A simple script to make a UI Image flash using a sine function.
 */
[RequireComponent(typeof(Image))]
public class FadeContinuously : MonoBehaviour
{
    [SerializeField] float frequency = 1f;

    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }
    void Update()
    {
        Color newColor = image.color;
        newColor.a = (Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f;
        image.color = newColor;
    }
}
