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
    public float fallRate;

    public float fallingRadius = 0.4f;

    public Color color; 

    public float hSliderValueR = 10.0F;
    public float hSliderValueG = 10.0F;
    public float hSliderValueB = 10.0F;
    public float hSliderValueA = 11.0F;

    // Start is called before the first frame update
    void Start()
    {
        // changes the radius in which leafs drop based on float ^
        var shape = leaves.shape;
        shape.scale = new Vector3(fallingRadius, fallingRadius, 1.0f);

        // setting leaves particle systems emission to the desired rate.
        var emission = leaves.emission;
        emission.rateOverTime = fallRate;

        //ParticleSystem.MainModule settings = leaves.main;
        //settings.startColor = new ParticleSystem.MinMaxGradient(color);

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
