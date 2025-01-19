using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorChanger : MonoBehaviour
{
    public GameObject databall;
    public Material coloredMaterial; // 赤またはグレーのマテリアル
    public Material highlightedMaterial;
    private Renderer databallRenderer;
    //private Material originalMaterial;
    private string targetTag;

    void Start()
    {

        if (databall != null)
        {
            databallRenderer = databall.GetComponent<Renderer>();
            if (databallRenderer != null)
            {
                //originalMaterial = databallRenderer.material;
            }
            else
            {
                Debug.LogError("Renderer component not found on the Databall.");
            }

        }
        else
        {
            Debug.LogError("Databall GameObject is not assigned in the Inspector.");
        }

        GetComponent<Button>().onClick.AddListener(ChangeColor);
    }

    void ChangeColor()
    {
        if (databallRenderer != null && coloredMaterial != null)
        {
            string colorname = coloredMaterial.name;
            if (databallRenderer.material.name.StartsWith(colorname) == true)
            {
                databallRenderer.material = highlightedMaterial;
            }
            else
            {
                databallRenderer.material = coloredMaterial;
            }

            // タグの設定    
            if (databallRenderer.material.name.StartsWith("RedPoint") == true)
            {
                targetTag = "Red_data";
            }
            else if (databallRenderer.material.name.StartsWith("GrayPoint") == true)
            {
                targetTag = "Gray_data";
            }
        }
        Debug.Log("---------- 操作：データ点の色を変更しました ----------");
        //HandleTextMeshProUI(databall.name);
    }

    void HandleTextMeshProUI(string databallName)
    {
        GameObject parentObject = GameObject.FindGameObjectWithTag(targetTag);

        if (parentObject != null)
        {
            Transform existingTextObject = parentObject.transform.Find(databallName);

            if (existingTextObject != null)
            {
                Destroy(existingTextObject.gameObject);
            }

            GameObject textObject = new GameObject(databallName);
            textObject.transform.SetParent(parentObject.transform, false);

            TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = databallName;
            tmpText.fontSize = 14;
            tmpText.alignment = TextAlignmentOptions.Center;

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -15);
            rectTransform.sizeDelta = new Vector2(340, 40);
        }
        else
        {
            Debug.LogError("Parent object with tag " + targetTag + " not found.");
        }
    }

    void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(ChangeColor);
    }
}
