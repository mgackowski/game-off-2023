using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

/** Allows activating/deactivating GameObjects from Yarn scripts.
 * Since YarnSpinner internally uses GameObject.Find, it can only look for active objects in the hierarchy.
 * Therefore, it is necessary to keep references to toggleable objects here.
 * TODO: Use a listener component on each toggleable object that will auto-subscribe to this object
 */
public class YarnGameObjectSwitcher : MonoBehaviour
{
    [SerializeField] List<GameObject> trackedObjects;

    [YarnCommand("setActive")]
    public void SetActive(string objectName, bool state)
    {
        List<GameObject> matches = trackedObjects.Where(x => x.name == objectName).ToList();
        if (!matches.Any())
        {
            Debug.Log("setActive was called, but the target GameObject was null. Check for spelling in the Yarn script and whether it is present in the Scene Hierarchy.");
            return;
        }

        matches.ForEach(x => x.SetActive(state));

    }

}