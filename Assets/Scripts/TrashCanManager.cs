using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static TrashCan;

/////Saveable Attributes, DONT DELETE
//////List<TrashCan> trashcanList;Vector3[] trashOnTheGroundCoords//////////

public class TrashCanManager : MonoBehaviour, Manager, Saveable
{
    static public TrashCanManager instance;
    public List<TrashCan> trashcanList;
    public List<GameObject> trashOnTheGround;
    public Vector3[] trashOnTheGroundCoords = new Vector3[0];
    public bool trashIsBeingPickedUp = false;
    
    void Start()
    {
        instance = this;
        trashIsBeingPickedUp = false;
        if (LoadMenu.loadedGame != null)
        {
            LoadMenu.currentManager = this;
            LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(TrashCan trashCan)
    {
        trashcanList.Add(trashCan);
        trashCan.transform.SetParent(TrashCanManager.instance.transform);
    }

    public bool GetIsLoaded()
    {
        return data.trashcanList.Count + 1 == LoadMenu.loadedObjects;
    }

    public void AddTrashOnTheGround(GameObject trash)
    {
        trashOnTheGround.Add(trash);
        var temp = new Vector3[trashOnTheGroundCoords.Length + 1];

        for (int i = 0; i < trashOnTheGroundCoords.Length; i++)
            temp[i] = trashOnTheGroundCoords[i];

        temp[temp.Length - 1] = trash.transform.position;
        trashOnTheGroundCoords = temp;
        GridManager.instance.GetGrid(trash.transform.position).trashCount++;
    }

    public void RemoveTrashOnTheGround(int index)
    {
        GridManager.instance.GetGrid(trashOnTheGroundCoords[index]).trashCount--;
        trashOnTheGround.RemoveAt(index);
        var temp = new Vector3[trashOnTheGroundCoords.Length - 1];

        for (int i = 0; i < trashOnTheGroundCoords.Length; i++)
        {
            if (i < index)
                temp[i] = trashOnTheGroundCoords[i];
            else if (i > index)
                temp[i - 1] = trashOnTheGroundCoords[i];
        }

        trashOnTheGroundCoords = temp;
    }

    public void LoadHelper()
    {
        foreach (var trashPos in trashOnTheGroundCoords)
        {
            var playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
            var tempTrashOnTheGround = Instantiate(playerControl.trashOnTheGroundPrefabs[Random.Range(0, playerControl.trashOnTheGroundPrefabs.Count)], trashPos, Quaternion.identity);
            tempTrashOnTheGround.tag = "Placed";
            trashOnTheGround.Add(tempTrashOnTheGround);
        }
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    public class TrashCanManagerData
    {
        public List<TrashCanData> trashcanList;
        [JsonConverter(typeof(Vector3ArrayConverter))]
        public Vector3[] trashOnTheGroundCoords;

        public TrashCanManagerData(List<TrashCanData> trashcanListParam, Vector3[] trashOnTheGroundCoordsParam)
        {
           trashcanList = trashcanListParam;
           trashOnTheGroundCoords = trashOnTheGroundCoordsParam;
        }
    }

    TrashCanManagerData data; 
    
    public string DataToJson(){

        List<TrashCanData> trashcanList = new List<TrashCanData>();
        foreach(var element in this.trashcanList){
            trashcanList.Add(element.ToData());
        }
        TrashCanManagerData data = new TrashCanManagerData(trashcanList, trashOnTheGroundCoords);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<TrashCanManagerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data.trashcanList, data.trashOnTheGroundCoords);
    }
    
    public string GetFileName(){
        return "TrashCanManager.json";
    }
    
    void SetData(List<TrashCanData> trashcanListParam, Vector3[] trashOnTheGroundCoordsParam){ 
        
        foreach(var element in trashcanListParam){
            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);
            var script = spawned.GetComponent<TrashCan>();
            script.FromData(element);
            script.LoadHelper();
            AddList(script);
        }

           trashOnTheGroundCoords = trashOnTheGroundCoordsParam;
    }
}
