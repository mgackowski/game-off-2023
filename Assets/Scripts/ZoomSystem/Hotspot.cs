using UnityEditor.SearchService;
using UnityEngine;
using Yarn.Unity;

/**
 * A 2D area placed on an image which can be scanned by a Scanner.
 * If the scan area and the hotspot's bounds match, the scan is successful
 */
public class Hotspot : MonoBehaviour
{
    public Scanner scannableBy;
    public DialogueRunner dialogueSystem;

    [SerializeField] EvidenceImage parentImage;
    
    [SerializeField] protected bool locked = true;
    [SerializeField] protected float requiredOverlapRatio;

    [SerializeField] protected string dialogueNode;
    [SerializeField] protected bool onlyRunDialogueOnce = false;

    [Header("Leave empty to calculate from renderer")]
    [SerializeField] protected Rect hotspotArea;

    [Header("Debug")]
    [SerializeField] protected bool showInPlayMode = true;

    protected bool scannedOnce = false;


    /* Run when a scan is successful */
    public virtual void Scan()
    {
        if (locked)
        {
            return;
        }
        
        Debug.Log(gameObject.name + " scanned.");

        if (onlyRunDialogueOnce && scannedOnce)
        {
            return;
        }

        scannedOnce = true;

        if (string.IsNullOrEmpty(dialogueNode))
        {
            return;
        }

        InputManager.Instance.SwitchTo(InputManager.Instance.Dialogue);
        dialogueSystem?.StartDialogue(dialogueNode);

    }

    /* A hotspot is successfully caught in a scan if:
     * 1. The center of the hotspot is within the area scanned
     * 2. A minimum ratio of the hotspot and scan areas' overlap
     */
    protected virtual void OnScanPerformed(Rect areaScanned)
    {
        if (!areaScanned.Contains(hotspotArea.center))
        {
            return;
        }
        //Debug.Log($"Center of {gameObject.name} in view: {hotspotArea}, center {hotspotArea.center}");
        if (CalculateOverlap(areaScanned, hotspotArea) >= requiredOverlapRatio) {
            Scan();
        }

    }

    /**
     * Disable hotspot interactivity if its parent image isn't unlocked yet
     */
    protected virtual void OnImageLockStateChanged(bool newLocked)
    {
        locked = newLocked;
    }

    /** Returns the ratio two rectangles overlap by, from 0f to 1f
     */
    protected float CalculateOverlap(Rect a, Rect b)
    {
        float bInA = (a.width - Mathf.Abs(a.min.x - b.min.x))
            * (a.height - Mathf.Abs(a.min.y - b.min.y))
            / (a.width * a.height);
        float aInB = (b.width - Mathf.Abs(a.min.x - b.min.x))
            * (b.height - Mathf.Abs(a.min.y - b.min.y))
            / (b.width * b.height);
        Debug.Log($"Overlaps: {bInA}, {aInB}");
        return Mathf.Min(bInA, aInB);
    }

    void OnEnable()
    {
        if (scannableBy != null)
        {
            scannableBy.ScanPerformed += OnScanPerformed;
        }
        if (parentImage != null)
        {
            parentImage.ImageLockStateChanged += OnImageLockStateChanged;
        }
    }

    void Start()
    {
        // If using a visible sprite or mesh to help position hotspot,
        // retrieve its bounds; then make invisible
        if (GetComponent<Renderer>() != null && hotspotArea == Rect.zero)
        {
            Renderer renderer = GetComponent<Renderer>();
            Bounds bounds = renderer.bounds;
            hotspotArea.Set(
                bounds.center.x - bounds.extents.x,
                bounds.center.y - bounds.extents.y,
                bounds.size.x, bounds.size.y);

            renderer.forceRenderingOff = !showInPlayMode;
        }

    }

    void OnDisable()
    {
        if (scannableBy != null)
        {
            scannableBy.ScanPerformed -= OnScanPerformed;
        }
        if (parentImage != null)
        {
            parentImage.ImageLockStateChanged += OnImageLockStateChanged;
        }
    }

    private void Reset()
    {
        if (scannableBy == null)
        {
            // We don't have to use a singleton Scanner and can still replace it
            scannableBy = GameObject.FindGameObjectWithTag("Scanner")
                .GetComponent<Scanner>();
        }
        if (dialogueSystem == null)
        {
            dialogueSystem = GameObject.FindGameObjectWithTag("DialogueSystem")
                .GetComponent<DialogueRunner>();
        }
    }

}
