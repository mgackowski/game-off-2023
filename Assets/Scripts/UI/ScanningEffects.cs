using System;
using System.Collections;
using UnityEngine;

public class ScanningEffects : MonoBehaviour
{
    public event Action<Scanner.ScanEventArgs> ScanAnimationFinished;
    public event Action<Scanner.EnhanceEventArgs> EnhanceAnimationFinished;
    public event Action<Scanner.ScanEventArgs> FinishedWithoutAnimation;

    [SerializeField] Scanner scanner;

    [SerializeField] GameObject scanEffect;
    [SerializeField] ParticleSystem scanParticles;
    [SerializeField] float scanEffectDuration = 0.5f;
    [SerializeField] GameObject enhanceEffect;
    [SerializeField] ParticleSystem enhanceParticles;
    [SerializeField] float enhanceEffectDuration = 1.5f;

    void OnScanPerformed(Scanner.ScanEventArgs args)
    {
        if (!args.userPerformedScan)
        {
            FinishedWithoutAnimation?.Invoke(args);
            return;
        }
        StartCoroutine(DisplayScanEffect(scanEffectDuration, args));
    }

    void OnEnhancePerformed(Scanner.EnhanceEventArgs args)
    {
        StartCoroutine(DisplayEnhanceEffect(enhanceEffectDuration, args));
    }

    IEnumerator DisplayScanEffect(float duration, Scanner.ScanEventArgs onCompleteArgs)
    {
        scanEffect.SetActive(true);
        scanParticles.Play();
        yield return new WaitForSeconds(duration);
        scanParticles.Stop();
        scanEffect.SetActive(false);
        ScanAnimationFinished?.Invoke(onCompleteArgs);
    }

    IEnumerator DisplayEnhanceEffect(float duration, Scanner.EnhanceEventArgs onCompleteArgs)
    {
        enhanceEffect.SetActive(true);
        enhanceParticles.Play();
        yield return new WaitForSeconds(duration);
        enhanceParticles.Stop();
        enhanceEffect.SetActive(false);
        EnhanceAnimationFinished?.Invoke(onCompleteArgs);
    }

    void Reset()
    {
        if (scanner == null)
        {
            scanner = GameObject.FindGameObjectWithTag("Scanner")
                .GetComponent<Scanner>();
        }
    }

    private void OnEnable()
    {
        scanner.ScanPerformed += OnScanPerformed;
        scanner.EnhancePerformed += OnEnhancePerformed;
    }
    private void OnDisable()
    {
        scanner.ScanPerformed -= OnScanPerformed;
        scanner.EnhancePerformed -= OnEnhancePerformed;
    }

}
