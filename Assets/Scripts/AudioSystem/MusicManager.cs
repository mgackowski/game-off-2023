using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-1)]
public class MusicManager : MonoBehaviour {
    private static MusicManager Instance;

    [SerializeField] AudioClip[] music;
    [SerializeField] AudioMixerGroup output;

    private AudioSource[] sources;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        sources = new AudioSource[music.Length];
        for (int i = 0; i < music.Length; i++) {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].clip = music[i];
            sources[i].outputAudioMixerGroup = output;
            sources[i].volume = i == 0 ? 1 : 0;
            sources[i].loop = true;
            sources[i].Play();
        }
    }

    /// <summary>
    /// Set the volume of the music layers based on a single intensity.
    /// The first clip is always at max volume and each other is gradually increased.
    /// </summary>
    /// <param name="intensity">0 to 1</param>
    public static void SetIntensity(float intensity) {
        var clips = Instance.music.Length - 1;
        if (clips <= 0) {
            return;
        }

        var intensityPerTrack = 1f / clips;
        for (int i = 0; i < clips; i++) {
            if (intensity > 0) {
                var vol = intensity > intensityPerTrack ? 1 : intensity / intensityPerTrack;
                Instance.sources[i + 1].volume = vol;
                intensity -= intensityPerTrack;
            } else {
                Instance.sources[i + 1].volume = 0;
            }
        }
    }
}
