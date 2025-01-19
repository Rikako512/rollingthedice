using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class QuadInteractionHandler : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    public Material highlightedMaterial;
    public Material selectedMaterial;
    public Material subselectedMaterial;
    public Material originalMaterial;
    private int[] clickedNum = new int[2];

    private List<Renderer> affectedRenderers = new List<Renderer>();
    private bool isHovered = false;
    private static List<Renderer> selectedRenderers = new List<Renderer>();

    // 静的変数で選択されたQuadの情報を保持
    private static QuadInteractionHandler selectedQuad = null;
    private static string selectedParentName = null;

    private static int col_X = -1;
    private static int col_Y = -1;
    private static int col_Z = -1;

    private Vector3 relativePositionToParent;
    private static float position_X;
    private static float position_Y;
    private static float position_Z;

    public GameObject plotter; //大きい方のSP
    private PointRenderer pointRenderer; //大きい方のSP
    public TextMeshProUGUI xAxisText;
    public TextMeshProUGUI yAxisText;
    public TextMeshProUGUI zAxisText;

    public GameObject spawnSPs;
    public GameObject prefabSP; //popさせるミニSP
    private static GameObject spawnedPrefab;

    public GameObject pointcontainer;


    void Start()
    {
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(OnQuadSelected);
        simpleInteractable.selectExited.AddListener(OnQuadSelectExited);
        simpleInteractable.hoverEntered.AddListener(OnQuadHoverEntered);
        simpleInteractable.hoverExited.AddListener(OnQuadHoverExited);

        // PointRendererコンポーネントを取得
        if (plotter != null)
        {
            pointRenderer = plotter.GetComponent<PointRenderer>();
            if (pointRenderer == null)
            {
                Debug.LogError("PointRenderer component not found on Plotter object");
            }
        }
        else
        {
            Debug.LogError("Plotter object not assigned");
        }
    }

    private void OnQuadSelected(SelectEnterEventArgs args)
    {
        // 前の選択をリセット
        ResetAllHighlights();

        selectedQuad = this;
        selectedParentName = transform.parent?.name ?? "No parent";
        Debug.Log($"---------- 操作：Quad: {selectedParentName} {gameObject.name} を選択しました ----------");

        // 親に対する相対座標を出力
        if (transform.parent != null)
        {
            relativePositionToParent = transform.localPosition;
            //Debug.Log($"Quad Position (Relative to Parent): {relativePositionToParent}");
        }

        // 新しく追加: 選択されたQuadの値を解析して代入
        AssignColumnValues(gameObject.name, selectedParentName);
        
        // 新しい選択に基づいてハイライトを適用
        //HighlightRelatedQuads(selectedParentName, true);
        UpdateQuadState(gameObject.name, true);
        UpdateXAxisText();
    }
        

    private void OnQuadSelectExited(SelectExitEventArgs args)
    {
        // 選択解除時には何もしない（次の選択まで維持）
        //Debug.Log($"Quad {gameObject.name} deselected");
    }

    private void OnQuadHoverEntered(HoverEnterEventArgs args)
    {
        isHovered = true;
        //Debug.Log($"Quad {gameObject.name} hover entered");
        selectedParentName = transform.parent?.name ?? "No parent";
        UpdateQuadState(gameObject.name, false);
    }

    private void OnQuadHoverExited(HoverExitEventArgs args)
    {
        isHovered = false;
        //Debug.Log($"Quad {gameObject.name} hover exited");
        UpdateQuadState(gameObject.name, false);
    }

    private void UpdateQuadState(string quadName, bool isSelecting)
    {
        ParseQuadName(quadName);
        HighlightRelatedQuads(selectedParentName, isSelecting, isHovered);
    }

    private void AssignColumnValues(string quadName, string parentName)
    {
        string[] parts = quadName.Split(',');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int value1) || !int.TryParse(parts[1], out int value2))
        {
            Debug.LogError($"Invalid quad name format: {quadName}");
            return;
        }

        switch (parentName)
        {
            case "Right":
                col_Z = value1;
                col_X = value2;
                position_Y = (float)relativePositionToParent.y;
                position_Z = 7.5f - (float)relativePositionToParent.x;
                break;
            case "Left":
                col_Z = value1;
                col_Y = value2;
                position_X = (float)relativePositionToParent.x;
                position_Y = (float)relativePositionToParent.y;
                break;
            case "Floor":
                col_X = value2;
                col_Y = value1;
                position_Z = 7.5f - (float)relativePositionToParent.x;
                position_X = 7f - (float)relativePositionToParent.y;
                break;
            default:
                Debug.LogError($"Unknown parent name: {parentName}");
                break;
        }

        Debug.Log($"Updated column values: X={col_X}, Y={col_Y}, Z={col_Z}");

        // 全ての値が0以上になったかチェック
        if (col_X >= 0 && col_Y >= 0 && col_Z >= 0)
        {
            UpdatePointRenderer();
            CheckAndSpawnPrefab();
        }
    }

    private void UpdateXAxisText()
    {
        List<string> columnList = new List<string>(CSVData.pointList[1].Keys);

        xAxisText.text = $"X axis: {columnList[col_X]}";
        yAxisText.text = $"Y axis: {columnList[col_Y]}";
        zAxisText.text = $"Z axis: {columnList[col_Z]}";
    }

    // Scatterplotの軸の変数を変更
    private void UpdatePointRenderer()
    {
        if (pointRenderer != null)
        {
            if (pointcontainer != null && pointcontainer.transform.childCount == 0)
            {
                pointRenderer.PlotDataPoints(col_X, col_Y, col_Z);
            }
            else
            {
                pointRenderer.UpdateDataPoints(col_X, col_Y, col_Z);
            }
            Debug.Log($"Updated PointRenderer: column1={col_X}, column2={col_Y}, column3={col_Z}");
        }
        else
        {
            Debug.LogError("PointRenderer is not assigned");
        }
    }

    private void CheckAndSpawnPrefab()
    {
        // 既存のprefabを削除
        if (spawnedPrefab != null)
        {
            Destroy(spawnedPrefab);
            spawnedPrefab = null; // nullに設定して確実に参照を解除
        }

        // 新しいprefabを生成
        if (prefabSP != null && spawnSPs != null)
        {
            Vector3 spawnPosition = new Vector3(position_X, position_Y, position_Z);
            //Debug.Log(spawnPosition);
            spawnedPrefab = Instantiate(prefabSP, spawnSPs.transform);
            spawnedPrefab.transform.localPosition = spawnPosition;
            spawnedPrefab.name = $"{col_X}, {col_Y}, {col_Z}"; // 名前を "x, y, z" に設定

            Transform graphFrame = spawnedPrefab.transform.Find("GraphFrame");
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
            Debug.LogWarning("Prefab or spawnSPs is not assigned!");
        }
        
    }

    
    private void ParseQuadName(string name)
    {
        string[] parts = name.Split(',');
        if (parts.Length == 2 && int.TryParse(parts[0], out clickedNum[0]) && int.TryParse(parts[1], out clickedNum[1]))
        {
            //Debug.Log($"Parsed numbers: {clickedNum[0]}, {clickedNum[1]}");
        }
        else
        {
            Debug.LogError($"Failed to parse quad name: {name}");
        }
    }
    
    private void HighlightRelatedQuads(string parent, bool isSelecting, bool isHovering)
    {
        if (isSelecting)
        {
            ResetAllHighlights();
        }
        else
        {
            ResetHighlights();
        }

        Transform scatterplotMatrix = transform.parent.parent;
        Transform leftQuads = scatterplotMatrix.Find("Left");
        Transform rightQuads = scatterplotMatrix.Find("Right");
        Transform floorQuads = scatterplotMatrix.Find("Floor");

        // col_x, col_y, col_zに基づいてQuadをハイライト
        
        if (col_X != -1)
        {
            HighlightQuads(rightQuads, 1, 0, isSelecting, isHovering, col_X);
            HighlightQuads(floorQuads, 1, 0, isSelecting, isHovering, col_X);
        }
        if (col_Y != -1)
        {
            HighlightQuads(leftQuads, 1, 0, isSelecting, isHovering, col_Y);
            HighlightQuads(floorQuads, 0, 0, isSelecting, isHovering, col_Y);
        }
        if (col_Z != -1)
        {
            HighlightQuads(leftQuads, 0, 0, isSelecting, isHovering, col_Z);
            HighlightQuads(rightQuads, 0, 0, isSelecting, isHovering, col_Z);
        }

        if(isHovering)
        {
            if(parent == "Right")
            {
                HighlightQuads(rightQuads, -1, -1, isSelecting, isHovering, null);
                HighlightQuads(leftQuads, 0, 0, isSelecting, isHovering, null);
                HighlightQuads(floorQuads, 1, 1, isSelecting, isHovering, null);
            }
            if(parent == "Left")
            {
                HighlightQuads(rightQuads, 0, 0, isSelecting, isHovering, null);
                HighlightQuads(leftQuads, -1, -1, isSelecting, isHovering, null);
                HighlightQuads(floorQuads, 0, 1, isSelecting, isHovering, null);
            }
            if(parent == "Floor")
            {
                HighlightQuads(rightQuads, 1, 1, isSelecting, isHovering, null);
                HighlightQuads(leftQuads, 1, 0, isSelecting, isHovering, null);
                HighlightQuads(floorQuads, -1, -1, isSelecting, isHovering, null);
            }
        }
        
    }

    private void HighlightQuads(Transform parent, int partsIndex, int clickedNumIndex, bool isSelecting, bool isHovering, int? specificValue = null)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            string childName = child.name;
            string[] parts = childName.Split(',');
            bool shouldHighlight = false;

            if (specificValue.HasValue) // colとして選ばれた箇所
            {
                shouldHighlight = parts.Length == 2 && int.TryParse(parts[partsIndex], out int num) && num == specificValue.Value;
            }
            else if (partsIndex == -1 && clickedNumIndex == -1) //rayが落ちた面
            {
                shouldHighlight = parts.Length == 2 && 
                    (int.TryParse(parts[0], out int num1) && num1 == clickedNum[0] || 
                    int.TryParse(parts[1], out int num2) && num2 == clickedNum[1]);
            }
            else if (parts.Length == 2 && int.TryParse(parts[partsIndex], out int num) && num == clickedNum[clickedNumIndex]) //rayが落ちていない側面
            {
                shouldHighlight = true;
            }

            if (shouldHighlight)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material materialToApply;
                    
                    if (isSelecting || selectedRenderers.Contains(renderer))
                    {
                        materialToApply = selectedMaterial;
                        if (!selectedRenderers.Contains(renderer))
                        {
                            selectedRenderers.Add(renderer);
                        }
                    }
                    else if (isHovering)
                    {
                        if (selectedQuad != null && selectedQuad.isHovered)
                        {
                            materialToApply = subselectedMaterial;
                        }
                        else
                        {
                            materialToApply = highlightedMaterial;
                        }
                    }
                    else
                    {
                        materialToApply = originalMaterial;
                    }

                    renderer.material = materialToApply;
                    //Debug.Log($"Applied {materialToApply.name} to {child.name}");
                    if (!affectedRenderers.Contains(renderer))
                        affectedRenderers.Add(renderer);
                }
            }
        }
    }

    private void ResetAllHighlights()
    {
        foreach (Renderer renderer in selectedRenderers)
        {
            renderer.material = originalMaterial;
        }
        selectedRenderers.Clear();

        foreach (Renderer renderer in affectedRenderers)
        {
            renderer.material = originalMaterial;
        }
        affectedRenderers.Clear();
    }

    private void ResetHighlights()
    {
        foreach (Renderer renderer in affectedRenderers)
        {
            if (!selectedRenderers.Contains(renderer))
            {
                renderer.material = originalMaterial;
            }
        }
        affectedRenderers.RemoveAll(r => !selectedRenderers.Contains(r));
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnQuadSelected);
            simpleInteractable.selectExited.RemoveListener(OnQuadSelectExited);
            simpleInteractable.hoverEntered.RemoveListener(OnQuadHoverEntered);
            simpleInteractable.hoverExited.RemoveListener(OnQuadHoverExited);
        }

        // 選択されていたQuadが破棄される場合、選択状態をリセット
        if (selectedQuad == this)
        {
            selectedQuad = null;
            selectedParentName = null;
        }

        // スクリプトが破棄されるときにprefabも削除
        if (spawnedPrefab != null)
        {
            Destroy(spawnedPrefab);
            spawnedPrefab = null;
        }
    }
}
