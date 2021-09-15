using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePoint : MonoBehaviour
{
    
    public float boundary = 10.0f;

    [Tooltip("list of bins that player can walk to")]
    public List<Bin> bins;

    public virtual void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boundary); 
    }
}

