using UnityEngine;

public class Roundabout : MonoBehaviour
{
    [Tooltip("How fast the roundabout rotates")]
    public float speed = 30.0f;

    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
