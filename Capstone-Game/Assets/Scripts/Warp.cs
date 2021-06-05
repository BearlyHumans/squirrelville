using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour
{

    public GameObject player;
    public GameObject location;
    // Start is called before the first frame update
   
    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        player.transform.position = new Vector3(location.transform.position.x, location.transform.position.y + 10f, location.transform.position.z);
    }
}
