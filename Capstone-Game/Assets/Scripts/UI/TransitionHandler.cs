using UnityEngine;
using UnityEngine.Events;

public class TransitionHandler : MonoBehaviour
{
    public Animation anim;
    public UnityEvent transitionEvent;

    [ContextMenu("Transition")]
    public void Transition()
    {
        anim.Play();
    }

    public void OnTransitionEnded()
    {
        transitionEvent.Invoke();
    }
}
