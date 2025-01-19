using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EyeHeightAdjuster : MonoBehaviour
{
    public Transform cameraOffset;
    public TextMeshProUGUI label;
    public float minHeight = 1.8f;
    public float maxHeight = 10.0f;
    
    private Slider slider;

    public float smoothTime = 0.3f; // スムージングの時間
    private Vector3 velocity = Vector3.zero;
    private float targetHeight;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(AdjustEyeHeight);
        
        // 初期値を設定
        slider.value = 0.5f;
        targetHeight = Mathf.Lerp(minHeight, maxHeight, slider.value);
    }

    private void Update()
    {
        // 現在のx,z座標を保持しつつ、yのみをスムーズに更新
        Vector3 currentPosition = cameraOffset.position;
        float newY = Mathf.SmoothDamp(currentPosition.y, targetHeight, ref velocity.y, smoothTime);
        cameraOffset.position = new Vector3(currentPosition.x, newY, currentPosition.z);
    }

    private void AdjustEyeHeight(float sliderValue)
    {
        targetHeight = Mathf.Lerp(minHeight, maxHeight, sliderValue);
        label.text = targetHeight.ToString("F1");
        //Debug.Log("---------- 操作：目線の高さを変えました ----------");
    }
}


