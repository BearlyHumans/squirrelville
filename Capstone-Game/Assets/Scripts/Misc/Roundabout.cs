using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundabout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(0, 0, 0 * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 2, 0 * Time.deltaTime);

    }
}
