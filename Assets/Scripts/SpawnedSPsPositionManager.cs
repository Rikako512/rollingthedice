using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedSPsPositionManager : MonoBehaviour
{
    public void ChangeSpawnedSPsPositions()
    {
        GameObject spawnSPs = GameObject.FindGameObjectWithTag("SpawnSPs");
        if (spawnSPs != null && spawnSPs.transform.childCount > 0)
        {
            Transform spawnedSP = spawnSPs.transform.GetChild(0);
            ChangePosition(spawnedSP);
        }
        else
        {
            Debug.LogWarning("SpawnSPs is null or has no children");
        }

        GameObject selectionSPs = GameObject.FindGameObjectWithTag("SelectedSPs");
        if (selectionSPs != null && selectionSPs.transform.childCount > 0)
        {
            foreach (Transform child in selectionSPs.transform)
            {
                ChangePosition(child);
            }
        }
        else
        {
            Debug.LogWarning("SelectedSPs is null or has no children");
        }

    }

    private void ChangePosition(Transform oneSP)
    {
        if (oneSP != null)
        {
            Vector3 newPosition = CalculatePosition(oneSP.name);
            oneSP.localPosition = newPosition;
        }
        else
        {
            Debug.LogWarning("SP is not found!");
        }
    }

    public Vector3 CalculatePosition(string spawnedSPName)
    {
        GameObject columnsparent = GameObject.FindGameObjectWithTag("column_parent");
        Transform columnsTransform = columnsparent.transform;

        string[] numbers = spawnedSPName.Split(',');

        int col_X = 0;
        int col_Y = 0;
        int col_Z = 0;

        if (numbers.Length == 3)
        {
            col_X = int.Parse(numbers[0].Trim());
            col_Y = int.Parse(numbers[1].Trim());
            col_Z = int.Parse(numbers[2].Trim());
        }
            
        Transform axisChild;
        Transform targetChild;
        axisChild = columnsTransform.Find("Y");
        targetChild = axisChild.Find(col_Y.ToString());
        float position_x = targetChild.localPosition.y + 7;
        axisChild = columnsTransform.Find("Z");
        targetChild = axisChild.Find(col_Z.ToString());
        float position_y = targetChild.localPosition.y + 7;
        axisChild = columnsTransform.Find("X");
        targetChild = axisChild.Find(col_X.ToString());
        float position_z = 0.5f - targetChild.localPosition.y;

        return new Vector3(position_x, position_y, position_z);
    }

}
