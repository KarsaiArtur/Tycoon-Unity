
using System;
using UnityEngine;
using Newtonsoft.Json;

public interface AnimalVisitable
{
    public abstract void Arrived(Animal animal);
    public abstract string GetId();
    public AnimalVisitableData ToData();
    public void FromData(AnimalVisitableData data);
    public abstract void LoadHelper();
}

[Serializable]
public class AnimalVisitableData
{
    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 position;
    public int selectedPrefabId;
    [JsonConverter(typeof(QuaternionConverter))]
    public Quaternion rotation;
}
