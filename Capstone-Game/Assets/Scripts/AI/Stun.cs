using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class Stun : MonoBehaviour
{
    public ParticleSystem stomp; 

    public void stompEffect(SquirrelController playerController)
    {
        //play stomp
        if(!stomp.isPlaying)
        {
            stomp.Play();
        }
        
        

    }
}
