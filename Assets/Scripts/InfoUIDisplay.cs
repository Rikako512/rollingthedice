using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class InfoUIDisplay : MonoBehaviour
{
    public GameObject uiContainer; // ScrollViewContainerをアサインする
    public TextMeshProUGUI headerText; // HeaderのTextMeshProUGUIコンポーネントをアサインする
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    private bool isUIVisible = false;

    void Start()
    {
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(ToggleUI);
        uiContainer.SetActive(false); // 初期状態では非表示
    }

    void ToggleUI(SelectEnterEventArgs args)
    {
        isUIVisible = !isUIVisible;
        uiContainer.SetActive(isUIVisible);

        if (isUIVisible)
        {
            // UIが表示されたときにHeaderのテキストを更新
            UpdateHeaderText();

            // ScrollViewTogglerの状態をリセット
            ScrollViewToggler toggler = uiContainer.GetComponentInChildren<ScrollViewToggler>();
            if (toggler != null)
            {
                toggler.InitialState(); // 初期状態（スクロールビュー非表示）に設定
            }
        }
    }

    void UpdateHeaderText()
    {
        if (headerText != null)
        {
            headerText.text = gameObject.name; // sphereの名前をHeaderのテキストに設定
        }
        else
        {
            Debug.LogWarning("Header Text component is not assigned in the Inspector.");
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
