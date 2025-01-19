using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetOrder : MonoBehaviour
{
    public GameObject SPM;
    public Transform SPM_select;

    private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
    public SpawnedSPsPositionManager SpawnedSPsPositionManager;


    void Start()
    {
        if (SPM != null && SPM_select != null)
        {
            // SPMの全ての子孫オブジェクトの初期位置を保存
            SaveInitialPositions(SPM.transform);
            
            // SPM_selectの全ての子孫オブジェクトの初期位置を保存
            SaveInitialPositions(SPM_select);
        }
        else
        {
            Debug.LogError("SPM or SPM_select is not assigned in the Inspector.");
        }

        GetComponent<Button>().onClick.AddListener(ResetPositions);
    }

    void SaveInitialPositions(Transform parent)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            initialPositions[child] = child.localPosition;
        }
    }

    public void ResetPositions()
    {
        // 全ての保存されたオブジェクトを初期位置に戻す
        foreach (var kvp in initialPositions)
        {
            kvp.Key.localPosition = kvp.Value;
        }

        if (SpawnedSPsPositionManager != null)
        {
            SpawnedSPsPositionManager.ChangeSpawnedSPsPositions();
        }
        
        Debug.Log("---------- 操作：ResetOrderボタンを押しました ----------");
    }

    public Vector3 GetInitialPosition(Transform transform)
    {
        if (initialPositions.TryGetValue(transform, out Vector3 position))
        {
            return position;
        }
        return transform.localPosition;
    }

}
