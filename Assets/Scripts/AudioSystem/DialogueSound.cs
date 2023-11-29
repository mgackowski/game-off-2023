using UnityEngine;

/**
 * Controls playback of an AudioSource
 */
[RequireComponent(typeof(AudioSource))]
public class DialogueSound : MonoBehaviour
{
    [SerializeField] float pitchVariance = 0f;
    AudioSource audioSource;
    float defaultPitch;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        defaultPitch = audioSource.pitch;
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.pitch = defaultPitch * (1f + Random.Range(-pitchVariance, pitchVariance));
            audioSource.Play();
        }
    }
}
