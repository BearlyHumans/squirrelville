using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class Stun : MonoBehaviour
{
    public ParticleSystem stomp; 

    private LayerMask layerMask;

    public Humans human;

    public float stunRange;
    public float stunTime;
    void Start()
    {
        layerMask = LayerMask.GetMask("Player");
    }

    public void stompEffect(SquirrelController playerController, 
                            SquirrelFoodGrabber foodController, int takeFoodAmmount)
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

                human.hitPlayerStun = true;

                takeFood(foodController,takeFoodAmmount);
                playerController.FreezeAndStun();
                StartCoroutine(unfreezePlayer(playerController));

            }
        }
       
    }
    private void takeFood(SquirrelFoodGrabber foodController, int takeFoodAmmount)
    {
        int i = 0; 
        while(i < takeFoodAmmount)
        {
            foodController.ThrowFood(1); 
            i++;
        }
    }

    IEnumerator unfreezePlayer(SquirrelController playerController)
    {
        yield return new WaitForSeconds(stunTime);
        playerController.UnfreezeMovement();
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (transform.position, stunRange);
    }
}
