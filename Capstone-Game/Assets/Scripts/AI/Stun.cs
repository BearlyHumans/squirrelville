using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class Stun : MonoBehaviour
{
    public ParticleSystem stomp; 
    private LayerMask layerMask;

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
        Collider[] thingsInBounds = Physics.OverlapSphere(transform.position, 5.0f);
        foreach(var thing in thingsInBounds)
        {
            if (thing.tag == playerController.tag) 
            {
                print("player in range");
            }
        }
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, 5.0f);
    }
}
