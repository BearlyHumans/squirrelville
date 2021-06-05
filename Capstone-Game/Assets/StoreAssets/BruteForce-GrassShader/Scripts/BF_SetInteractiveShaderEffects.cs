using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BF_SetInteractiveShaderEffects : MonoBehaviour
{
    public Transform transformToFollow;
    public RenderTexture rt;
    public string GlobalTexName = "_GlobalEffectRT";
    public string GlobalOrthoName = "_OrthographicCamSize";
    private float orthoMem = 0;
    
    private void Awake()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
        Shader.SetGlobalFloat("_HasRT", 1);
    }
    private void Update()
    {
        if (transformToFollow != null)
        {
            transform.position = new Vector3(transformToFollow.position.x, transformToFollow.position.y + 20, transformToFollow.position.z);
        }
        Shader.SetGlobalVector("_Position", transform.position);
    }
}