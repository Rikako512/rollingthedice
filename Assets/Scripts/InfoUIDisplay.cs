using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class InfoUIDisplay : MonoBehaviour
{
    public GameObject uiContainer; // ScrollViewContainerをアサインする
    public TextMeshProUGUI headerText; // HeaderのTextMeshProUGUIコンポーネントをアサインする
    public GameObject databall;
    public Material selectedMaterial;
    private Material originalMaterial;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    private bool isUIVisible = false;
    private Renderer ballRenderer;

    void Start()
    {
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(ToggleUI);
        uiContainer.SetActive(false); // 初期状態では非表示

        if (databall != null)
        {
            ballRenderer = databall.GetComponent<Renderer>();
            if (ballRenderer != null)
            {
                originalMaterial = ballRenderer.material;
            }
            else
            {
                Debug.LogError("Renderer component not found on the target sphere.");
            }
        }
        else
        {
            Debug.LogError("Target Sphere is not assigned in the inspector.");
        }
    }

    void ToggleUI(SelectEnterEventArgs args)
    {
        isUIVisible = !isUIVisible;
        uiContainer.SetActive(isUIVisible);

        if (isUIVisible)
        {
            // UIが表示されたときにHeaderのテキストを更新
            UpdateHeaderText();
            ChangeSphereAppearance(true);

            // ScrollViewTogglerの状態をリセット
            ScrollViewToggler toggler = uiContainer.GetComponentInChildren<ScrollViewToggler>();
            if (toggler != null)
            {
                toggler.InitialState(); // 初期状態（スクロールビュー非表示）に設定
            }
        }
        else
        {
            ChangeSphereAppearance(false);
        }
    }

    void UpdateHeaderText()
    {
        if (databall != null && headerText != null)
        {
            string dataName = databall.name;
            if (int.TryParse(dataName, out int index) && index >= 0 && index < CSVData.occupationList.Count)
            {
                headerText.text = CSVData.occupationList[index];
            }
            else
            {
                Debug.LogWarning("Invalid data name or index: " + dataName);
            }
        }
        else
        {
            Debug.LogError("Target Databall or Header Text is not assigned in the inspector.");
        }
    }

    void ChangeSphereAppearance(bool selected)
    {
        if (ballRenderer != null && selectedMaterial != null)
        {
            ballRenderer.material = selected ? selectedMaterial : originalMaterial;
        }
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(ToggleUI);
        }
    }
}
