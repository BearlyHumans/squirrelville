using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ObjectArea : MonoBehaviour
{
    private Collider trigger;
    private List<GameObject> objectsInArea = new List<GameObject>();
    private GameObject[] oldObjectsInArea = new GameObject[0];
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
        foreach (GameObject obj in oldObjectsInArea)
        {
            if (obj == null || !obj.activeInHierarchy)
            {
                objectsInArea.Remove(obj);

                if (exitEvent != null)
                {
                    exitEvent.Invoke(obj);
                }
            }
        }

        oldObjectsInArea = objectsInArea.ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ObjectMatchesMask(other.gameObject) && objectsInArea.IndexOf(other.gameObject) == -1)
        {
            objectsInArea.Add(other.gameObject);

            if (enterEvent != null)
            {
                enterEvent.Invoke(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ObjectMatchesMask(other.gameObject) && objectsInArea.IndexOf(other.gameObject) > -1)
        {
            objectsInArea.Remove(other.gameObject);

            if (exitEvent != null)
            {
                exitEvent.Invoke(other.gameObject);
            }
        }
    }

    public GameObject[] GetObjectsInArea()
    {
        return objectsInArea.ToArray();
    }

    public int GetObjectCount()
    {
        return objectsInArea.Count;
    }

    private bool ObjectMatchesMask(GameObject obj)
    {
        return layerMask == (layerMask | (1 << obj.layer));
    }
}
