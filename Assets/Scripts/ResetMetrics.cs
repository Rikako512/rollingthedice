using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ResetMetrics : MonoBehaviour
{
    public Transform selectedSPs;
    public Text outputText01;
    public Text outputText02;
    public Text outputText03;
    public Text outputText04;


    void Start()
    {
        GetComponent<Button>().onClick.AddListener(DestroySelectedSPs);
    }

    void DestroySelectedSPs()	
    {
        outputText01.text = "0";
        outputText02.text = "0";
        outputText03.text = "0";
        outputText04.text = "0";
        
        if (selectedSPs != null)
        {
            var children = selectedSPs.Cast<Transform>().ToList();

            foreach (var child in children)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.Log("SelectedSPs is null.");
        }

        Debug.Log("---------- 操作：ResetMetricボタンを押しました ----------");
    }

}
