using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class InfoDisplay : MonoBehaviour
{
    //private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable simpleInteractable;
    private GameObject nameText;
    private bool isNameVisible = false;

    void Start()
    {
        //grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        simpleInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        nameText = transform.Find("Name").gameObject;
        nameText.SetActive(false);

        //grabInteractable.activated.AddListener(ToggleName);
        simpleInteractable.selectEntered.AddListener(ToggleName);
    }

    void ToggleName(SelectEnterEventArgs args)
    {
        if (isNameVisible)
        {
            HideName();
        }
        else
        {
            ShowName();
        }
    }

    void ShowName()
    {
        nameText.GetComponent<TextMeshPro>().text = gameObject.name;
        nameText.SetActive(true);
        isNameVisible = true;
    }

    void HideName()
    {
        nameText.SetActive(false);
        isNameVisible = false;
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(ToggleName);
        }
    } 
}
