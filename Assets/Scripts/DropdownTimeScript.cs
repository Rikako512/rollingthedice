using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownTimeScript : MonoBehaviour
{
    private Dropdown dropdown;
    private float startTime;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        startTime = Time.time;

        // ドロップダウンのオプションをクリア
        dropdown.ClearOptions();

        // 0から144までのオプションを追加
        List<string> options = new List<string>();
        for (int i = 0; i <= 144; i++)
        {
            options.Add(i.ToString());
        }
        dropdown.AddOptions(options);

        // ドロップダウンの値が変更されたときのイベントを設定
        dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });
    }

    void OnDropdownValueChanged()
    {
        // 現在の経過時間を計算
        float elapsedTime = Time.time - startTime;

        // 経過時間をログに出力
        Debug.Log("回答終了の時刻: " + elapsedTime.ToString("F2") + "秒");
    }
}
