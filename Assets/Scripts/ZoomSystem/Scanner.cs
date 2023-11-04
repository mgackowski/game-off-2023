using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/** Controls panning, zooming, scanning and enhancing.
 */
public class Scanner : MonoBehaviour
{
    // Events
    public event Action<float> ZoomLevelChanged;
    public event Action<Rect> ScanPerformed; // Rect = XY (world space) of area scanned

    // Recommended property to get zoom level in familar format e.g. 1x, 10x etc.
    public float zoomLevel { get { return baseOrthoSize / cam.m_Lens.OrthographicSize; ; } }

    [SerializeField] float panSpeedModifier = 1f;
    [SerializeField] float zoomSpeedModifier = 1f;
    [SerializeField] float minZoomLevel = 1f;
    [SerializeField] float maxZoomLevel = 5f;
    [SerializeField] float maxPanDistanceX = 0.5f;
    [SerializeField] float maxPanDistanceY = 0.5f;
    // Camera's ortho size property that maps to a 1x zoom level */
    [SerializeField] float baseOrthoSize = 0.5f;

    CinemachineVirtualCamera cam;
    Vector2 panningSpeed = Vector2.zero;
    float zoomSpeed = 0f;
    Rect areaInView;

    void Start()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
        areaInView = Rect.zero;
    }

    private void OnEnable() {
        InputManager.Instance.Gameplay.Pan.performed += Pan;
        InputManager.Instance.Gameplay.Zoom.performed += Zoom;
        InputManager.Instance.Gameplay.Scan.performed += Scan;
    }

    private void OnDisable() {
        InputManager.Instance.Gameplay.Pan.performed -= Pan;
        InputManager.Instance.Gameplay.Zoom.performed -= Zoom;
        InputManager.Instance.Gameplay.Scan.performed -= Scan;
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
        zoomSpeed = ctx.ReadValue<float>();
        //Debug.Log($"{zoomSpeed}");
    }

    public void Scan(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Scan attempted.");

        LensSettings lens = cam.m_Lens;
        float viewHeight = 2f * lens.OrthographicSize;
        float viewWidth = viewHeight * lens.Aspect;
        areaInView.Set(
            cam.transform.position.x - (viewWidth / 2),
            cam.transform.position.y - (viewHeight / 2),
            viewWidth, viewHeight);

        //Debug.Log($"Area in view determined: {areaInView}");

        ScanPerformed?.Invoke(areaInView);

    }

    /** Calculate and apply the next frame's zoom and position **/
    void ApplyMovement()
    {
        Vector3 panDelta = panningSpeed * Time.deltaTime * panSpeedModifier / zoomLevel;
        
        Vector3 currentPosition = cam.transform.localPosition; 
        currentPosition += panDelta;
        cam.transform.localPosition = currentPosition;

        if (zoomSpeed != 0f)
        {
            float zoomDelta = (zoomSpeed * zoomSpeedModifier * Time.deltaTime) + 1f;
            cam.m_Lens.OrthographicSize /= zoomDelta;
            ZoomLevelChanged?.Invoke(zoomLevel); // ignores snap correction but should be OK
        }

        SnapToBounds();

    }

    /** Don't let the camera exceed its pan and zoom limits **/
    void SnapToBounds()
    {
        Vector3 cameraPosition = cam.transform.position;
        if (Mathf.Abs(cameraPosition.x - transform.position.x) > maxPanDistanceX)
        {
            cameraPosition.x = maxPanDistanceX * Mathf.Sign(cameraPosition.x - transform.position.x);
            cam.transform.position = cameraPosition;
        }
        if (Mathf.Abs(cameraPosition.y - transform.position.y) > maxPanDistanceY)
        {
            cameraPosition.y = maxPanDistanceY * Mathf.Sign(cameraPosition.y - transform.position.y);
            cam.transform.position = cameraPosition;
        }
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
