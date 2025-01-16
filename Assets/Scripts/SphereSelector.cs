using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SphereSelector : MonoBehaviour
{
    private Button uiButton;
    public Material meshMaterial;

    private bool isSelecting = false;
    private List<Transform> selectedSpheres = new List<Transform>();
    private Mesh generatedMesh;
    private GameObject meshObject;

    private ColorBlock originalColors;
    public Color waitingStateColor = new Color32(200, 0, 0, 255); // 待機状態の色（インスペクタで変更可能）

    private void Start()
    {
        uiButton = GetComponent<Button>();
        uiButton.onClick.AddListener(ToggleSelectionMode);
        originalColors = uiButton.colors;

        generatedMesh = new Mesh();
        meshObject = new GameObject("GeneratedMesh");
        meshObject.AddComponent<MeshFilter>().mesh = generatedMesh;
        meshObject.AddComponent<MeshRenderer>().material = meshMaterial;
    }

    private void ToggleSelectionMode()
    {
        isSelecting = !isSelecting;

        if (isSelecting)
        {
            // 選択モード開始
            Debug.Log("選択モード開始");
            selectedSpheres.Clear();
            if (meshObject != null)
            {
                meshObject.SetActive(true);
            }
            ChangeButtonColor(waitingStateColor);
        }
        else
        {
            // 選択モード終了
            Debug.Log("選択モード終了");
            if (selectedSpheres.Count >= 3)
            {
                // メッシュを最終更新
                UpdateMesh();
            }
            else
            {
                // 選択されたSphereが3つ未満の場合、メッシュを非表示
                if (meshObject != null)
                {
                    meshObject.SetActive(false);
                }
            }
            ResetButtonColor();
        }
    }

    private void ChangeButtonColor(Color newColor)
    {
        ColorBlock colorBlock = uiButton.colors;
        colorBlock.normalColor = newColor;
        uiButton.colors = colorBlock;
    }

    private void ResetButtonColor()
    {
        uiButton.colors = originalColors;
    }

    private void Update()
    {
        if (isSelecting)
        {
            // 選択されたSphereを検出
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable[] interactables = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            foreach (UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable in interactables)
            {
                if (interactable.isSelected && !selectedSpheres.Contains(interactable.transform))
                {
                    selectedSpheres.Add(interactable.transform);
                }
            }

            // 3つ以上のSphereが選択された場合、メッシュを生成/更新
            if (selectedSpheres.Count >= 3)
            {
                UpdateMesh();
            }
        }
    }

    private void UpdateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();

        // 選択されたSphereの位置を頂点として使用
        foreach (Transform sphere in selectedSpheres)
        {
            vertices.Add(sphere.position);
        }

        // 凸包アルゴリズムでメッシュを生成
        int[] triangles = GenerateConvexHull(vertices);

        generatedMesh.Clear();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateNormals();
    }

    private int[] GenerateConvexHull(List<Vector3> points)
    {
        // QuickHullアルゴリズムなどを使って凸包を計算する部分。
        // Unityでは外部ライブラリや独自実装が必要になる。
        
        // 仮の実装: 三角形分割（簡易版）
        List<int> triangles = new List<int>();
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        return triangles.ToArray();
    }

    private void OnDisable()
    {
        // コンポーネントが無効になる際にリスナーを削除
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(ToggleSelectionMode);
        }
    }
}
