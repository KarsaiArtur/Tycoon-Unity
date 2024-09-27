using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;float food;string exhibitId//////////
//////SERIALIZABLE:YES/

public class AnimalFood : MonoBehaviour, AnimalVisitable, Saveable
{
    public string _id;
    public int selectedPrefabId;
    public float food = 200;
    /////GENERATE
    private Exhibit exhibit;

    public void Awake()
    {
        _id = Placeable.encodeID(this);
    }

    public void FinalPlace()
    {
        tag = "Placed";
        AnimalVisitableManager.instance.AddList(this);
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().AddFoodPlace(this);
            GetExhibit(GridManager.instance.GetGrid(transform.position).GetExhibit()._id);
        }
    }

    public void Arrived(Animal animal)
    {
        float foodEaten = UnityEngine.Random.Range(40, 60);
        foodEaten = food > foodEaten ? foodEaten : food;
        foodEaten = animal.hunger + foodEaten > 100 ? 100 - animal.hunger : foodEaten;
        animal.hunger += foodEaten;
        GetExhibit().food -= foodEaten;
        food -= foodEaten;
        animal.hungerDetriment = UnityEngine.Random.Range(0.2f, 0.3f);

        if (food <= 0)
        {
            GetExhibit().RemoveFoodPlaces(this);
            animal.GetDestinationVisitable("");
            if (gameObject != null)
                Destroy(gameObject);
        }
    }

    public void Delete()
    {
        AnimalVisitableManager.instance.animalvisitableList.Remove(this);
        Destroy(gameObject);
    }

    public string GetId(){
        return _id;
    }

    public void LoadHelper()
    {
        
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
    public class AnimalFoodData : AnimalVisitableData
    {
        public string _id;
        public string tag;
        public float food;
        public string exhibitId;

        public AnimalFoodData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, float foodParam, string exhibitIdParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           food = foodParam;
           exhibitId = exhibitIdParam;
        }
    }

    AnimalFoodData data; 
    
    public string DataToJson(){
        AnimalFoodData data = new AnimalFoodData(_id, transform.position, transform.rotation, selectedPrefabId, tag, food, exhibitId);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<AnimalFoodData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.food, data.exhibitId);
    }
    
    public string GetFileName(){
        return "AnimalFood.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, float foodParam, string exhibitIdParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           food = foodParam;
           exhibitId = exhibitIdParam;
    }
    
    public AnimalVisitableData ToData(){
         return new AnimalFoodData(_id, transform.position, transform.rotation, selectedPrefabId, tag, food, exhibitId);
    }
    
    public void FromData(AnimalVisitableData data){
        var castedData = (AnimalFoodData)data;
           _id = castedData._id;
           transform.position = castedData.position;
           transform.rotation = castedData.rotation;
           selectedPrefabId = castedData.selectedPrefabId;
           tag = castedData.tag;
           food = castedData.food;
           exhibitId = castedData.exhibitId;
    }
}
