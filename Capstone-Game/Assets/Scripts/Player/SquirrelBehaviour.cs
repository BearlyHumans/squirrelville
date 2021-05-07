using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SquirrelControllerSettings;
using Player;

namespace Player
{
    [RequireComponent(typeof(SquirrelController))]
    public abstract class SquirrelBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void ManualUpdate()
        {

        }
    }
}
