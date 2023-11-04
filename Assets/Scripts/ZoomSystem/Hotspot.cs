using UnityEngine;

public class Hotspot : MonoBehaviour, IScannable
{
    [SerializeField] bool scannedOnce = false;
    [SerializeField] bool locked = false;

    public bool IsScannedOnce()
    {
        return scannedOnce;
    }

    public bool IsLocked()
    {
        return locked;
    }

    public void Scan()
    {
        if (locked)
        {
            return;
        }
        scannedOnce = true;
        Debug.Log(gameObject.name + " scanned.");
    }

}
