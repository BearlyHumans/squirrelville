using UnityEngine;

public class Clouds : MonoBehaviour
{
    [Tooltip("How fast the clouds rotate")]
    public float rotationSpeed;

    void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * rotationSpeed);
    }
}
