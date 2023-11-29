using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Yarn.Unity;

[DefaultExecutionOrder(-1)]
public class MusicManager : MonoBehaviour {
    private static MusicManager Instance;

    [SerializeField] AudioClip[] music;
    [SerializeField] AudioMixerGroup output;
    [SerializeField] float transitionTime = 1.0f;

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
        }
    }

    [YarnCommand("playMusic")]
    public static void PlayMusic()
    {
        for (int i = 0; i < Instance.music.Length; i++)
        {
            Instance.sources[i].Play();
        }
    }

    [YarnCommand("stopMusic")]
    public static void StopMusic()
    {
        for (int i = 0; i < Instance.music.Length; i++)
        {
            Instance.sources[i].Stop();
        }
    }

    /// <summary>
    /// Set the volume of the music layers based on a single intensity.
    /// The first clip is always at max volume and each other is gradually increased.
    /// </summary>
    /// <param name="intensity">0 to 1</param>
    [YarnCommand("setIntensity")]
    public static void SetIntensity(float intensity) {
        var clips = Instance.music.Length - 1;
        if (clips <= 0) {
            return;
        }

        var intensityPerTrack = 1f / clips;
        for (int i = 0; i < clips; i++) {
            if (intensity > 0) {
                var vol = intensity > intensityPerTrack ? 1 : intensity / intensityPerTrack;
                //Instance.sources[i + 1].volume = vol;
                Instance.StartCoroutine(fadeGradually(Instance.sources[i + 1], vol, Instance.transitionTime));
                intensity -= intensityPerTrack;
            } else {
                Instance.sources[i + 1].volume = 0;
            }
        }
    }

    static IEnumerator fadeGradually(AudioSource source, float targetVol, float transitionTime)
    {
        float timeElapsed = 0f;
        float startVol = source.volume;
        while (timeElapsed < transitionTime)
        {
            source.volume = Mathf.Lerp(startVol, targetVol, timeElapsed / transitionTime);
            yield return null;
            timeElapsed += Time.deltaTime;
        }
        source.volume = targetVol;
    }

}
