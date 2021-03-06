using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trees : MonoBehaviour
{
    public ParticleSystem leaves;

    [Tooltip("Do you want this tree to have falling leaves?")]
    [SerializeField]
    public bool hasLeaves;

    [Tooltip("how fast do you want the leaves to fall?")]
    [SerializeField]
    public float fallRate = 0.5f;

    [Tooltip("how far up the tree do the leaves start?")]
    [SerializeField]
    public float particleHeight = 7.5f;

    // float to determine the radius of the particle system
    public float fallingRadius = 0.4f;

    // custom color
    public Color leafColor; 

    // ensure thats leaves are always viable
    private float alphaLevel = 255.0F;

    // Start is called before the first frame update
    void Awake()
    {
        // create empty game object for leaf particles to join
        if (GameObject.Find("leavesHolder") == null)
        {
            new GameObject("leavesHolder");   
        }
    }
    void Start()
    {
        GameObject leavesParent = GameObject.Find("leavesHolder");
        
        // set leaves alpha color to max
        leafColor[3] = alphaLevel;

        // changes the radius in which leafs drop based on float ^
        var shape = leaves.shape;
        shape.scale = new Vector3(fallingRadius, fallingRadius, 1.0f);
        shape.rotation = new Vector3(90.0f, 0.0f, 0.0f);

        // setting leaves particle systems emission to the desired rate.
        var emission = leaves.emission;
        emission.rateOverTime = fallRate;

        // setting color of particle system 
        var color = leaves.main;
        color.startColor = new Color(leafColor[0],leafColor[1],leafColor[2],leafColor[3]);

        // correctly setting the position of the instantiated particle effect in the tree
        Vector3 pos = transform.position;
        pos.y += particleHeight;

        // setting rotation of particle system to point downwards
        //var rot = transform.rotation;
        //rot.y = -90.0f;

        // if tree has falling leaves
        if (hasLeaves)
        {
            // instaniate new leaf particle 
            if (leavesParent != null)
            {
                ParticleSystem leavesParticle = Instantiate(leaves, pos, transform.rotation);
                leavesParticle.transform.parent = leavesParent.transform;
            }
        }
            
    }
}
