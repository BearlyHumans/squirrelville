using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animation))]
public class TransitionHandler : MonoBehaviour
{
    private Animation anim;
    public AnimationClip enterAnimClip;
    public AnimationClip exitAnimClip;
    public UnityEvent transitionEvent;

    private void Awake()
    {
        anim = GetComponent<Animation>();
        anim.playAutomatically = false;
        DontDestroyOnLoad(this.gameObject);
    }

    [ContextMenu("Transition")]
    public void Transition()
    {
        if (enterAnimClip != null)
        {
            anim.AddClip(enterAnimClip, "enter");
            anim.Play("enter");
        }
        else
        {
            OnTransitionEnded();
        }
    }

    public void OnTransitionEnded()
    {
        transitionEvent.Invoke();

        if (exitAnimClip != null)
        {
            anim.AddClip(exitAnimClip, "exit");
            anim.Play("exit");
        }
    }
}
