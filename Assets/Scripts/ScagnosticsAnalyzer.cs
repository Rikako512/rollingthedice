using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Python.Runtime;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

public class ScagnosticsAnalyzer : MonoBehaviour
{
    public Transform selectedSPs;
    public GameObject prefabSP;
    public GameObject columns;
    private GameObject spawnedselectedSPs;
    private Dropdown dropdown;
    public Text outputText;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();

        if (dropdown != null)
        {
            // ドロップダウンの値が選択されたときのイベントリスナーを追加
            dropdown.onValueChanged.AddListener(SelectionSPs);
        }
        else
        {
            Debug.LogError("Dropdownコンポーネントが見つかりません");
        }

    }

    void SelectionSPs(int value)
    {
        if (outputText != null)
        {
            outputText.text = value.ToString();
        }

        string metric = gameObject.name;
        Debug.Log(metric + "で選択した個数: " + value);

        if (CalculateMetrics.IsInitialized)
        {
            List<int[]> selected_list = CalculateMetrics.top10variables[metric].Take(value).ToList();
            SpawnAllPrefabs(selected_list);
        }
        else
        {
            Debug.LogWarning("Metrics not initialized yet.");
        }
        Debug.Log("---------- 操作：MetricでSelectionを行いました ----------");
    }

    private void DestroySPs()	
    {
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
    }

    private void SpawnAllPrefabs(List<int[]> list)
    {
        DestroySPs();

        if (list != null && list.Count > 0)
        {

            foreach (int[] combination in list)
            {
                if (combination.Length >= 3)
                {
                    int col_x = combination[0];
                    int col_y = combination[1];
                    int col_z = combination[2];

                    Transform columnsTransform = columns.transform;

                    Transform axisChild;
                    Transform targetChild;
                    axisChild = columnsTransform.Find("Y");
                    targetChild = axisChild.Find(col_y.ToString());
                    float position_x = targetChild.localPosition.y +7;
                    axisChild = columnsTransform.Find("Z");
                    targetChild = axisChild.Find(col_z.ToString());
                    float position_y = targetChild.localPosition.y +7;
                    axisChild = columnsTransform.Find("X");
                    targetChild = axisChild.Find(col_x.ToString());
                    float position_z = 0.5f - targetChild.localPosition.y;

                    Debug.Log($"selected by metric: {col_x}, {col_y}, {col_z}");
                    SpawnPrefabs(col_x, col_y, col_z, position_x, position_y, position_z);
                }
                else
                {
                    Debug.LogWarning("Combination does not have enough elements.");
                }
            }
        }
        else
        {
            Debug.LogError("Analysis failed or returned null.");
        }
    }

    private void SpawnPrefabs(int col_X, int col_Y, int col_Z, float position_X, float position_Y, float position_Z)
    {

        // 新しいprefabを生成
        if (prefabSP != null && selectedSPs != null)
        {
            Vector3 spawnPosition = new Vector3(position_X, position_Y, position_Z);
            //Debug.Log($"spawnPosition: {spawnPosition}");
            //Debug.Log(spawnPosition);
            spawnedselectedSPs = Instantiate(prefabSP, selectedSPs);
            spawnedselectedSPs.transform.localPosition = spawnPosition;
            spawnedselectedSPs.name = $"{col_X}, {col_Y}, {col_Z}"; // 名前を "x, y, z" に設定

            Transform graphFrame = spawnedselectedSPs.transform.Find("GraphFrame");
            Transform sp_plotter = graphFrame.Find("Plotter");
            if (sp_plotter != null)
            {
                PointRenderer spawned_pointRenderer = sp_plotter.GetComponent<PointRenderer>();
                if (spawned_pointRenderer != null)
                {
                    spawned_pointRenderer.PlotDataPoints(col_X, col_Y, col_Z);
                }
            }
        }
        else
        {
            Debug.LogWarning("Prefab or selectedSPs is not assigned!");
        }
        
    }
}
