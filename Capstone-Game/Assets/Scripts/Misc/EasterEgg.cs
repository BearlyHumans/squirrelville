using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEgg : MonoBehaviour
{
    static int numFound = 0;

    public ParticleSystem particleEffect;
    public AnimationCurve wiggle = new AnimationCurve();
    public enum Axis { x, y, z, none }
    public Axis axis = Axis.y;
    public Transform visibleObject;
    public AudioSource audioSource;
    public float destroyTime = 5f;

    private Vector3 objectRelPos = Vector3.zero;
    public bool found = false;

    void Awake()
    {
        objectRelPos = visibleObject.position;
    }

    void Update()
    {
        if (axis == Axis.none)
            return;

        Vector3 wiggleAxis = Vector3.up;
        if (axis == Axis.x)
            wiggleAxis = Vector3.right;
        else if (axis == Axis.z)
            wiggleAxis = Vector3.forward;
        visibleObject.position = objectRelPos + wiggleAxis * wiggle.Evaluate(Time.time);
    }

    private void OnTriggerEnter(Collider other)
    {
        Collect();
    }

    private void Collect()
    {
        if (!found)
        {
            found = true;
            numFound += 1;
            particleEffect.Play();
            audioSource.Play();
            visibleObject.gameObject.SetActive(false);
            Destroy(gameObject, destroyTime);
        }
    }
}
