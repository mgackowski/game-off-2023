using System;
using UnityEngine;

/** An image containing Hostpots. Interactions can be prevented if the
 * image hasn't been scanned first, see: <see cref="EnhanceHotspot"/>
 * A PolygonCollider2D defines the image's bounding box to constrain the camera.
 */
[RequireComponent(typeof(Renderer), typeof(PolygonCollider2D))]
public class EvidenceImage : MonoBehaviour
{
    public event Action<bool> ImageLockStateChanged;

    public Bounds Bounds { get => GetComponent<Renderer>().bounds; }
    public bool Locked { get => locked; set => SetLockedState(value); }

    [Header("Only sets initial state in Edit Mode")] // Inspector unaware of Properties
    [SerializeField] bool locked = true;

    Renderer rend;

    // TODO: Unimplemented; zoomed image stays at max opacity once unlocked
    // Instead this behaviour is reserved for child class DetailTextureImage
    //float minOpacity = 1f;
    //float maxOpacity = 1f;
    //[SerializeField] float zoomLevelForMinOpacity = 1f; // calculate from zoom
    //[SerializeField] float zoomLevelForMaxOpacity = 5f; // calculate from zoom
    //float opacityTransitionZoomLevel;


    public void SetLockedState(bool newLocked)
    {
        locked = newLocked;
        rend.forceRenderingOff = newLocked;
        ImageLockStateChanged?.Invoke(newLocked);
    }

    private void Start()
    {
        rend = GetComponent<Renderer>();
        rend.forceRenderingOff = locked;
        ImageLockStateChanged?.Invoke(locked);
    }

}
