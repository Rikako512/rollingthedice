using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DataPointInfoDisplay : MonoBehaviour
{
    public GameObject infoPanelPrefab;
    public Transform uiCanvas;
    public XRRayInteractor rayInteractor;
    public float panelOffset = 0.1f;

    private GameObject currentInfoPanel;
    private bool isPanelFixed = false;
    private GameObject hoveredDataPoint;

    // �{�^���A�N�V������ݒ肷�邽�߂̕ϐ�
    public InputActionProperty selectAction; // Input Action Property��Inspector����ݒ�

    void Update()
    {
        // RaycastHit�̎擾
        if (rayInteractor.TryGetCurrentRaycast(out RaycastHit? hit, out int hitIndex, out RaycastResult? uiRaycastHit, out int uiRaycastHitIndex, out bool isValidTarget))
        {
            GameObject hitObject = hit.Value.collider.gameObject;

            if (hitObject.transform.parent != null && hitObject.transform.parent.name == "PointHolder")
            {
                if (hoveredDataPoint != hitObject)
                {
                    hoveredDataPoint = hitObject;
                    ShowInfoPanel(hoveredDataPoint);
                }

                // VR�R���g���[���[�̃{�^�����͂����o���āA�p�l�����Œ�
                if (selectAction.action.triggered) // �{�^���������ꂽ�u��
                {
                    isPanelFixed = !isPanelFixed; // �p�l���̌Œ��Ԃ�؂�ւ�
                }
            }
            else
            {
                HideInfoPanel();
            }
        }
        else
        {
            HideInfoPanel();
        }

        if (currentInfoPanel != null)
        {
            UpdatePanelPosition();
        }
    }

    void ShowInfoPanel(GameObject dataPoint)
    {
        if (currentInfoPanel == null)
        {
            currentInfoPanel = Instantiate(infoPanelPrefab, uiCanvas);
        }

        UpdatePanelContent(dataPoint);
    }

    void HideInfoPanel()
    {
        if (!isPanelFixed && currentInfoPanel != null)
        {
            Destroy(currentInfoPanel);
            currentInfoPanel = null;
            hoveredDataPoint = null;
        }
    }

    void UpdatePanelPosition()
    {
        if (hoveredDataPoint != null)
        {
            Vector3 panelPosition = hoveredDataPoint.transform.position + Vector3.right * panelOffset;
            currentInfoPanel.transform.position = panelPosition;
            currentInfoPanel.transform.LookAt(Camera.main.transform);
            currentInfoPanel.transform.Rotate(0, 180, 0); // �p�l�����J�����������悤�ɉ�]
        }
    }

    void UpdatePanelContent(GameObject dataPoint)
    {
        if (currentInfoPanel != null)
        {
            TextMeshProUGUI nameText = currentInfoPanel.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = dataPoint.name;

            Transform detailsContent = currentInfoPanel.transform.Find("DetailsPanel/Viewport/Content");
            Button expandButton = currentInfoPanel.transform.Find("ExpandButton").GetComponent<Button>();

            expandButton.onClick.RemoveAllListeners();
            expandButton.onClick.AddListener(() => ToggleDetailsPanel(detailsContent.gameObject));

            // �f�[�^�|�C���g�̏ڍ׏����X�V
            UpdateDetailsContent(dataPoint, detailsContent);
        }
    }

    void UpdateDetailsContent(GameObject dataPoint, Transform detailsContent)
    {
        // CSVData����Y������f�[�^�|�C���g�̏����擾
        Dictionary<string, object> pointData = CSVData.pointList[int.Parse(dataPoint.name)];

        // �����̏ڍ׏����폜
        foreach (Transform child in detailsContent)
        {
            Destroy(child.gameObject);
        }

        // �V�����ڍ׏���ǉ�
        foreach (var kvp in pointData)
        {
            GameObject detailItem = new GameObject("DetailItem");
            detailItem.transform.SetParent(detailsContent, false);

            TextMeshProUGUI detailText = detailItem.AddComponent<TextMeshProUGUI>();
            detailText.text = $"{kvp.Key}: {kvp.Value}";
            detailText.fontSize = 12;
            detailText.alignment = TextAlignmentOptions.Left;
        }
    }

    void ToggleDetailsPanel(GameObject detailsPanel)
    {
        detailsPanel.SetActive(!detailsPanel.activeSelf);
    }
}
