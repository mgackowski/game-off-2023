using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/** Controls panning, zooming, scanning and enhancing.
 */
public class Scanner : MonoBehaviour
{
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

    void Start()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
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
        //Debug.Log("Captured zoom event");   // Not getting scrollwheel input!
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
            float zoomDelta = ((zoomSpeed - 1f) * zoomSpeedModifier * Time.deltaTime) + 1f;
            cam.m_Lens.OrthographicSize /= zoomDelta;
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