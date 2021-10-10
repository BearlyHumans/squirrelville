using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : MonoBehaviour
{
    public ParticleSystem stomp; 

    // play stomp effect and ready the stun box

    void stompEffect()
    {
        //play stomp
        if(!stomp.isPlaying)
        {
            stomp.Play();
        }
        StartCoroutine(stunPlayer());
       
    }
     // if player if inside check then freeze them
    IEnumerator stunPlayer()
    {
        yield return new WaitForSeconds(2.8f);
    
        print("stuned");
    }

}
