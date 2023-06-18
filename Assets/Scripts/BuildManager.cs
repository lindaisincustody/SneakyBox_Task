using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class BuildManager : MonoBehaviour
{
    public static BuildManager current;

    public GridLayout gridLayout;
    private Grid grid;

    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase TransparentTile;

    public List<GameObject> prefabList;
    public Canvas canvas;

    private PlaceableObject objectToPlace;
    private List<PlaceableObject> objectsToMove = new List<PlaceableObject>();
    private PlaceableObject selectedObject;
    private string saveFileName = "saved_game.json";
    private string saveFilePath;

    public bool CanInitialize = true;

    public List<PlaceableObject> placedObjects = new List<PlaceableObject>();

    public List<Material> materials;
    public int currentMaterialIndex = 0;

    #region Unity methods

    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();

        // Initialize save file path
        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
    }

    private void Update()
    {
        Debug.Log("Exists: " + File.Exists(saveFilePath));
        HandleObjectInitializationInput();
        HandleObjectSelection();
        HandleObjectRotation();
        HandleObjectPlacement();
        HandleObjectRemove();

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGame();
        }
    }

    #endregion

    #region Utils

    // Handle object initialization with 1,2,3 keys
    private void HandleObjectInitializationInput()
    {
        if (!CanInitialize)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InitializeWithObject(prefabList[0]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            InitializeWithObject(prefabList[1]);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            InitializeWithObject(prefabList[2]);
        }
    }

    // Object selection with left click
    private void HandleObjectSelection()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlaceableObject clickedObject = hit.collider.GetComponent<PlaceableObject>();
            if (clickedObject != null)
            {
                canvas.gameObject.SetActive(true);
                selectedObject = clickedObject;
                objectToPlace = selectedObject;
                selectedObject.RevertToInitialization();
                CanInitialize = true;
            }
        }
    }

    // Object rotation with scroll
    public void HandleObjectRotation()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput > 0.0f) //scrolling up
        {
            if (objectToPlace)
                objectToPlace.RotateRight();

        }
        else if (scrollInput < 0.0f) //scrolling down
        {
            if (objectToPlace)
                objectToPlace.RotateLeft();

        }
    }

    // Object placement with right click
    public void HandleObjectPlacement()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (objectToPlace != null)
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
                CanInitialize = true;
                placedObjects.Add(objectToPlace);
                objectToPlace = null;
                canvas.gameObject.SetActive(false);
            }
        }
    }

    // Object Removal with escape
    public void HandleObjectRemove()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (objectsToMove.Count > 0)
            {
                foreach (var obj in objectsToMove)
                {
                    obj.RevertToInitialization();
                }
                objectsToMove.Clear();
                CanInitialize = true;
            }
            else if (objectToPlace != null)
            {
                Destroy(objectToPlace.gameObject);
                objectToPlace = null;
                CanInitialize = true;
            }
            else if (selectedObject != null)
            {
                placedObjects.Remove(selectedObject);
                Destroy(selectedObject.gameObject);
                selectedObject = null;
                CanInitialize = true;
            }
            else if (placedObjects.Count > 0)
            {
                PlaceableObject lastPlacedObject = placedObjects[placedObjects.Count - 1];
                placedObjects.Remove(lastPlacedObject);
                Destroy(lastPlacedObject.gameObject);
                CanInitialize = true;
            }
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    [SerializeField]
    public void OnChangeMaterialButtonClick()
    {
        // Trigger button to change object material
        if (selectedObject != null && selectedObject.childRenderer != null)
        {
            currentMaterialIndex++;
            if (currentMaterialIndex >= materials.Count)
            {
                currentMaterialIndex = 0;
            }
            selectedObject.childRenderer.material = materials[currentMaterialIndex];
        }
    }
    #endregion

    #region Building Placement

   public void InitializeWithObject(GameObject prefab)
    {
        Vector3 position = SnapCoordinateToGrid(GetMouseWorldPosition());
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectFollow>();
        CanInitialize = false;
        objectToPlace.objectFollow = obj.GetComponent<ObjectFollow>();

        objectToPlace.AssignChildRenderer();

        // Check if object has child renderer
        if (objectToPlace.childRenderer != null)
        {
            // Find the material index
            int materialIndex = materials.IndexOf(objectToPlace.childRenderer.sharedMaterial);
            
            if (materialIndex >= 0)
            {
                // Assign material index to object
                objectToPlace.materialIndex = materialIndex;
            }
        }
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        MainTilemap.BoxFill(start, TransparentTile, start.x, start.y, start.x + size.x, start.y + size.y);
    }

    #endregion

    #region Saving/Loading Data

   public void SaveGame()
    {
        SaveData saveData = new SaveData();
        saveData.placedObjects = new List<SerializablePlaceableObject>();

        // Convert placed objects to serializable format
        foreach (PlaceableObject obj in placedObjects)
        {
            SerializablePlaceableObject serializableObject = new SerializablePlaceableObject();
            serializableObject.position = obj.transform.position;
            serializableObject.rotation = obj.transform.rotation.eulerAngles;
            serializableObject.prefabName = obj.gameObject.name.Replace("(Clone)", ""); // Remove the "(Clone)" suffix

            int materialIndex = materials.IndexOf(obj.childRenderer.sharedMaterial);

            // Assign the material index to object
            serializableObject.materialIndex = materialIndex;

            saveData.placedObjects.Add(serializableObject);
        }

        string jsonData = JsonUtility.ToJson(saveData);

        // Save data to file
        File.WriteAllText(saveFilePath, jsonData);

        Debug.Log("Game saved.");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            // Read data
            string jsonData = File.ReadAllText(saveFilePath);

            // Convert to SaveData object
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

            // Clear existing placed objects
            foreach (PlaceableObject obj in placedObjects)
            {
                Destroy(obj.gameObject);
            }
            placedObjects.Clear();

            foreach (SerializablePlaceableObject serializableObject in saveData.placedObjects)
            {
                GameObject prefab = prefabList.Find(obj => obj.name == serializableObject.prefabName);
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, serializableObject.position, Quaternion.Euler(serializableObject.rotation));
                    PlaceableObject placeableObject = obj.GetComponent<PlaceableObject>();

                    // Check if object has a child renderer
                    if (placeableObject != null && placeableObject.childRenderer != null)
                    {
                        // Assign material index 
                        int materialIndex = serializableObject.materialIndex;
                        if (materialIndex >= 0 && materialIndex < materials.Count)
                        {
                            // Find the child object with a renderer
                            Renderer[] childRenderers = placeableObject.GetComponentsInChildren<Renderer>();
                            foreach (Renderer renderer in childRenderers)
                            {
                                if (renderer != placeableObject.childRenderer)
                                {
                                    renderer.material = materials[materialIndex];
                                    currentMaterialIndex = materialIndex;
                                    break;
                                }
                            }
                        }
                    }

                    placedObjects.Add(placeableObject);
                }
                else
                {
                    Debug.LogWarning("Prefab not found: " + serializableObject.prefabName);
                }
            }

            Debug.Log("Game loaded.");
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }
}

#endregion