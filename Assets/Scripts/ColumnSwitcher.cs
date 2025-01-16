using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Linq;

public class ColumnSwitcher : MonoBehaviour
{
    private XRSimpleInteractable simpleInteractable;
    public TextMeshPro textMeshPro;
    public Transform SPM; // SPMオブジェクトへの参照
    public Transform SPM_select;
    public Transform spawnSPs;

    private static List<ColumnSwitcher> selectedObjects = new List<ColumnSwitcher>();
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float moveSpeed = 3f;

    private List<Transform> linkedQuads = new List<Transform>();
    private List<Transform> linkedSpawned = new List<Transform>();

    private void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(OnSelectEntered);
        targetPosition = transform.position;
        textMeshPro = GetComponentInChildren<TextMeshPro>();

        FindLinkedQuads();
    }

    private void FindLinkedQuads()
    {
        string parentName = transform.parent.name;
        string columnName = transform.name;

        if (SPM != null && SPM_select != null)
        {
            if (parentName == "X")
            {
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Right", columnName, 1));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Floor", columnName, 1));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Right", columnName, 1));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Floor", columnName, 1));
            }
            else if (parentName == "Y")
            {
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Left", columnName, 1));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Floor", columnName, 0));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Left", columnName, 1));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Floor", columnName, 0));

            }
            else if (parentName == "Z")
            {
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Right", columnName, 0));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM, "Left", columnName, 0));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Right", columnName, 0));
                linkedQuads.AddRange(FindAllQuadsInChild(SPM_select, "Left", columnName, 0));
            }
        }
    }	


    public void UpdateLinkedSpawned()	
    {

        linkedSpawned.Clear(); // 既存のリンクをクリア
        if (spawnSPs != null)
        {
            var children = spawnSPs.GetComponentsInChildren<Transform>(true)
                .Where(t => t != spawnSPs && IsValidCName(t.name))
                .ToList();

            linkedSpawned.AddRange(children);

            foreach (var child in children)
            {
                //Debug.Log($"Found Child: {child.name} in SpawnSPs");
            }
        }
        else
        {
            Debug.LogError("SpawnSPs is null!");
        }
		
    }

    private bool IsValidCName(string name)
    {
        string[] parts = name.Split(',');
        if (parts.Length != 3) return false;
        
        return parts.All(part => 
            part.Trim().Length == 1 && char.IsDigit(part.Trim()[0]));
    }

    private List<Transform> FindAllQuadsInChild(Transform parent, string childName, string columnName, int indexToCheck)
    {
        List<Transform> quads = new List<Transform>();
        Transform child = parent.Find(childName);
        if (child != null)
        {
            quads.AddRange(child.GetComponentsInChildren<Transform>()
                .Where(t => {
                    string[] parts = t.name.Split(',');
                    return parts.Length == 2 && parts[indexToCheck].Trim() == columnName;
                }));
        }
        return quads;
    }

    private void Update()
	{
		    if (isMoving)
		    {
		        Vector3 previousPosition = transform.position;
		        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
		        Vector3 movement = transform.position - previousPosition;
		
		        foreach (var linkedObject in linkedQuads)
		        {
		            if (linkedObject != null)
		            {
		                linkedObject.position += movement;
		            }
		        }

                foreach (var spawnedObject in linkedSpawned)
                {
                    if (spawnedObject != null)
                    {
                        spawnedObject.position += movement;
                    }
                }
		
		        if (transform.position == targetPosition)
		        {
		            isMoving = false;
		            //Debug.Log("Movement complete.");
		        }
		    }
	}
		

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!selectedObjects.Contains(this))
        {
            selectedObjects.Add(this);
            
            if (textMeshPro != null)
            {
                textMeshPro.color = Color.yellow;
            }

            if (selectedObjects.Count == 2)
            {
                StartCoroutine(SwapPositions(selectedObjects[0], selectedObjects[1]));
                UpdateLinkedSpawned();
            }
        }
    }

    private IEnumerator SwapPositions(ColumnSwitcher obj1, ColumnSwitcher obj2)
    {
        Vector3 tempPosition = obj1.transform.position;
        
        obj1.targetPosition = obj2.transform.position;
        obj2.targetPosition = tempPosition;
        
        obj1.isMoving = true;
        obj2.isMoving = true;

        yield return new WaitForSeconds(1f);

        if (obj1.textMeshPro != null)
        {
            obj1.textMeshPro.color = Color.black;
        }
        if (obj2.textMeshPro != null)
        {
            obj2.textMeshPro.color = Color.black;
        }

        selectedObjects.Clear();
    }
}
