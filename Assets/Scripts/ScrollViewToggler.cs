using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollViewToggler : MonoBehaviour
{
    public GameObject scrollView;
    public RectTransform scrollViewContainer;
    public TextMeshProUGUI scrollText;
    public GameObject databall;
    private Button toggleButton;
    private bool isVisible = false;
    private float expandedHeight = 450f;
    private float collapsedHeight = 136f;

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(ToggleScrollView);
        InitialState(); // 初期状態を設定
        UpdateScrollText();
    }

    void ToggleScrollView()
    {
        isVisible = !isVisible;
        scrollView.SetActive(isVisible);
        
        // ScrollViewContainerの高さを変更
        float newHeight = isVisible ? expandedHeight : collapsedHeight;
        scrollViewContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        toggleButton.GetComponentInChildren<TextMeshProUGUI>().text = isVisible ? "Close" : "Show";
    }

    public void InitialState()
    {
        isVisible = false;
        scrollView.SetActive(false);
        scrollViewContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, collapsedHeight);
        toggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Show";
    }

    void UpdateScrollText()
    {
        if (databall != null)
        {
            string dataName = databall.name; // DataBallの名前を取得
            if (int.TryParse(dataName, out int index))
            {
                string data = CSVData.GetDataForIndex(index); // CSVDataからデータを取得
                scrollText.text = data; // Scroll Textを更新
            }
            else
            {
                Debug.LogWarning("Invalid data name: " + dataName);
            }
        }
        else
        {
            Debug.LogError("Target DataBall is not assigned in the inspector.");
        }
    }

    void OnDestroy()
    {
        toggleButton.onClick.RemoveListener(ToggleScrollView);
    }
}
