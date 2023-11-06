using UnityEngine;

/**
 * A 2D area placed on an image which can be scanned by a Scanner.
 * If the scan area and the hotspot's bounds match, the scan is successful
 */
public class Hotspot : MonoBehaviour
{
    public Scanner scannableBy;

    [SerializeField] EvidenceImage parentImage;
    [SerializeField] public bool scannedOnce = false;
    [SerializeField] protected bool locked = true;
    [SerializeField] protected float requiredOverlapRatio;

    //TODO: Once dialogue system is hooked up
    //[SerializeField] string dialogueNode;

    [Header("Leave empty to calculate from renderer")]
    [SerializeField] protected Rect hotspotArea;


    /* Run when a scan is successful */
    public virtual void Scan()
    {
        if (locked)
        {
            return;
        }
        scannedOnce = true;
        Debug.Log(gameObject.name + " scanned.");
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

            //Disabled while testing
            //renderer.forceRenderingOff = true;
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

}
