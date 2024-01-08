using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Yarn.Unity;

[DefaultExecutionOrder(-1)]
public class SoundManager : MonoBehaviour {
    private static SoundManager Instance;

    [SerializeField] private List<AudioClip> clips;
    [SerializeField] private AudioMixerGroup output;
    [SerializeField] private float volume = 1f;

    private List<AudioSource> sources = new List<AudioSource>();

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    [YarnCommand("playSound")]
    public static void PlayFromList(int index, bool looped)
    {
        for (int i = 0; i < Instance.sources.Count; i++)
        {
            var source = Instance.sources[i];
            if (!source.isPlaying)
            {
                source.clip = Instance.clips[index];
                source.loop = looped;
                source.Play();
                return;
            }
        }

        var newSource = Instance.gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.outputAudioMixerGroup = Instance.output;
        newSource.clip = Instance.clips[index];
        newSource.volume = Instance.volume;
        newSource.loop = looped;
        newSource.Play();
        Instance.sources.Add(newSource);
    }

    [YarnCommand("stopSound")]
    public static void StopFromList(int index)
    {
        for (int i = 0; i < Instance.sources.Count; i++)
        {
            var source = Instance.sources[i];
            if (source.clip == Instance.clips[index] && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    public static void PlayEffect(AudioClip clip) {
        for (int i = 0; i < Instance.sources.Count; i++) {
            var source = Instance.sources[i];
            if (!source.isPlaying) {
                source.PlayOneShot(clip);
                return;
            }
        }

        // We didn't find a free AudioSource, create a new one
        var newSource = Instance.gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.outputAudioMixerGroup = Instance.output;
        newSource.PlayOneShot(clip);
        Instance.sources.Add(newSource);
    }

    static void PlayHintSound(InputAction.CallbackContext ctx)
    {
        PlayFromList(4, false);
    }

    private void OnEnable()
    {
        InputManager.Instance.Gameplay.Hint.performed += PlayHintSound;
    }

    private void OnDisable()
    {
        InputManager.Instance.Gameplay.Hint.performed -= PlayHintSound;
    }

}
