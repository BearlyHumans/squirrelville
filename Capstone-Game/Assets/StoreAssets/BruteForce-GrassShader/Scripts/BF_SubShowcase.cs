using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BF_SubShowcase : MonoBehaviour
{
    public BF_AssetManager aM;
    public Text uiText;
    public List<GameObject> subShowcases;
    public List<string> nameSubs;
    private int oldIndex = -1;

    private UnityAction showcaseChange;

    // Start is called before the first frame update
    private void OnEnable()
    {
        aM.maxSubIndex = subShowcases.Count-1;
        aM.m_ShowcaseChange.AddListener(ChangeIndex);
    }
    private void OnDisable()
    {
        aM.m_ShowcaseChange.RemoveListener(ChangeIndex);
    }
    void Update()
    {
        /*
        if(aM.subShowcaseIndex != oldIndex)
        {
            oldIndex = aM.subShowcaseIndex;
            foreach(GameObject GO in subShowcases)
            {
                GO.SetActive(false);
            }
            subShowcases[oldIndex].SetActive(true);
            uiText.text = nameSubs[oldIndex];
        }
        */
    }

    private void ChangeIndex()
    {
        oldIndex = aM.subShowcaseIndex;

        foreach (GameObject GO in subShowcases)
        {
            GO.SetActive(false);
        }
        subShowcases[oldIndex].SetActive(true);
        uiText.text = nameSubs[oldIndex];
    }

}
