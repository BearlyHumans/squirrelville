using UnityEngine;

public class Warp : MonoBehaviour
{
    public Player.SquirrelController player;
    public CameraGimbal cam;
    public Transform location;

    private void OnTriggerEnter(Collider other)
    {
        cam.transform.position = location.position;
        player.transform.position = location.position;
    }
}
