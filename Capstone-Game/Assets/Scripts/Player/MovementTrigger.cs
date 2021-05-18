using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MovementTrigger : MonoBehaviour
{
    private Collider trigger;

    public bool triggered = false;
    public Collider collision;

    // Start is called before the first frame update
    void Awake()
    {
        trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        triggered = true;
    }

    private void OnTriggerStay(Collider other)
    {
        triggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        triggered = false;
    }

    private void OnDisable()
    {
        triggered = false;
    }
}
