using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag;float water;string exhibitId//////////
//////SERIALIZABLE:YES/

public class WaterTrough : Placeable, AnimalVisitable, Saveable
{
    float height;
    NavMeshObstacle navMeshObstacle;
    public float waterCapacity = 500;
    public float water = 500;
    /////GENERATE
    private Exhibit exhibit;
    bool waiting = false;
    float time = 0;
    public string description;

    public override void Awake()
    {
        height = gameObject.GetComponent<BoxCollider>().size.y;
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
    }

    public void Update()
    {
        if (waiting)
        {
            time += Time.deltaTime;
            if (time > 0.5f)
            {
                Remove();
            }
        }
    }
    
    public override void FinalPlace()
    {
        AnimalVisitableManager.instance.AddList(this);
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().AddWaterPlace(this);
            GetExhibit(GridManager.instance.GetGrid(transform.position).GetExhibit()._id);
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);
        if (!playerControl.canBePlaced || GridManager.instance.GetGrid(transform.position).GetExhibit() == null)
        {
            ChangeMaterial(2);
            playerControl.canBePlaced = false;
        }
        else
        {
            ChangeMaterial(1);
            playerControl.canBePlaced = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (playerControl.placedTags.Contains(collision.collider.tag) && !tag.Equals("Placed"))
        {
            playerControl.canBePlaced = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            playerControl.canBePlaced = true;
            ChangeMaterial(1);
        }
    }

    public void Arrived(Animal animal)
    {
        float waterDrunk = UnityEngine.Random.Range(40, 60);
        waterDrunk = water > waterDrunk ? waterDrunk : water;
        waterDrunk = animal.thirst + waterDrunk > 100 ? 100 - animal.thirst : waterDrunk;
        animal.thirst += waterDrunk;
        GetExhibit().water -= waterDrunk;
        water -= waterDrunk;
        animal.thirstDetriment = UnityEngine.Random.Range(0.45f, 0.55f);
    }

    public void FillWithWater()
    {
        GetExhibit().water += waterCapacity - water;
        water = waterCapacity;
    }

    public override void Remove()
    {
        if (!waiting && GetExhibit().destroyed)
        {
            waiting = true;
            return;
        }
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null && (GetExhibit() == null || GetExhibit().destroyed))
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().AddWaterPlace(this);
            GetExhibit(GridManager.instance.GetGrid(transform.position).GetExhibit()._id);
            waiting = false;
            time = 0;
            return;
        }
        AnimalVisitableManager.instance.animalvisitableList.Remove(this);
        base.Remove();

        if (GetExhibit() != null)
        {
            GetExhibit().water -= water;
            GetExhibit().waterCapacity -= waterCapacity;
            GetExhibit().RemoveWaterTrough(this);
        }
            
        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        navMeshObstacle.enabled = true;
        LoadMenu.objectLoadedEvent.Invoke();
    }
////GENERATED

    public string exhibitId;
    public Exhibit GetExhibit(string id = null)
    {
        id ??=exhibitId;

        if(id != exhibitId || exhibit == null)
        {
            exhibitId = id;
            exhibit = ExhibitManager.instance.exhibitList.Where((element) => element.GetId() == exhibitId).FirstOrDefault();
        }
        return exhibit;
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class WaterTroughData : AnimalVisitableData
    {
        public string _id;
        public int placeablePrice;
        public string tag;
        public float water;
        public string exhibitId;

        public WaterTroughData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam, float waterParam, string exhibitIdParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           water = waterParam;
           exhibitId = exhibitIdParam;
        }
    }

    WaterTroughData data; 
    
    public string DataToJson(){
        WaterTroughData data = new WaterTroughData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag, water, exhibitId);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<WaterTroughData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag, data.water, data.exhibitId);
    }
    
    public string GetFileName(){
        return "WaterTrough.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam, float waterParam, string exhibitIdParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           water = waterParam;
           exhibitId = exhibitIdParam;
    }
    
    public AnimalVisitableData ToData(){
        return new WaterTroughData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag, water, exhibitId);
    }
    
    public void FromData(AnimalVisitableData data){
        var castedData = (WaterTroughData)data;
           _id = castedData._id;
           transform.position = castedData.position;
           selectedPrefabId = castedData.selectedPrefabId;
           transform.rotation = castedData.rotation;
           placeablePrice = castedData.placeablePrice;
           tag = castedData.tag;
           water = castedData.water;
           exhibitId = castedData.exhibitId;
    }
}
