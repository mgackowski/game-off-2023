using UnityEngine;

public class ImageEffect : MonoBehaviour
{
    [SerializeField] EvidenceImage parentImage;
    [SerializeField] float fadeTime = 0.5f;
    //ScreenFade fader;

    Renderer rend;

    void Awake()
    {
        /*fader = GetComponent<ScreenFade>();
        if (fader != null)
        {
            if (parentImage.Locked)
            {
                fader.FadeOut(0f);
            }
            else
            {
                fader.FadeIn(0f);
            }
        }*/

        rend = GetComponent<Renderer>();
        rend.forceRenderingOff = parentImage.Locked;
    }

    void OnParentLockStateChanged(bool locked)
    {
        /*if (fader != null)
        {
            if (locked)
            {
                fader.FadeOut(fadeTime);
            }
            else
            {
                fader.FadeIn(fadeTime);
            }
        }*/

        rend.forceRenderingOff = locked;
    }

    private void OnEnable()
    {
        parentImage.ImageLockStateChanged += OnParentLockStateChanged;
    }

    private void OnDisable()
    {
        parentImage.ImageLockStateChanged -= OnParentLockStateChanged;
    }
}
