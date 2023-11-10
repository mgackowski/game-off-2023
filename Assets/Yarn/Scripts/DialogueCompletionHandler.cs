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
    public void OnDialogueComplete()
    {
        InputManager.Instance.SwitchTo(InputManager.Instance.Gameplay);
    }

}
