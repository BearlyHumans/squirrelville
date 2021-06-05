using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour
{

    public GameObject player;
    public GameObject camera;
    public GameObject location;
    
   
    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        camera.transform.position = location.transform.position;
        player.transform.position = location.transform.position;
        
        
    }
}
