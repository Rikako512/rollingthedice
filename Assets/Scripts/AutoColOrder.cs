using UnityEngine;
using System.Collections.Generic;
using Python.Runtime;
using System.Linq;
using System.IO;
using System;
using System.Collections;
using UnityEngine.UI;


public class AutoColOrder : MonoBehaviour
{
    public Transform columnsParent;

    private (List<int>, List<int>) lastResult;
    public Transform SPM; // SPMオブジェクトへの参照
    public Transform SPM_select;

    private static Dictionary<Transform, Vector3> orderedPositions = new Dictionary<Transform, Vector3>();
    private List<Transform>[] linkedQuadsArrayX = new List<Transform>[8];
    private List<Transform>[] linkedQuadsArrayY = new List<Transform>[8];
    private List<Transform>[] linkedQuadsArrayZ = new List<Transform>[8];

    public ResetOrder resetOrder; // ResetOrderクラスへの参照

    private bool isInitialized = false;
    public SpawnedSPsPositionManager SpawnedSPsPositionManager;

    void Start()
    {
        List<Dictionary<string, object>> pointlist = CSVData.pointList;
        lastResult = CalculateFeatureOrders(pointlist);

        Debug.Log("col_order: " + string.Join(", ", lastResult.Item1));
        Debug.Log("row_order: " + string.Join(", ", lastResult.Item2));

        for (int i = 0; i < 8; i++)
        {
            linkedQuadsArrayX[i] = new List<Transform>();
            linkedQuadsArrayY[i] = new List<Transform>();
            linkedQuadsArrayZ[i] = new List<Transform>();
        }


        GetComponent<Button>().onClick.AddListener(AutoOrder);
    }

    private void SaveOrderedPositions()
    {
        orderedPositions.Clear();

        SavePositionsForAxis(linkedQuadsArrayX);
        SavePositionsForAxis(linkedQuadsArrayY);
        SavePositionsForAxis(linkedQuadsArrayZ);
    }

    private void SavePositionsForAxis(List<Transform>[] quadsArray)
    {
        foreach (var list in quadsArray)
        {
            foreach (var quad in list)
            {
                if (quad != null)
                {
                    orderedPositions[quad] = quad.localPosition;
                }
            }
        }
    }

    public void AutoOrder()
    {
        if (!isInitialized)
        {
            CalculateOrderedPositions();
            isInitialized = true;
        }

        ApplyOrderedPositions();

        if (SpawnedSPsPositionManager != null)
        {
            SpawnedSPsPositionManager.ChangeSpawnedSPsPositions();
        }

        Debug.Log("---------- 操作：Auto Order 完了 ----------");
    }

    private void ApplyOrderedPositions()
    {
        foreach (var kvp in orderedPositions)
        {
            if (kvp.Key != null)
            {
                kvp.Key.localPosition = kvp.Value;
            }
        }
    }
 
    void CalculateOrderedPositions()
    {
        // ResetPositionsを呼び出す
        if (resetOrder != null)
        {
            resetOrder.ResetPositions();
        }
        else
        {
            Debug.LogError("ResetOrder is not assigned in the Inspector.");
            return;
        }

        if (columnsParent == null)
        {
            Debug.LogError("Columns Parent is not assigned. Please assign it in the inspector.");
            return;
        }

        FindLinkedQuads("X");
        ReorderColumns(lastResult.Item1, "X");

        FindLinkedQuads("Y");
        ReorderColumns(lastResult.Item1, "Y");

        FindLinkedQuads("Z");
        ReorderColumns(lastResult.Item2, "Z");

        SaveOrderedPositions();

    }

