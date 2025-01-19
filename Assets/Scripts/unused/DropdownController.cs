using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DropdownController : MonoBehaviour
{
    private Dropdown dropdown;
    public Text outputText;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();

        if (dropdown != null)
        {
            // ドロップダウンの値が選択されたときのイベントリスナーを追加
            dropdown.onValueChanged.AddListener(OnDropdownValueSelected);
        }
        else
        {
            Debug.LogError("Dropdownコンポーネントが見つかりません");
        }
    }

    void OnDropdownValueSelected(int value)
    {
        if (outputText != null)
        {
            outputText.text = value.ToString();
        }

        Debug.Log(gameObject.name + "でユーザーが選択した数字: " +　value);
    }
}
