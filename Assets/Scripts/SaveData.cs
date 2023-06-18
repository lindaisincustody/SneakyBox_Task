using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<SerializablePlaceableObject> placedObjects;
}

[Serializable]
public class SerializablePlaceableObject
{
    public string prefabName;
    public Vector3 position;
    public Vector3 rotation;
    public int materialIndex;
}
