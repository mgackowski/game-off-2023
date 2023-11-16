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

    /* Run when a scan is successful */
    public override void Scan()
    {
        if (locked)
        {
            return;
        }

        Debug.Log(gameObject.name + " scanned.");

        if (scannedOnce)
        {
            return;
        }

        scannedOnce = true;

        if (!string.IsNullOrEmpty(dialogueNode))
        {
            dialogueSystem?.StartDialogue(dialogueNode);
            if (dialogueSystem != null && dialogueSystem.IsDialogueRunning)
            {
                InputManager.Instance.SwitchTo(InputManager.Instance.Dialogue);
            }
        }

    }

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
            dialogueSystem?.StartDialogue(dialogueNodeAfterEnhance);
            if (dialogueSystem != null && dialogueSystem.IsDialogueRunning)
            {
                InputManager.Instance.SwitchTo(InputManager.Instance.Dialogue);
            }
        }

        locked = true;
    }

    /* A hotspot is successfully caught in a scan if:
    * 1. The center of the hotspot is within the area scanned
    * 2. A minimum ratio of the hotspot and scan areas' overlap
    */
/*    protected override void OnScanPerformed(Scanner.ScanEventArgs eventArgs)
    {
        if (!eventArgs.areaInView.Contains(hotspotArea.center))
        {
            return;
        }
        if (CalculateOverlap(eventArgs.areaInView, hotspotArea) >= requiredOverlapRatio)
        {
            Scan();
        }

    }*/

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
        if (scannableBy != null)
        {
            scannableBy.EnhancePerformed += OnEnhancePerformed;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (scannableBy != null)
        {
            scannableBy.EnhancePerformed -= OnEnhancePerformed;
        }
    }
}
