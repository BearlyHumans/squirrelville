using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePoint : MonoBehaviour
{
    
    public float boundary = 10.0f;

    public virtual void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boundary); 
    }
}

