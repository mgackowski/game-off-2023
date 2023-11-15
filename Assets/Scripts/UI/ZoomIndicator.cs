using TMPro;
using UnityEngine;

/**
 * Update the zoom and max zoom levels displayed on screen.
 */
public class ZoomIndicator : MonoBehaviour
{
    [SerializeField] Scanner whichScanner;
    [SerializeField] string prefix = "";
    [SerializeField] string divider = "x / ";
    [SerializeField] string postfix = "x";

    TextMeshProUGUI tmp;
    float zoomLevel = 1f;
    float maxZoomLevel = 1f;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        //TODO: Initial values might depend on execution order, currently best to
        //have ZoomLevelChanged and MaxZoomLevelChanged fired at least once
        if (whichScanner != null)
        {
            zoomLevel = whichScanner.zoomLevel;
            maxZoomLevel = whichScanner.maximumZoomLevel;
        }
    }

    void OnZoomLevelChanged(float level)
    {
        zoomLevel = level;
        UpdateTextField();
    }

    void OnMaxZoomLevelChanged(float level)
    {
        maxZoomLevel = level;
        UpdateTextField();
    }

    void UpdateTextField()
    {
        tmp.text = prefix + string.Format("{0:0.#}", zoomLevel) + divider +
            string.Format("{0:0.#}", maxZoomLevel) + postfix;
    }

    protected virtual void OnEnable()
    {
        if (whichScanner != null)
        {
            whichScanner.ZoomLevelChanged += OnZoomLevelChanged;
            whichScanner.MaxZoomLevelChanged += OnMaxZoomLevelChanged;
        }
    }

    protected virtual void OnDisable()
    {
        if (whichScanner != null)
        {
            whichScanner.ZoomLevelChanged -= OnZoomLevelChanged;
            whichScanner.MaxZoomLevelChanged -= OnMaxZoomLevelChanged;
        }
    }

    void Reset()
    {
        if (whichScanner == null)
        {
            whichScanner = GameObject.FindGameObjectWithTag("Scanner")
                .GetComponent<Scanner>();
        }
    }
}
