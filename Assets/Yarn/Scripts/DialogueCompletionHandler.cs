using UnityEngine;
using Yarn.Unity;

/**
 * As DialogueRunner uses Unity Events, this MonoBehaviour allows it
 * to interface with InputManager, without having to modify or extend
 * DialogueRunenr itself.
 */
[RequireComponent(typeof(DialogueRunner))]
public class DialogueCompletionHandler : MonoBehaviour
{
    [SerializeField] Scanner scanner;

    private void Awake()
    {
       if (scanner == null)
        {
            scanner = GameObject.FindGameObjectWithTag("Scanner").GetComponent<Scanner>();

        }
    }

    public void OnDialogueComplete()
    {
        InputManager.Instance.SwitchTo(InputManager.Instance.Gameplay);
        scanner?.EvaluateForHotspots(); // restore hourglass hint after dialogue

    }

}
