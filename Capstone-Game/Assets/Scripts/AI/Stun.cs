using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class Stun : MonoBehaviour
{
    public ParticleSystem stomp; 

    private LayerMask layerMask;

    public float stunRange;
    public float stunTime;
    void Start()
    {
        layerMask = LayerMask.GetMask("Player");
    }

    public void stompEffect(SquirrelController playerController)
    {
        //play stomp
        if(!stomp.isPlaying)
        {
            stomp.Play();
        }
        Collider[] thingsInBounds = Physics.OverlapSphere(transform.position, stunRange);
        foreach(var thing in thingsInBounds)
        {
            if (thing.tag == playerController.tag) 
            {
                playerController.FreezeMovement();
                StartCoroutine(unfreezePlayer(playerController));

            }
        }
    }

    IEnumerator unfreezePlayer(SquirrelController playerController)
    {
        yield return new WaitForSeconds(stunTime);
        playerController.UnfreezeMovement();
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, stunRange);
    }
}
