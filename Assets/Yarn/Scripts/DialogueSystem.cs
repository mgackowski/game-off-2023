using UnityEngine;
using Yarn.Unity;

/** 
 *  Allows other Unity classes to run Yarn dialogue.
 */
[RequireComponent(typeof(DialogueRunner))]
public class DialogueSystem : MonoBehaviour
{
    DialogueRunner runner;

    private void Awake()
    {
        runner = GetComponent<DialogueRunner>();
    }

    /* Run dialogue from a given node using Yarn Spinner. */
    public void RunDialogue(string nodeName)
    {
        if (runner == null)
        {
            Debug.Log("Couldn't run dialogue from {nodeName}, DialogueSystem couldn't find a DialogueRunner!");
            return;
        }
        if (!runner.IsDialogueRunning)
        {
            runner.StartDialogue(nodeName);
            InputManager.Instance.SwitchTo(InputManager.Instance.Dialogue);
        }

    }
}