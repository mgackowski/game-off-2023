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
    public ScanningEffects effects;
    public DialogueSystem dialogueSystem;

    [SerializeField] EvidenceImage parentImage;
    
    [SerializeField] protected bool locked = true;
    [SerializeField][Range(0, 1)] protected float requiredOverlapRatio = 0.7f;
    [SerializeField][Range(0, 1)] protected float toleranceForHint = 0.9f; // how close to the overlap ratio to get a hint?

    [SerializeField] protected string dialogueNode;
    [SerializeField] protected bool onlyRunDialogueOnce = false;

    [Header("Leave empty to calculate from renderer")]
    [SerializeField] protected Rect hotspotArea;

    [Header("Debug")]
    [SerializeField] protected bool showInPlayMode = true;

    protected bool scannedOnce = false;


    /* Individually lock the hotspot, preventing it from being interacted with.
     */
    [YarnCommand("lockHotspot")]
    public void SetLocked(bool newLocked)
    {
        locked = newLocked;
    }

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

        dialogueSystem.RunDialogue(dialogueNode);

    }

    /* A hotspot is successfully caught in a scan if:
     * 1. The center of the hotspot is within the area scanned
     * 2. A minimum ratio of the hotspot and scan areas' overlap
     */
    protected virtual void OnScanPerformed(Scanner.ScanEventArgs eventArgs)
    {
        if (locked || !eventArgs.areaInView.Contains(hotspotArea.center))
        {
            return;
        }
        float overlap = CalculateOverlap(eventArgs.areaInView, hotspotArea);
        if (!eventArgs.userPerformedScan && !scannedOnce && overlap >= requiredOverlapRatio * toleranceForHint)
        {
            //Debug.Log($"{this.name} is triggering the hint.");
            eventArgs.successful = true; // the Scanner will know something's close
        }
        else if (eventArgs.userPerformedScan && overlap >= requiredOverlapRatio) {
            eventArgs.successful = true;
            Scan();
        }

    }

    /**
     * Disable hotspot interactivity if its parent image isn't unlocked yet
     */
    protected virtual void OnImageLockStateChanged(bool newLocked)
    {
        SetLocked(newLocked);
    }

    /** Returns the ratio two rectangles overlap by, from 0f to 1f
     */
    protected float CalculateOverlap(Rect a, Rect b)
    {
        Debug.DrawLine(new Vector3(a.xMin, a.yMin, 0f), new Vector3(a.xMin, a.yMax, 0f));
        Debug.DrawLine(new Vector3(a.xMin, a.yMin, 0f), new Vector3(a.xMax, a.yMin, 0f));
        Debug.DrawLine(new Vector3(a.xMax, a.yMax, 0f), new Vector3(a.xMin, a.yMax, 0f));
        Debug.DrawLine(new Vector3(a.xMax, a.yMax, 0f), new Vector3(a.xMax, a.yMin, 0f));

        Debug.DrawLine(new Vector3(b.xMin, b.yMin, 0f), new Vector3(b.xMin, b.yMax, 0f));
        Debug.DrawLine(new Vector3(b.xMin, b.yMin, 0f), new Vector3(b.xMax, b.yMin, 0f));
        Debug.DrawLine(new Vector3(b.xMax, b.yMax, 0f), new Vector3(b.xMin, b.yMax, 0f));
        Debug.DrawLine(new Vector3(b.xMax, b.yMax, 0f), new Vector3(b.xMax, b.yMin, 0f));

        // Determine common rectangle, c
        Rect c = Rect.zero;
        c.xMin = Mathf.Max(a.xMin, b.xMin);
        c.xMax = Mathf.Min(a.xMax, b.xMax);
        c.yMin = Mathf.Max(a.yMin, b.yMin);
        c.yMax = Mathf.Min(a.yMax, b.yMax);
        if (c.width <= 0f || c.height <= 0f)
        {
            return 0f; // No overlap
        }

        Debug.DrawLine(new Vector3(c.xMin, c.yMin, 0f), new Vector3(c.xMin, c.yMax, 0f));
        Debug.DrawLine(new Vector3(c.xMin, c.yMin, 0f), new Vector3(c.xMax, c.yMin, 0f));
        Debug.DrawLine(new Vector3(c.xMax, c.yMax, 0f), new Vector3(c.xMin, c.yMax, 0f));
        Debug.DrawLine(new Vector3(c.xMax, c.yMax, 0f), new Vector3(c.xMax, c.yMin, 0f));

        float cArea = c.width * c.height;
        float cInA = cArea / (a.width * a.height);
        float cInB = cArea / (b.width * b.height);

        // Old method
        //float bInA = (a.width - Mathf.Abs(a.min.x - b.min.x))
        //    * (a.height - Mathf.Abs(a.min.y - b.min.y))
        //    / (a.width * a.height);
        //float aInB = (b.width - Mathf.Abs(a.min.x - b.min.x))
        //    * (b.height - Mathf.Abs(a.min.y - b.min.y))
        //    / (b.width * b.height);

        Debug.Log($"Overlaps: {cInA}, {cInB}");
        return Mathf.Min(cInA, cInB);
    }

    protected virtual void OnEnable()
    {
        if (effects != null) // subscribing to effect finish is ideal
        {
            effects.ScanAnimationFinished += OnScanPerformed;
            effects.FinishedWithoutAnimation += OnScanPerformed;
        }
        else if (scannableBy != null) // if unavailable, subscribe directly to scanner
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

    protected virtual void OnDisable()
    {
        if (effects != null)
        {
            effects.ScanAnimationFinished -= OnScanPerformed;
            effects.FinishedWithoutAnimation -= OnScanPerformed;
        }
        else if (scannableBy != null)
        {
            scannableBy.ScanPerformed -= OnScanPerformed;
        }
        if (parentImage != null)
        {
            parentImage.ImageLockStateChanged += OnImageLockStateChanged;
        }
    }

    void Reset()
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
                .GetComponent<DialogueSystem>();
        }
    }

}
