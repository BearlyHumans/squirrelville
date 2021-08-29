using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Player
{
    [CustomEditor(typeof(SquirrelController))]
    public class SquirrelControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}
