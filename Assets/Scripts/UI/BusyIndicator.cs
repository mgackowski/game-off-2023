using UnityEngine;

/**
 * Show the busy indicator when you're close to a hotspot
 */
public class BusyIndicator : MonoBehaviour
{
    [SerializeField] Scanner whichScanner;
    [SerializeField] GameObject icon;

    void OnScannerCloseToHotspot()
    {
        icon.SetActive(true);
        //TODO: start playing sound;
    }

    void OnScannerLeavesHotspot()
    {
        icon.SetActive(false);
        //TODO: start playing sound
    }

    void OnEnable()
    {
        if (whichScanner != null)
        {
            whichScanner.EnteredNearHotspot += OnScannerCloseToHotspot;
            whichScanner.LeftNearHotspot += OnScannerLeavesHotspot;
        }
    }

    void OnDisable()
    {
        if (whichScanner != null)
        {
            whichScanner.EnteredNearHotspot -= OnScannerCloseToHotspot;
            whichScanner.LeftNearHotspot -= OnScannerLeavesHotspot;
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