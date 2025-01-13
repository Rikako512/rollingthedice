using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class UIPositioner_basic : MonoBehaviour
{
    [SerializeField] private Transform vrCamera;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    
    private Vector3 fixedLocalPosition;
    private float fixedYRotation;
    private bool isGrabbed = false;

    private void Start()
    {
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        // 初期位置と回転を設定
        fixedLocalPosition = transform.localPosition;
        fixedYRotation = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (!isGrabbed)
        {
            // カメラに対して固定位置に配置
            transform.position = vrCamera.TransformPoint(fixedLocalPosition);

            // カメラの方向を向くが、Y軸回転のみ適用
            Vector3 directionToCamera = vrCamera.position - transform.position;
            directionToCamera.y = 0; // Y軸の差を無視
            if (directionToCamera != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera);
                transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y + fixedYRotation, 0);
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // 新しい固定位置とY軸回転を保存
        fixedLocalPosition = vrCamera.InverseTransformPoint(transform.position);
        fixedYRotation = transform.localEulerAngles.y - vrCamera.localEulerAngles.y;
    }
}
