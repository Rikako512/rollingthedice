using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;


public class ClickedSP : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    /*
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
    */

    private int col_X = -1;
    private int col_Y = -1;
    private int col_Z = -1;

    private GameObject plotter; //大きい方のSP
    private PointRenderer pointRenderer;
    private GameObject pointcontainer;
    private TextMeshProUGUI xAxisText;
    private TextMeshProUGUI yAxisText;
    private TextMeshProUGUI zAxisText;

    void Start()
    {
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(OnQuadSelected);
        simpleInteractable.selectExited.AddListener(OnQuadSelectExited);
        simpleInteractable.hoverEntered.AddListener(OnQuadHoverEntered);
        simpleInteractable.hoverExited.AddListener(OnQuadHoverExited);

        plotter = GameObject.FindGameObjectWithTag("main_plotter");
        pointcontainer = GameObject.FindGameObjectWithTag("main_pointcontainer");
        
        GameObject xAxisObj = GameObject.FindGameObjectWithTag("main_X_axis");
        GameObject yAxisObj = GameObject.FindGameObjectWithTag("main_Y_axis");
        GameObject zAxisObj = GameObject.FindGameObjectWithTag("main_Z_axis");

        xAxisText = xAxisObj?.GetComponent<TextMeshProUGUI>();
        yAxisText = yAxisObj?.GetComponent<TextMeshProUGUI>();
        zAxisText = zAxisObj?.GetComponent<TextMeshProUGUI>();

        if (xAxisText == null || yAxisText == null || zAxisText == null)
        {
            Debug.LogError("One or more axis text components not found");
        }

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
        //ResetAllHighlights();

        //selectedQuad = this;
        //selectedParentName = transform.parent?.name ?? "No parent";
        //Debug.Log($"Quad: {selectedParentName} {gameObject.name} selected");
   
        // ３つのcolの数値を取得
        AssignColumnValues();
        
        // 新しい選択に基づいてハイライトを適用
        /////////////HighlightRelatedQuads(selectedParentName, true);
        //UpdateQuadState(gameObject.name, selectedParentName, true);
        UpdateAxisText(); //UIの表記を更新
        UpdatePointRenderer(); // データ点群の分布を更新
        Debug.Log("---------- 操作：Selected by metricのSPをクリックしました ----------");
    }
        

    private void OnQuadSelectExited(SelectExitEventArgs args)
    {
        // 選択解除時には何もしない（次の選択まで維持）
        //Debug.Log($"Quad {gameObject.name} deselected");
    }

    private void OnQuadHoverEntered(HoverEnterEventArgs args)
    {
        //isHovered = true;
        //////////////Debug.Log($"Quad {gameObject.name} hover entered");
        //UpdateQuadState(gameObject.name, transform.parent?.name ?? "No parent", false);
    }

    private void OnQuadHoverExited(HoverExitEventArgs args)
    {
        //isHovered = false;
        ////////////Debug.Log($"Quad {gameObject.name} hover exited");
        //UpdateQuadState(gameObject.name, transform.parent?.name ?? "No parent", false);
    }

    private void UpdateQuadState(string spName, string parentName, bool isSelecting)
    {
        //HighlightRelatedQuads(parentName, isSelecting, isHovered);
    }

    private void AssignColumnValues()
    {
        string parentName = transform.parent.name;
        string[] numbers = parentName.Split(',');
        if (numbers.Length == 3)
        {
            col_X = int.Parse(numbers[0].Trim());
            col_Y = int.Parse(numbers[1].Trim());
            col_Z = int.Parse(numbers[2].Trim());
        }
    }

    private void UpdateAxisText()
    {
        List<string> columnList = new List<string>(CSVData.pointList[1].Keys);

        xAxisText.text = $"X axis: {columnList[col_X]}";
        yAxisText.text = $"Y axis: {columnList[col_Y]}";
        zAxisText.text = $"Z axis: {columnList[col_Z]}";

        Debug.Log($"Updated column values: X={col_X}, Y={col_Y}, Z={col_Z}");
    }

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
/*
    private void HighlightRelatedQuads(string parentName, bool isSelecting, bool isHovering)
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
    }

    private void HighlightQuads(Transform parent, int partsIndex, int clickedNumIndex, bool isSelecting, bool isHovering, int? specificValue = null)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            string childName = child.name;
            string[] parts = childName.Split(',');
            bool shouldHighlight = false;

            if (specificValue.HasValue)
            {
                shouldHighlight = parts.Length == 2 && int.TryParse(parts[partsIndex], out int num) && num == specificValue.Value;
            }
            else if (partsIndex == -1 && clickedNumIndex == -1)
            {
                shouldHighlight = parts.Length == 2 && 
                    (int.TryParse(parts[0], out int num1) && num1 == clickedNum[0] || 
                    int.TryParse(parts[1], out int num2) && num2 == clickedNum[1]);
            }
            else if (parts.Length == 2 && int.TryParse(parts[partsIndex], out int num) && num == clickedNum[clickedNumIndex])
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
    */

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
        /*
        if (selectedQuad == this)
        {
            selectedQuad = null;
            selectedParentName = null;
        }
        */
    }
}
