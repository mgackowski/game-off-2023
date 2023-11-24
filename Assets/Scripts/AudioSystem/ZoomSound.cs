using UnityEngine;

/**
 * Controls zooming sound in response to a Scanner.
 */
[RequireComponent(typeof(AudioSource))]
public class ZoomSound : MonoBehaviour
{
    [SerializeField] Scanner eventSource;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Reset()
    {
        if (eventSource == null)
        {
            eventSource = GameObject.FindGameObjectWithTag("Scanner")
                .GetComponent<Scanner>();
        }
    }

    void OnZoomChanged(float newZoom)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        eventSource.ZoomLevelChanged += OnZoomChanged;
    }
}
