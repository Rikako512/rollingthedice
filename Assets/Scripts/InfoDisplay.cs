using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class InfoDisplay : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private GameObject nameText;
    private bool isNameVisible = false;
    private float lastActivationTime = 0f;
    private const float doubleClickTime = 0.5f; // ダブルクリックと判定する時間間隔

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        nameText = transform.Find("Name").gameObject;
        nameText.SetActive(false);

        grabInteractable.activated.AddListener(ToggleName);
    }

    void ToggleName(ActivateEventArgs args)
    {
        float currentTime = Time.time;
        if (currentTime - lastActivationTime < doubleClickTime)
        {
            // ダブルクリック検出
            if (!isNameVisible)
            {
                ShowName(true);
            }
            else
            {
                HideName();
            }
        }
        else
        {
            // シングルクリック
            if (!isNameVisible)
            {
                ShowName(false);
            }
            else
            {
                HideName();
            }
        }
        lastActivationTime = currentTime;
    }

    void ShowName(bool keepVisible)
    {
        nameText.GetComponent<TextMeshPro>().text = gameObject.name;
        nameText.SetActive(true);
        isNameVisible = true;

        if (!keepVisible)
        {
            Invoke("HideName", 2f);
        }
    }

    void HideName()
    {
        if (isNameVisible)
        {
            nameText.SetActive(false);
            isNameVisible = false;
        }
    }

    void OnDestroy()
    {
        grabInteractable.activated.RemoveListener(ToggleName);
    }
}
