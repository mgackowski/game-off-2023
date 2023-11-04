using UnityEngine;

/**
 * Controls the visibility of a texture depending on a Scanner's
 * zoom level.
 * TODO: Has a dependency on Scanner type
 */
[RequireComponent(typeof(Renderer))]
public class DetailTextureImage : MonoBehaviour
{
    [SerializeField] Scanner scannedBy;
    [SerializeField] float minOpacity = 0f;
    [SerializeField] float maxOpacity = 0.75f;
    [SerializeField] float zoomLevelForMinOpacity = 1f;
    [SerializeField] float zoomLevelForMaxOpacity = 5f;


    void OnZoomChanged(float zoomLevel)
    {
        Material mat = GetComponent<Renderer>().material;
        Color matColor = mat.color;
        matColor.a = minOpacity +
            (Mathf.Sqrt(Mathf.Clamp(
                ((zoomLevel - zoomLevelForMinOpacity) / zoomLevelForMaxOpacity),
                0f, 1f)) * maxOpacity) - minOpacity;
        mat.color = matColor;
    }

    void Awake()
    {
        if (scannedBy != null)
        {
           scannedBy.ZoomLevelChanged += OnZoomChanged;
        }  
    }

    void OnDestroy()
    {
        if (scannedBy != null)
        {
            scannedBy.ZoomLevelChanged -= OnZoomChanged;
        }
    }
}
