using System;
using UnityEngine;
using Yarn.Unity;

public class EvidenceFile : MonoBehaviour
{
    /* The first image displayed when file is in focus */
    [SerializeField] public EvidenceImage defaultImage;
    /* Parent Transform of all images held in this file */
    [SerializeField] GameObject imageGroup;

    [SerializeField] public bool playDialogueOnFirstFocus = false;
    [SerializeField] public string dialogueNode;

    [NonSerialized] public bool viewedBefore = false;

    /* A file may be hidden until a progression event occurs */
    [Header("Initial value in Inspector only")]
    [SerializeField] bool hidden = false;

    public bool Hidden { get { return hidden; } set { SetHidden(value); } }

    [YarnCommand("lockFile")]
    public void SetHidden(bool newHidden)
    {
        hidden = newHidden;
        imageGroup.SetActive(!hidden);
    }

    private void Start()
    {
        Hidden = hidden;
    }

    
}