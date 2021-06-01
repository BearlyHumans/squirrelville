using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ObjectArea : MonoBehaviour
{
    private Collider trigger;
    private GameObject[] oldObjectsInArea;
    public LayerMask layerMask;

    public UnityEvent<GameObject> enterEvent;
    public UnityEvent<GameObject> exitEvent;

    private void Start()
    {
        trigger = GetComponent<Collider>();
    }

    private void Update()
    {
        // Detect when food is eaten from within the area
        if (oldObjectsInArea != null)
        {
            foreach (GameObject obj in oldObjectsInArea)
            {
                if (!obj.activeInHierarchy && exitEvent != null)
                {
                    exitEvent.Invoke(obj);
                }
            }
        }

        oldObjectsInArea = GetObjectsInArea();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ObjectMatchesMask(other.gameObject) && enterEvent != null)
        {
            enterEvent.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ObjectMatchesMask(other.gameObject) && exitEvent != null)
        {
            exitEvent.Invoke(other.gameObject);
        }
    }

    public GameObject[] GetObjectsInArea()
    {

        GameObject[] objects = FindObjectsOfType<GameObject>();
        List<GameObject> objectsInArea = new List<GameObject>();

        if (trigger != null)
        {
            foreach (GameObject obj in objects)
            {
                if (ObjectMatchesMask(obj))
                {
                    Collider objCollider = obj.GetComponent<Collider>();
                    if (objCollider != null)
                    {
                        Bounds objBounds = objCollider.bounds;
                        if (trigger.bounds.Intersects(objBounds))
                        {
                            objectsInArea.Add(obj);
                        }
                    }
                }
            }
        }

        return objectsInArea.ToArray();
    }

    public int GetObjectCount()
    {
        return GetObjectsInArea().Length;
    }

    private bool ObjectMatchesMask(GameObject obj)
    {
        return layerMask == (layerMask | (1 << obj.layer));
    }
}
