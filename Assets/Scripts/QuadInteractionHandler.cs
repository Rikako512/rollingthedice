using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class QuadInteractionHandler : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    public Material highlightedMaterial;
    public Material originalMaterial;
    private static int[] clickedNum = new int[2];

    private static List<Renderer> highlightedRenderers = new List<Renderer>();

    void Start()
    {
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(OnQuadSelected);
    }

    private void OnQuadSelected(SelectEnterEventArgs args)
    {
        // 直前のハイライトをリセット
        ResetHighlights();

        string quadName = gameObject.name;
        string parentName = transform.parent?.name ?? "No parent";

        Debug.Log($"Quad名: {quadName}");
        Debug.Log($"Quadの親の名前: {parentName}");

        ParseQuadName(quadName);
        HighlightRelatedQuads(parentName);
    }

    private void ParseQuadName(string name)
    {
        string[] parts = name.Split(',');
        if (parts.Length == 2 && int.TryParse(parts[0], out clickedNum[0]) && int.TryParse(parts[1], out clickedNum[1]))
        {
            Debug.Log($"Parsed numbers: {clickedNum[0]}, {clickedNum[1]}");
        }
        else
        {
            Debug.LogError($"Failed to parse quad name: {name}");
        }
    }

    private void HighlightRelatedQuads(string parentName)
    {
        Transform scatterplotMatrix = transform.parent.parent;
        Transform leftQuads = scatterplotMatrix.Find("Left");
        Transform rightQuads = scatterplotMatrix.Find("Right");
        Transform floorQuads = scatterplotMatrix.Find("Floor");

        if (parentName == "Right")
        {
            HighlightThisQuads(rightQuads);
            HighlightQuads(leftQuads, 0, 0);
            HighlightQuads(floorQuads, 1, 1);
        }
        else if (parentName == "Left")
        {
            HighlightThisQuads(leftQuads);
            HighlightQuads(rightQuads, 0, 0);
            HighlightQuads(floorQuads, 0, 1);
        }
        else if (parentName == "Floor")
        {
            HighlightThisQuads(floorQuads);
            HighlightQuads(leftQuads, 1, 0);
            HighlightQuads(rightQuads, 1, 1);
        }
    }

    // 選択した面をハイライト
    private void HighlightThisQuads(Transform parent)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            string childName = child.name;
            string[] parts = childName.Split(',');
            if (parts.Length == 2 && 
                (int.TryParse(parts[0], out int num1) && num1 == clickedNum[0] || 
                int.TryParse(parts[1], out int num2) && num2 == clickedNum[1]))
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // ハイライトされたマテリアルに変更
                    renderer.material = highlightedMaterial;
                    highlightedRenderers.Add(renderer); // ハイライトされたレンダラーをリストに追加
                }
            }

        }
    }


    // 他の面をハイライト
    private void HighlightQuads(Transform parent, int partsIndex, int clickedNumIndex)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            string childName = child.name;
            string[] parts = childName.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[partsIndex], out int num) && num == clickedNum[clickedNumIndex])
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 元のマテリアルを保持している場合は、ハイライトされたマテリアルに変更
                    renderer.material = highlightedMaterial;
                    if (!highlightedRenderers.Contains(renderer)) // 重複を防ぐ
                    {
                        highlightedRenderers.Add(renderer); // ハイライトされたレンダラーをリストに追加
                    }
                }
            }
        }
    }

    private void ResetHighlights()
    {
        foreach (Renderer renderer in highlightedRenderers)
        {
            // 元のマテリアルに戻す（元のマテリアルは各Quadごとに異なる）
            /*
            Material originalMaterialForChild = GetOriginalMaterialForChild(renderer.gameObject);
            if (originalMaterialForChild != null)
            {
                renderer.material = originalMaterialForChild; 
            }
            */
            renderer.material = originalMaterial; 
        }
        highlightedRenderers.Clear(); // リストをクリア
    }

    /*
    private Material GetOriginalMaterialForChild(GameObject child)
    {
        // Quad名から元のマテリアル名を生成
        string quadName = child.name; // Quad名は "5, 3" の形式
        string materialName = quadName.Replace(", ", "_").Trim(); // "5_3" に変換

        // マテリアルをResourcesフォルダから取得（Assets/Resources/2dsp_mat内）
        Material material = Resources.Load<Material>($"2dsp_mat/{materialName}");
        
        if (material == null)
        {
            Debug.LogError($"Material not found: 2dsp_mat/{materialName}");
            return null; // マテリアルが見つからない場合はnullを返す
        }

        return material; // マテリアルが見つかった場合は返す
    }
    */



    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnQuadSelected);
        }
    }
}
