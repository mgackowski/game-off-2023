using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/** Controls panning, zooming, scanning and enhancing.
 */
public class Scanner : MonoBehaviour
{
    public struct EnhanceEventArgs
    {
        public Rect areaInView;
        public bool successful;
    }

    // Events
    public event Action<float> ZoomLevelChanged;
    public event Action<float> MaxZoomLevelChanged;
    public event Action<Rect> ScanPerformed; // Rect = XY (world space) of area scanned
    public event Action<EnhanceEventArgs> EnhancePerformed;
    public event Action EnhanceAttemptedAtLowZoom;

    // Recommended property to get zoom level in familar format e.g. 1x, 10x etc.
    public float zoomLevel { get { return baseOrthoSize / cam.m_Lens.OrthographicSize; ; } }

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
    

    CinemachineVirtualCamera cam;
    Vector2 panningSpeed = Vector2.zero;
    float zoomSpeed = 0f;
    bool lastZoomByScroll;
    float currentZoomSpeedDamp = 1;
    Rect areaInView;
    Collider2D viewBoundingShape;

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
    }

    private void OnDisable() {
        InputManager.Instance.Gameplay.Pan.performed -= Pan;
        InputManager.Instance.Gameplay.Zoom.performed -= Zoom;
        InputManager.Instance.Gameplay.Scan.performed -= Scan;
        InputManager.Instance.Gameplay.Enhance.performed -= Enhance;
        fileSwitcher.FileSwitched -= SwitchFile;
    }

    void LateUpdate()
    {
        ApplyMovement();
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

    public void Scan(InputAction.CallbackContext ctx)
    {
        UpdateAreaInView();
        ScanPerformed?.Invoke(areaInView);

    }

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
        if(!eventArgs.successful) // no enhance hotspots reported success
        {
            Debug.Log("Nothing to enhance here.");
            //TODO: Produce an effect or run dialogue.
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
    }

    [YarnCommand("maxZoom")]
    public void SetMaxZoomLevel(float newMax)
    {
        maxZoomLevel = newMax;
        MaxZoomLevelChanged?.Invoke(newMax);
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
        Vector3 panDelta = panningSpeed * Time.deltaTime * panSpeedModifier / zoomLevel;
        
        Vector3 currentPosition = followTarget.localPosition; 
        currentPosition += panDelta;
        followTarget.localPosition = currentPosition;

        if (zoomSpeed != 0f)
        {
            float modifier = lastZoomByScroll ? zoomSpeedScrollModifier : zoomSpeedKeysModifier;
            float zoomDelta = (zoomSpeed * modifier * Time.deltaTime) + 1f;
            cam.m_Lens.OrthographicSize /= zoomDelta;
            ZoomLevelChanged?.Invoke(zoomLevel); // ignores snap correction but should be OK

            if (lastZoomByScroll) {
                currentZoomSpeedDamp += Time.deltaTime;
                if (currentZoomSpeedDamp > zoomSpeedScrollDampOver) {
                    currentZoomSpeedDamp = zoomSpeedScrollDampOver;
                }
                zoomSpeed = Mathf.Lerp(zoomSpeed, 0, currentZoomSpeedDamp / zoomSpeedScrollDampOver);
            }
        }

        SnapToBounds();
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
        }
    }


}
