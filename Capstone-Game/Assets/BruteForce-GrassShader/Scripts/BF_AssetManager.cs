using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BF_AssetManager : MonoBehaviour
{
    public GameObject UIText;
    public int showcaseIndex = 0;
    [HideInInspector] public int subShowcaseIndex = 0;
    public List<GameObject> showcasesGO;
    public List<GameObject> cameras;
    public List<GameObject> lights;
    private int maxIndex = 4;
    [HideInInspector] public int maxSubIndex = 3;

    [HideInInspector] public UnityEvent m_ShowcaseChange = new UnityEvent();
    // Start is called before the first frame update
    void Start()
    {
        SwitchShowcase(0);
        SwitchSubShowcase(0);
        RenderSettings.fog = true;
        UIText.SetActive(false);
    }

    public void SwitchShowcase(int addIndex)
    {
        for(int i =0;i<=maxIndex;i++)
        {
            showcasesGO[i].SetActive(false);
            cameras[i].SetActive(false);
            lights[i].SetActive(false);
        }
        showcaseIndex += addIndex;
        if(showcaseIndex <= -1)
        {
            showcaseIndex = maxIndex;
        }
        else if(showcaseIndex == maxIndex+1)
        {
            showcaseIndex = 0;
        }
        showcasesGO[showcaseIndex].SetActive(true);
        cameras[showcaseIndex].SetActive(true);
        lights[showcaseIndex].SetActive(true);
        subShowcaseIndex = 0;
        m_ShowcaseChange.Invoke();
    }

    public void SwitchSubShowcase(int addIndex)
    {
        subShowcaseIndex += addIndex;
        if(subShowcaseIndex <= -1)
        {
            subShowcaseIndex = maxSubIndex;
        }
        else if(subShowcaseIndex == maxSubIndex + 1)
        {
            subShowcaseIndex = 0;
        }
        m_ShowcaseChange.Invoke();

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchSubShowcase(-1);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchSubShowcase(1);
        }
    }
}