    private void FindLinkedQuads(string parentName)
    {
        if (SPM != null && SPM_select != null && columnsParent != null)
        {
            List<Transform>[] currentArray = null;
            if (parentName == "X")
                currentArray = linkedQuadsArrayX; 
                //currentArrayを通じて行われた変更はlinkedQuadsArrayXにも反映され、linkedQuadsArrayXへの変更もcurrentArrayに反映される。
                //両者は実質的に同じオブジェクトを指す。
            else if (parentName == "Y")
                currentArray = linkedQuadsArrayY;
            else if (parentName == "Z")
                currentArray = linkedQuadsArrayZ;

            for (int i = 0; i < 8; i++)
            {
                string columnName = i.ToString();
                if (parentName == "X")
                {
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Right", columnName, 1));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Floor", columnName, 1));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Right", columnName, 1));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Floor", columnName, 1));
                }
                else if (parentName == "Y")
                {
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Left", columnName, 1));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Floor", columnName, 0));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Left", columnName, 1));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Floor", columnName, 0));
                }
                else if (parentName == "Z")
                {
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Right", columnName, 0));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM, "Left", columnName, 0));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Right", columnName, 0));
                    currentArray[i].AddRange(FindAllQuadsInChild(SPM_select, "Left", columnName, 0));
                }

                // Columnsの子オブジェクトを追加
                Transform columnParent = columnsParent.Find(parentName);
                if (columnParent != null)
                {
                    Transform columnChild = columnParent.Find(i.ToString());
                    if (columnChild != null)
                    {
                        currentArray[i].Add(columnChild);
                    }
                }
            }
        }
    }

	
    private List<Transform> FindAllQuadsInChild(Transform parent, string childName, string columnName, int indexToCheck)
    {
        List<Transform> quads = new List<Transform>();
        Transform child = parent.Find(childName);
        if (child != null)
        {
            quads.AddRange(child.GetComponentsInChildren<Transform>()
                .Where(t => {
                    string[] parts = t.name.Split(',');
                    return parts.Length == 2 && parts[indexToCheck].Trim() == columnName;
                }));
        }
        return quads;
    }

    private void ReorderColumns(List<int> result_order, string axisName)
    {
        List<Transform>[] currentArray;
        switch (axisName)
        {
            case "X":
                currentArray = linkedQuadsArrayX;
                break;
            case "Y":
                currentArray = linkedQuadsArrayY;
                break;
            case "Z":
                currentArray = linkedQuadsArrayZ;
                break;
            default:
                Debug.LogError($"Invalid axis name: {axisName}");
                return;
        }

        List<Transform>[] originalArray = currentArray.Clone() as List<Transform>[];
        
        // 初期位置を保存
        Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
        foreach (var list in originalArray)
        {
            foreach (var quad in list)
            {
                initialPositions[quad] = resetOrder.GetInitialPosition(quad);
            }
        }

        for (int i = 0; i < result_order.Count; i++)
        {
            currentArray[i] = originalArray[result_order[i]];

            // Quadの位置を実際に変更
            for (int j = 0; j < currentArray[i].Count; j++)
            {
                Transform quad = currentArray[i][j];
                //Debug.Log($"NOW: {quad.name}");
                // 初期位置を取得
                Vector3 initialPosition = initialPositions[quad];
                Vector3 newPosition = initialPosition;

                switch (axisName)
                {
                    case "X":
                        if (j == currentArray[i].Count - 1)
                        {
                            newPosition.y = -i;
                        }
                        else
                        {
                            newPosition.x = 7.0f - i;
                        }
                        break;
                    case "Y":
                        if (j == currentArray[i].Count - 1)
                        {
                            newPosition.y = i - 7.0f;
                        }
                        else
                        {
                            if (quad.CompareTag("floor"))
                            {
                                newPosition.y = 7.0f - i;
                            }
                            else
                            {
                                newPosition.x = i;
                            }
                        }
                        break;
                    case "Z":
                        if (j == currentArray[i].Count - 1)
                        {
                            newPosition.y = i - 7.0f;
                        }
                        else
                        {
                            newPosition.y = i;
                        }
                        break;
                }

                // 変更された座標のみを更新
                if (quad.parent != null)
                {
                    Vector3 localPosition = quad.localPosition;
                    if (axisName == "X")
                    {
                        if (j == currentArray[i].Count - 1)
                        {
                            localPosition.y = newPosition.y;
                        }
                        else
                        {
                            localPosition.x = newPosition.x;
                        }
                    }
                    else if (axisName == "Y")
                    {
                        if (j == currentArray[i].Count - 1 || quad.CompareTag("floor"))
                        {
                            localPosition.y = newPosition.y;
                        }
                        else
                        {
                           localPosition.x = newPosition.x;
                        }
                    }
                    else if (axisName == "Z")
                    {
                        localPosition.y = newPosition.y;
                    }
                    quad.localPosition = localPosition;
                }

            }
        }
    }

    private bool IsValidCName(string name)
    {
        string[] parts = name.Split(',');
        if (parts.Length != 3) return false;
        
        return parts.All(part => 
            part.Trim().Length == 1 && char.IsDigit(part.Trim()[0]));
    }


    private (List<int>, List<int>) CalculateFeatureOrders(List<Dictionary<string, object>> pointlist)
    {
        // Extract feature names
        var featureNames = pointlist[0].Keys.ToList();

        // Convert pointlist to 2D array
        double[,] X = new double[pointlist.Count, featureNames.Count];
        for (int i = 0; i < pointlist.Count; i++)
        {
            for (int j = 0; j < featureNames.Count; j++)
            {
                X[i, j] = Convert.ToDouble(pointlist[i][featureNames[j]]);
            }
        }

        // Calculate Spearman correlation
        double[,] corrMatrix = CalculateSpearmanCorrelation(X);

        // Calculate similarity and dissimilarity metrics
        int[,] D = new int[corrMatrix.GetLength(0), corrMatrix.GetLength(1)];
        int[,] S = new int[corrMatrix.GetLength(0), corrMatrix.GetLength(1)];
        for (int i = 0; i < corrMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < corrMatrix.GetLength(1); j++)
            {
                D[i, j] = (int)(Math.Abs(corrMatrix[i, j]) * 1000); // Scale to integer
                S[i, j] = (int)((1 - Math.Abs(corrMatrix[i, j])) * 1000); // Scale to integer
            }
        }

        // Solve TSP problem using the new TSPSolver
        TSPSolver colSolver = new TSPSolver(S);
        TSPSolver rowSolver = new TSPSolver(D);

        int colCost = colSolver.Solve();
        int rowCost = rowSolver.Solve();

        List<int> colOrder = ReconstructPath(colSolver);
        List<int> rowOrder = ReconstructPath(rowSolver);

        return (colOrder, rowOrder);
    }

    private List<int> ReconstructPath(TSPSolver solver)
    {
        List<int> path = new List<int>();
        int current = 0;
        int visited = 1;

        for (int i = 0; i < solver.n; i++)
        {
            path.Add(current);
            int bestNext = -1;
            int bestCost = int.MaxValue;

            for (int next = 0; next < solver.n; next++)
            {
                if ((visited & (1 << next)) == 0)
                {
                    int newCost = solver.distances[current, next] + solver.TSP(next, visited | (1 << next));
                    if (newCost < bestCost)
                    {
                        bestCost = newCost;
                        bestNext = next;
                    }
                }
            }

            if (bestNext == -1) break;
            current = bestNext;
            visited |= (1 << current);
        }

        return path;
    }

    private double[,] CalculateSpearmanCorrelation(double[,] X)
    {
        int n = X.GetLength(0);
        int m = X.GetLength(1);
        double[,] corrMatrix = new double[m, m];

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (i == j)
                {
                    corrMatrix[i, j] = 1;
                }
                else
                {
                    double[] xi = Enumerable.Range(0, n).Select(k => X[k, i]).ToArray();
                    double[] xj = Enumerable.Range(0, n).Select(k => X[k, j]).ToArray();
                    corrMatrix[i, j] = SpearmanCorrelation(xi, xj);
                }
            }
        }

        return corrMatrix;
    }

    private double SpearmanCorrelation(double[] x, double[] y)
    {
        int n = x.Length;
        var xRank = GetRanks(x);
        var yRank = GetRanks(y);

        double sumDiffSquared = 0;
        for (int i = 0; i < n; i++)
        {
            double diff = xRank[i] - yRank[i];
            sumDiffSquared += diff * diff;
        }

        return 1 - (6 * sumDiffSquared) / (n * (n * n - 1));
    }

    private double[] GetRanks(double[] x)
    {
        var sorted = x.Select((value, index) => new { Value = value, Index = index })
                      .OrderBy(item => item.Value)
                      .ToList();

        double[] ranks = new double[x.Length];
        for (int i = 0; i < sorted.Count; i++)
        {
            ranks[sorted[i].Index] = i + 1;
        }

        return ranks;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < 8; i++)
        {
            linkedQuadsArrayX[i].Clear();
            linkedQuadsArrayX[i] = null;
            linkedQuadsArrayY[i].Clear();
            linkedQuadsArrayY[i] = null;
            linkedQuadsArrayZ[i].Clear();
            linkedQuadsArrayZ[i] = null;
        }
    }

}

public class TSPSolver
{
    public int[,] distances;
    public int n;
    private int[,] memo;

    public TSPSolver(int[,] distances)
    {
        this.distances = distances;
        this.n = distances.GetLength(0);
        this.memo = new int[n, 1 << n];
    }

    public int Solve()
    {
        return TSP(0, 1);
    }

    public int TSP(int pos, int mask)
    {
        if (mask == (1 << n) - 1)
            return distances[pos, 0];

        if (memo[pos, mask] != 0)
            return memo[pos, mask];

        int ans = int.MaxValue;
        for (int city = 0; city < n; city++)
        {
            if ((mask & (1 << city)) == 0)
            {
                int newAns = distances[pos, city] + TSP(city, mask | (1 << city));
                ans = Math.Min(ans, newAns);
            }
        }

        memo[pos, mask] = ans;
        return ans;
    }
}
