using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private Vector3 initialLocalPosition;
    private List<GameObject> synchronizedQuads = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        initialLocalPosition = transform.localPosition;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        // グラブ時の処理
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (isSelected)
        {
            RestrictMovement();
            SynchronizeQuads();
        }
    }

    private void RestrictMovement()
    {
        Vector3 newLocalPosition = transform.localPosition;
        newLocalPosition.y = Mathf.Round(newLocalPosition.y); // Y軸を整数値に制限
        newLocalPosition.x = initialLocalPosition.x; // X軸を初期位置に固定
        newLocalPosition.z = initialLocalPosition.z; // Z軸を初期位置に固定
        transform.localPosition = newLocalPosition;
    }

    private void SynchronizeQuads()
    {
        // 同期するQuadをクリア
        synchronizedQuads.Clear();

        // 親オブジェクトを検索
        GameObject parentA = GameObject.Find("A");
        if (parentA == null)
        {
            Debug.LogError("親オブジェクトが見つかりません");
            return;
        }

        // 同期するQuadを検索
        foreach (Transform child in parentA.transform)
        {
            if (child.GetComponent<MeshFilter>()?.sharedMesh.name == "Quad" &&
                Mathf.RoundToInt(child.localPosition.x) == Mathf.RoundToInt(initialLocalPosition.x))
            {
                synchronizedQuads.Add(child.gameObject);
            }
        }

        // Quadの位置を同期
        foreach (GameObject quad in synchronizedQuads)
        {
            Vector3 quadLocalPosition = quad.transform.localPosition;
            quadLocalPosition.y = transform.localPosition.y;
            quad.transform.localPosition = quadLocalPosition;
        }
    }

    // 追加：移動を許可するためのオーバーライド
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        RestrictMovement(); // 移動後に位置を制限
    }
}
