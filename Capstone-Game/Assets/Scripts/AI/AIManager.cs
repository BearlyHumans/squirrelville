using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class AIManager : MonoBehaviour
{
    public static AIManager singleton;
    public SquirrelController sController;
    // Start is called before the first frame update
    void Awake()
    {
        if(singleton != null)
        {
            Destroy(gameObject);
            Debug.LogError("Jake doesnt like my poor quality code");
        }
        else 
        {
            singleton = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
