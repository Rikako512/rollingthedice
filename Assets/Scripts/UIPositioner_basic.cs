using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPositioner_basic : MonoBehaviour
{
    public Transform vrCamera;

    void Update()
    {
        transform.LookAt(new Vector3(vrCamera.position.x, transform.position.y, vrCamera.position.z));
        transform.Rotate(0, 180, 0);
    }
}