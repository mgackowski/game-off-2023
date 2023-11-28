using System;
using System.Diagnostics.Tracing;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/** Controls panning, zooming, scanning and enhancing.
 */
public class Scanner : MonoBehaviour
{
    public class EnhanceEventArgs
    {
        public Rect areaInView; // XY (world space) of area scanned
        public bool successful;
    }

    public class ScanEventArgs
    {
        public bool userPerformedScan; // false if scanning continuously to update UI
        public Rect areaInView; // XY (world space) of area scanned
        public bool successful;
    }

    // Events
    public event Action<float> ZoomLevelChanged;
    public event Action<float> MaxZoomLevelChanged;
    public event Action<ScanEventArgs> ScanPerformed;
    public event Action<EnhanceEventArgs> EnhancePerformed;
    public event Action EnhanceAttemptedAtLowZoom;
    public event Action EnteredNearHotspot;
    public event Action LeftNearHotspot;
    public event Action EnhanceSuccessful;
    public event Action EnhanceUnsuccessful;

    // Recommended property to get zoom level in familar format e.g. 1x, 10x etc.
    public float zoomLevel { get { return baseOrthoSize / cam.m_Lens.OrthographicSize; ; } }
    public float maximumZoomLevel { get { return maxZoomLevel; } set { SetMaxZoomLevel(value); } }

    [SerializeField] EvidenceImage imageInFocus;
    [SerializeField] float panSpeedModifier = 1f;
    [SerializeField] float zoomSpeedKeysModifier = 1f;
    [SerializeField] float zoomSpeedScrollModifier = 1f;
    [SerializeField] float zoomSpeedScrollDampOver = .2f;
    [SerializeField] float minZoomLevel = 1f;
    [SerializeField] float maxZoomLevel = 5f;
    [SerializeField] float maxPanDistanceFromEdge = 0f;
    [SerializeField] float baseOrthoSize = 0.5f; // Camera's ortho size property that maps to a 1x zoom level
    [SerializeField] FileSwitcher fileSwitcher;
    [SerializeField] Transform followTarget;
    [SerializeField] ScanningEffects effects;

    [Header("Dialogue")]
    [SerializeField] DialogueSystem dialogueSystem;
    [SerializeField] string onEnhanceFailedNodeName;

    [Header("Ending")]
    [SerializeField] D3SceneManager d3SceneManager;
    [SerializeField] float specialZoomSpeedModifier = 0.5f;

    CinemachineVirtualCamera cam;
    Vector2 panningSpeed = Vector2.zero;
    float zoomSpeed = 0f;
    bool lastZoomByScroll;
    float currentZoomSpeedDamp = 1;
    Rect areaInView;
    Collider2D viewBoundingShape;
    bool closeToHotspot = false;
    bool specialZoomoutMode = false;
    float lastZoomLevel = 1f;

