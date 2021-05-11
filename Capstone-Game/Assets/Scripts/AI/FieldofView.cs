using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldofView : MonoBehaviour
{
    public Transform target;

    // prints "close" if the z-axis of this transform looks
    // almost towards the target

    void Update()
    {
        Vector3 targetDir = target.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        if (angle < 5.0f)
            Debug.Log("close");
    }
}
