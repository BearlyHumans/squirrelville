using UnityEngine;

public class TransitionHandler : MonoBehaviour
{
    public Animation anim;

    [ContextMenu("Transition")]
    void Transition()
    {
        anim.Play();
    }
}
