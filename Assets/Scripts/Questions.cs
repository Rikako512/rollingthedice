using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Questions : MonoBehaviour
{
    private Dropdown dropdown;
    public Text qText;
    public Image image;
    private float startTime;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        startTime = Time.time;

        // ドロップダウンのオプションをクリア
        dropdown.ClearOptions();

        // 0から20までのオプションを追加
        List<string> options = new List<string>();
        for (int i = 0; i <= 20; i++)
        {
            options.Add(i.ToString());
        }
        dropdown.AddOptions(options);

        // ドロップダウンの値が変更されたときのイベントを設定
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int num)
    {
        Sprite newSprite = Resources.Load<Sprite>($"Questions/Q{num}");
        if (newSprite != null)
        {
            image.sprite = newSprite;
        }
        else
        {
            Debug.LogError("Sprite not found in Resources folder");
        }
        // 現在の経過時間を計算
        float elapsedTime = Time.time - startTime;

        // 経過時間をログに出力
        Debug.Log("問題開始の時刻: " + elapsedTime.ToString("F2") + "秒");

    }
}

