using UnityEngine;
using Yarn.Unity;

/** Allows setting animation triggers from Yarn scripts.
 */

[RequireComponent(typeof(Animator))]
public class AnimationPlayer : MonoBehaviour
{
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    [YarnCommand("playAnimation")]
    public void SetTrigger(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }
    
}