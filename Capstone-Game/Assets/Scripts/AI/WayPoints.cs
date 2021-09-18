using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{
    [SerializeField]
    protected float radius = 0.1f;


    [SerializeField]
    public float waitForThisLong = 0f;
    
    public virtual void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius); 
    }

}
