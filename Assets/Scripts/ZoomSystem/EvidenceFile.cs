using UnityEngine;

public class EvidenceFile : MonoBehaviour
{
    /* The first image displayed when file is in focus */
    [SerializeField] public EvidenceImage defaultImage;
    /* A file may be hidden until a progression event occurs */
    [SerializeField] bool hidden = false;
}