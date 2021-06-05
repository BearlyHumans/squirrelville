using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_FxOnMouseClick : MonoBehaviour
{
    private Camera mainCam;
    public ParticleSystem ps;
    public ParticleSystem psRightClick;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit,100))
            {
                if (!ps.isEmitting)
                {
                    ps.Play();
                }
                ps.transform.position = hit.point;
            }
            else
            {
               // ps.Stop();
            }
        }
        else
        {
            ps.Stop();
        }

        if (Input.GetMouseButton(1))
        {
            RaycastHit hit;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(this.transform.position, ray.direction);
            if (Physics.Raycast(ray, out hit,100))
            {
                if (!psRightClick.isEmitting)
                {
                    psRightClick.Play();
                }
                psRightClick.transform.position = hit.point;
            }
            else
            {
               // ps.Stop();
            }
        }
        else
        {
            psRightClick.Stop();
        }
    }
}
