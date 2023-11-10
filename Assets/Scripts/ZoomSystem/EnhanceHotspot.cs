using UnityEngine;

/**
 * A scannable Hotspot that, when scanned successfully, unlocks another image.
 */
public class EnhanceHotspot : Hotspot
{
    [Header("Enhance-specific fields")]
    [SerializeField] EvidenceImage unlocksImage;
    [SerializeField] bool startDialogueOnSuccess;

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
        unlocksImage.SetLockedState(false);

        if (startDialogueOnSuccess && !string.IsNullOrEmpty(dialogueNode))
        {
            InputManager.Instance.SwitchTo(InputManager.Instance.Dialogue);
            dialogueSystem?.StartDialogue(dialogueNode);
        }

    }

    /* A hotspot is successfully caught in a scan if:
    * 1. The center of the hotspot is within the area scanned
    * 2. A minimum ratio of the hotspot and scan areas' overlap
    */
    protected override void OnScanPerformed(Rect areaScanned)
    {
        if (!areaScanned.Contains(hotspotArea.center))
        {
            return;
        }
        if (CalculateOverlap(areaScanned, hotspotArea) >= requiredOverlapRatio)
        {
            Scan();
        }

    }
}
