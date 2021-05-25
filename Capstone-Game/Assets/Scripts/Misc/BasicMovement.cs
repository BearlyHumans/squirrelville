using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 motion = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            motion += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            motion = Vector3.back;
        }
        if (Input.GetKey(KeyCode.D))
        {
            motion += Vector3.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            motion += Vector3.left;
        }

        GetComponent<Rigidbody>().velocity = motion*5;
    }
}
