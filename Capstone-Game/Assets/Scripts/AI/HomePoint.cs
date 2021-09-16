using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePoint : MonoBehaviour
{
    
    public float boundary = 10.0f;

    [Tooltip("list of bins that player can walk to")]
    public List<Bin> bins;


    public Bin closestBin(Vector3 humanPos)
    {
        float bestDistance = 9999.0f;
        Bin bestBin = null;

        foreach (Bin bin in bins)
        {
            float distToBin = Vector3.Distance(bin.transform.position, humanPos);
            print(distToBin);
            if (distToBin < bestDistance)
            {
                bestDistance = distToBin;
                bestBin = bin;
            }
        }
        return bestBin;
    }

    public virtual void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, boundary); 
    }
}

