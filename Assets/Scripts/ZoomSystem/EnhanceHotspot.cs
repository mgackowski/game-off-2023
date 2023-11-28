using UnityEngine;

/**
 * A scannable Hotspot that, when scanned successfully, unlocks another image.
 */
public class EnhanceHotspot : Hotspot
{
    [Header("Enhance-specific fields")]
    [SerializeField] EvidenceImage unlocksImage;
    [SerializeField] bool startDialogueOnSuccess;
    [SerializeField] string dialogueNodeAfterEnhance;

    bool enhancedOnce = false;

    /* Run when an Enhance is successful */
    public void Enhance()
    {
        if (locked || enhancedOnce)
        {
            return;
        }
        scannedOnce = true;
        enhancedOnce = true;
        unlocksImage.SetLockedState(false);

        if (startDialogueOnSuccess && !string.IsNullOrEmpty(dialogueNodeAfterEnhance))
        {
            dialogueSystem.RunDialogue(dialogueNodeAfterEnhance);
        }

        locked = true;
    }

    /* Run when an Enhance is successful and the animation has finished */
    public void EnhanceFinished()
    {
        //TODO: Implement this so that this is run separately
    }

    /* Same criteria as OnScanPerformed.
     */
    void OnEnhancePerformed(Scanner.EnhanceEventArgs args)
    {
        if (!args.areaInView.Contains(hotspotArea.center))
        {
            return;
        }
        if (CalculateOverlap(args.areaInView, hotspotArea) >= requiredOverlapRatio)
        {
            args.successful = true;
            Enhance();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (effects != null)
        {
            effects.EnhanceAnimationFinished += OnEnhancePerformed;
        }
        else if (scannableBy != null)
        {
            scannableBy.EnhancePerformed += OnEnhancePerformed;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (effects != null)
        {
            effects.EnhanceAnimationFinished -= OnEnhancePerformed;
        }
        else if (scannableBy != null)
        {
            scannableBy.EnhancePerformed -= OnEnhancePerformed;
        }
    }
}
