using UnityEngine;

/**
 * Controls sound playback in response to a Scanner.
 */
[RequireComponent(typeof(AudioSource))]
public class ScannerAudio : MonoBehaviour
{
    [SerializeField] Scanner eventSource;

    [SerializeField] AudioClip zoomClip;
    [SerializeField] AudioClip scanClip;
    [SerializeField] AudioClip enhanceClip;
    [SerializeField] AudioClip successClip;
    [SerializeField] AudioClip failClip;

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

    public void PlaySuccessSound()
    {
        audioSource.clip = successClip;
        audioSource.Play();
    }

    public void PlayFailSound()
    {
        audioSource.clip = failClip;
        audioSource.Play();
    }

    void OnZoomChanged(float newZoom)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.clip = zoomClip;
            audioSource.Play();
        }
    }

    void OnScanPerformed(Scanner.ScanEventArgs args)
    {
        if (args.userPerformedScan)
        {
            audioSource.clip = scanClip;
            audioSource.Play();
        }
    }

    void OnEnhancePerformed(Scanner.EnhanceEventArgs args)
    {
        audioSource.clip = enhanceClip;
        audioSource.Play();
    }

    private void OnEnable()
    {
        eventSource.ZoomLevelChanged += OnZoomChanged;
        eventSource.ScanPerformed += OnScanPerformed;
        eventSource.EnhancePerformed += OnEnhancePerformed;
    }

    private void OnDisable()
    {
        eventSource.ZoomLevelChanged -= OnZoomChanged;
        eventSource.ScanPerformed -= OnScanPerformed;
        eventSource.EnhancePerformed -= OnEnhancePerformed;
    }
}
