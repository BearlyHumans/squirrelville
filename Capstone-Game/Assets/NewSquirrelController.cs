using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NewSquirrelController : MonoBehaviour
{
    private Rigidbody rb;

    private 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + new Vector3(0, -0.1f, 0.04f));
    }
}
