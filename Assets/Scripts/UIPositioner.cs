using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPositioner : MonoBehaviour
{
    private Transform vrCamera;
    public Transform sphere;
    private Vector3 uiOffset = new Vector3(-0.5f, 0, 0);

    void Start()	
    {	
        // XR Origin > Camera Offset > Main Camera を見つける	
        vrCamera = FindVRCamera();	
        if (vrCamera == null)	
        {	
        Debug.LogError("VR Camera not found. Make sure you have the correct XR setup.");	
        }	
    }

    void Update()
    {
        if (vrCamera == null) return;

        // sphereからカメラへの方向ベクトルを計算
        Vector3 directionToCamera = vrCamera.position - sphere.position;
        directionToCamera.y = 0; // Y軸回転のみを考慮

        // sphereを中心とした回転を計算
        Quaternion rotationToCamera = Quaternion.LookRotation(directionToCamera);

        // UIの位置をsphereに対して相対的に維持しつつ回転
        transform.position = sphere.position + rotationToCamera * uiOffset;

        // UIの向きをカメラに向ける
        //transform.rotation = rotationToCamera;
        transform.rotation = rotationToCamera * Quaternion.Euler(0, 180, 0);
    }
    
    private Transform FindVRCamera()
    {
        // XR Originを探す
        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            // Camera Offsetを探す
            var cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset != null)
            {
                // Main Cameraを探す
                var mainCamera = cameraOffset.Find("Main Camera");
                if (mainCamera != null)
                {
                    return mainCamera;
                }
            }
        }
        return null;
    }
}