    void Start()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
        viewBoundingShape = imageInFocus.GetComponent<Collider2D>();
        areaInView = Rect.zero;
    }

    private void OnEnable() {
        InputManager.Instance.Gameplay.Pan.performed += Pan;
        InputManager.Instance.Gameplay.Zoom.performed += Zoom;
        InputManager.Instance.Gameplay.Scan.performed += Scan;
        InputManager.Instance.Gameplay.Enhance.performed += Enhance;
        fileSwitcher.FileSwitched += SwitchFile;
        effects.ScanAnimationFinished += ScanComplete;
        effects.EnhanceAnimationFinished += EnhanceComplete;
        effects.FinishedWithoutAnimation += EvaluationFinished;
    }

    private void OnDisable() {
        InputManager.Instance.Gameplay.Pan.performed -= Pan;
        InputManager.Instance.Gameplay.Zoom.performed -= Zoom;
        InputManager.Instance.Gameplay.Scan.performed -= Scan;
        InputManager.Instance.Gameplay.Enhance.performed -= Enhance;
        fileSwitcher.FileSwitched -= SwitchFile;
        effects.ScanAnimationFinished -= ScanComplete;
        effects.EnhanceAnimationFinished -= EnhanceComplete;
        effects.FinishedWithoutAnimation -= EvaluationFinished;
    }

    void LateUpdate()
    {
        ApplyMovement();
    }

    void Reset()
    {
        if (dialogueSystem == null)
        {
            dialogueSystem = GameObject.FindGameObjectWithTag("DialogueSystem").GetComponent<DialogueSystem>();
        }
    }

    public void Pan(InputAction.CallbackContext ctx)
    {
        panningSpeed = ctx.ReadValue<Vector2>();
    }

    public void Zoom(InputAction.CallbackContext ctx)
    {
        var newSpeed = ctx.ReadValue<float>();
        if (ctx.control.path.Contains("scroll")) {
            // The scroll speed resets automatically
            if (newSpeed != 0) {
                zoomSpeed = newSpeed;
                lastZoomByScroll = true;
                currentZoomSpeedDamp = 0;
            }
        } else {
            lastZoomByScroll = false;
            zoomSpeed = newSpeed;
        }
    }

    /* Begin a scan. */
    public void Scan(InputAction.CallbackContext ctx)
    {
        UpdateAreaInView();
        ScanEventArgs eventArgs = new ScanEventArgs()
        {
            userPerformedScan = true,
            successful = false,
            areaInView = areaInView
        };
        ScanPerformed?.Invoke(eventArgs);
        if (effects == null)
        {
            ScanComplete(eventArgs); // don't wait on effects to finish because there aren't any
        }
        
    }

    /* Finish up after scan effects have taken place. */
    public void ScanComplete(ScanEventArgs args)
    {
        if (args.successful == true)
        {
            LeftNearHotspot?.Invoke(); // stop showing the hint
        }
    }

    /* Begin an enhance. */
    public void Enhance(InputAction.CallbackContext ctx)
    {
        /*if (zoomLevel < maxZoomLevel)
        {
            Debug.Log("An enhance can only be performed at max zoom level.");
            EnhanceAttemptedAtLowZoom?.Invoke();       
            return;
        }*/

        UpdateAreaInView();
        EnhanceEventArgs eventArgs = new EnhanceEventArgs()
        {
            successful = false,
            areaInView = areaInView,

        };
        EnhancePerformed?.Invoke(eventArgs);
        if (effects == null)
        {
            EnhanceComplete(eventArgs); // don't wait on effects to finish because there aren't any
        }
    }

    /* Finish up an ehnance after effects are complete. */
    public void EnhanceComplete(EnhanceEventArgs args)
    {
        if (args.successful)
        {
            EnhanceSuccessful?.Invoke();
        }
        else // no enhance hotspots reported success
        {
            EnhanceUnsuccessful?.Invoke();
            dialogueSystem.RunDialogue(onEnhanceFailedNodeName);
        }
    }

    public void SwitchFile(EvidenceFile newFile)
    {
        imageInFocus = newFile.defaultImage;
        followTarget.position = imageInFocus.Bounds.center;

        Collider2D imageCollider = imageInFocus.GetComponent<Collider2D>();
        if (imageCollider != null)
        {
            viewBoundingShape = imageCollider;
        }
        else
        {
            viewBoundingShape = null;
        }

        if (newFile.playDialogueOnFirstFocus && !newFile.viewedBefore)
        {
            dialogueSystem.RunDialogue(newFile.dialogueNode);
        }

        newFile.viewedBefore = true;
    }

    [YarnCommand("maxZoom")]
    public void SetMaxZoomLevel(float newMax)
    {
        maxZoomLevel = newMax;
        MaxZoomLevelChanged?.Invoke(newMax);
    }

    [YarnCommand("specialZoomout")]
    public void SetSpecialZoomoutMode(bool enabled)
    {
        specialZoomoutMode = enabled;
    }

    /** Use the camera's settings to update the areaInView field **/
    void UpdateAreaInView()
    {
        LensSettings lens = cam.m_Lens;
        float viewHeight = 2f * lens.OrthographicSize;
        float viewWidth = viewHeight * lens.Aspect;
        areaInView.Set(
            cam.transform.position.x - (viewWidth / 2),
            cam.transform.position.y - (viewHeight / 2),
            viewWidth, viewHeight);
    }

    /** Calculate and apply the next frame's zoom and position **/
    void ApplyMovement()
    {
        if (!InputManager.Instance.Gameplay.enabled)
        {
            panningSpeed = Vector2.zero;
            return;
        }

        Vector3 panDelta = panningSpeed * Time.deltaTime * panSpeedModifier / zoomLevel;
        
        Vector3 currentPosition = followTarget.localPosition; 
        currentPosition += panDelta;
        followTarget.localPosition = currentPosition;

        bool ZoomChanged = false;

        if (zoomSpeed != 0f)
        {
            ZoomChanged = true;
            float modifier = lastZoomByScroll ? zoomSpeedScrollModifier : zoomSpeedKeysModifier;
            if (specialZoomoutMode)
            {
                modifier *= specialZoomSpeedModifier;
            }
            float zoomDelta = (zoomSpeed * modifier * Time.deltaTime) + 1f;
            cam.m_Lens.OrthographicSize /= zoomDelta;
            if (lastZoomByScroll) {
                currentZoomSpeedDamp += Time.deltaTime;
                if (currentZoomSpeedDamp > zoomSpeedScrollDampOver) {
                    currentZoomSpeedDamp = zoomSpeedScrollDampOver;
                }
                zoomSpeed = Mathf.Lerp(zoomSpeed, 0, currentZoomSpeedDamp / zoomSpeedScrollDampOver);
            }
        }

        SnapToBounds();

        if (ZoomChanged && zoomLevel != lastZoomLevel) //Fire event with correct zoom level after it's snapped
        {
            lastZoomLevel = zoomLevel;
            ZoomLevelChanged?.Invoke(zoomLevel);
        }
        if (ZoomChanged || panDelta.magnitude > 0f)
        {
            EvaluateForHotspots(); // Player moved, see if we can show them a hint they're close
        }
    }

    /** Don't let the camera exceed its pan and zoom limits
     * **/
    void SnapToBounds()
    {
        // Use of CinemachineConfiner provides nice damping effect but
        // creates inconsistens panning behaviour for user at edges
        //CinemachineConfiner confiner = cam.GetComponent<CinemachineConfiner>();
        //if (confiner != null) {
        //    confiner.m_BoundingShape2D = viewBoundingShape;
        //}

        if (viewBoundingShape != null)
        {
            Vector3 followTargetPos = followTarget.position;
            Bounds imageBounds = viewBoundingShape.bounds;
            imageBounds.Expand(maxPanDistanceFromEdge); //TODO: Adjust based on zoom

            if (!imageBounds.Contains(followTargetPos))
            {
                followTargetPos = imageBounds.ClosestPoint(followTargetPos);
                followTarget.position = followTargetPos;
            }
        }
        
        // Zoom level locked by story progression
        if (zoomLevel > maxZoomLevel)
        {
            cam.m_Lens.OrthographicSize = baseOrthoSize / maxZoomLevel;
        }
        else if (zoomLevel < minZoomLevel)
        {
            cam.m_Lens.OrthographicSize = baseOrthoSize / minZoomLevel;
            if (specialZoomoutMode)
            {
                zoomSpeed = 0f;
                d3SceneManager.SwitchTo3D();
            }

        }
    }

    /* Perform an "invisible" scan so that the user can get a hint if there's a hotspot nearby.
     */
    public void EvaluateForHotspots()
    {
        //Debug.Log("Evaluating for hotspots...");

        UpdateAreaInView();
        ScanEventArgs eventArgs = new ScanEventArgs()
        {
            userPerformedScan = false,
            successful = false,
            areaInView = areaInView
        };
        ScanPerformed?.Invoke(eventArgs);

        if (effects == null)
        {
            EvaluationFinished(eventArgs);
        }
        
    }

    void EvaluationFinished(ScanEventArgs args)
    {
        if (args.successful && !closeToHotspot)
        {
            closeToHotspot = true;
            EnteredNearHotspot?.Invoke();
        }
        else if (!args.successful && closeToHotspot)
        {
            closeToHotspot = false;
            LeftNearHotspot?.Invoke();
        }
    }

}
