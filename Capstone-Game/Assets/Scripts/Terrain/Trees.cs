using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trees : MonoBehaviour
{
    
    
    public ParticleSystem leaves;
    public bool hasLeaves;
    public float fallRate;

    // Start is called before the first frame update
    void Start()
    {
        // setting leaves particle systems emission to the desired rate.
        var emission = leaves.emission;
        emission.rateOverTime = fallRate;

        // correctly setting the position of the instantiated particle effect in the tree
        Vector3 pos = transform.position;
        pos.y = 7.5f;
        var rot = transform.rotation;
        rot.x = 1.0f;

        // if tree has falling leaves
        if (hasLeaves)
        {
            // instaniate new leaf particle 
            ParticleSystem leavesParticle = Instantiate(leaves, pos, rot);
        }
            
    }
}
