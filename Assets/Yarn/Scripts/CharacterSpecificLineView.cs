using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;

/** A customised Line View that only displays dialogue for certain characters.
 * Adapted to use the custom Input System.
 * TODO: Fade effects are broken.
 */
public class CharacterSpecificLineView : LineView
{
    [Header("Customised fields")]
    [SerializeField] public List<string> activeForCharacters;
    
    CanvasGroup childObjects;
    string speakingCharacter;

    private void Start()
    {
        childObjects = GetComponent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        onDialogueLineFinished += finaliseAlpha; // Override alpha setting from private member base.RunLineInternal
        speakingCharacter = dialogueLine.CharacterName;

        if (activeForCharacters.Contains(speakingCharacter))
        {
            base.RunLine(dialogueLine, onDialogueLineFinished);
        }
        else
        {
            childObjects.gameObject.SetActive(false);
            onDialogueLineFinished();
        }

    }

    void finaliseAlpha()
    {
        if (activeForCharacters.Contains(speakingCharacter))
        {
            childObjects.alpha = 1f;
        }
        else
        {
            childObjects.alpha = 0f;
        }
    }

    public void AdvanceView(InputAction.CallbackContext ctx)
    {
        UserRequestedViewAdvancement();
    }

    private void OnEnable()
    {
        InputManager.Instance.Dialogue.Advance.performed += AdvanceView;
    }

    private void OnDisable()
    {
        InputManager.Instance.Dialogue.Advance.performed -= AdvanceView;
    }

}
