using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public bool Placed { get; private set; }
    public Vector3Int Size { get; private set; }
    public Vector3[] Vertices;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public Renderer childRenderer;
    public ObjectFollow objectFollow;
    public int materialIndex;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        childRenderer = GetComponentInChildren<Renderer>();
    }

    public void RevertToInitialization()
    {
        if (Placed)
        {
            if (objectFollow != null)
            {
                Destroy(objectFollow);
                objectFollow = null;
            }

            objectFollow = gameObject.AddComponent<ObjectFollow>();
            Placed = false;
            BuildManager.current.placedObjects.Remove(this);
            materialIndex = 0;   
        }
        else
        {
            if (objectFollow != null)
            {
                objectFollow.enabled = true;
            }
            else
            {
                objectFollow = gameObject.AddComponent<ObjectFollow>();
            }
        }
    }

    private void GetColliderVertexPositionsLocal()
    {
        BoxCollider b = gameObject.GetComponent<BoxCollider>();
        Vertices = new Vector3[4];
        Vertices[0] = b.center + new Vector3(-b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[1] = b.center + new Vector3(b.size.x, -b.size.y, -b.size.z) * 0.5f;
        Vertices[2] = b.center + new Vector3(b.size.x, -b.size.y, b.size.z) * 0.5f;
        Vertices[3] = b.center + new Vector3(-b.size.x, -b.size.y, b.size.z) * 0.5f;
    }

    private void CalculateSizeInCells()
    {
        Vector3Int[] vertices = new Vector3Int[Vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(Vertices[i]);
            vertices[i] = BuildManager.current.gridLayout.WorldToCell(worldPos);
        }

        Size = new Vector3Int(Math.Abs((vertices[0] - vertices[1]).x), Math.Abs((vertices[0] - vertices[3]).y), 1);
    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(Vertices[0]);
    }

    private void Start()
    {
        GetColliderVertexPositionsLocal();
        CalculateSizeInCells();
    }

    public void RotateRight()
    {
        transform.Rotate(new Vector3(0, 90, 0));
        Size = new Vector3Int(Size.y, Size.x, 1);
        UpdateVertices();
    }

    public void RotateLeft()
    {
        transform.Rotate(new Vector3(0, -90, 0));
        Size = new Vector3Int(Size.y, Size.x, 1);
        UpdateVertices();
    }

    private void UpdateVertices()
    {
        Vector3[] vertices = new Vector3[Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vertices[(i + 1) % Vertices.Length];
        }
        Vertices = vertices;
    }

     public void AssignChildRenderer()
    {
        //Find child object
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            if (renderer != childRenderer)
            {
                childRenderer = renderer;
                break;
            }
        }

        //Assign material for child object
        if (childRenderer != null)
        {
            int materialIndex = BuildManager.current.currentMaterialIndex % BuildManager.current.materials.Count;
            childRenderer.material = BuildManager.current.materials[materialIndex];
        }
    }

    
    public void Place()
    {
        // Disable ObjectFollow component
        if (objectFollow != null)
        {
            Destroy(objectFollow);
            objectFollow = null;
        }

        Placed = true;
    }
}