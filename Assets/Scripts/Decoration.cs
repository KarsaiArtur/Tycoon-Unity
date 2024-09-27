using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;Quaternion rotation;int selectedPrefabId;string tag;int placeablePrice//////////
//////SERIALIZABLE:YES/

public class Decoration : Placeable, Saveable
{
    float height;
    NavMeshObstacle navMeshObstacle;

    public override void Awake()
    {
        height = gameObject.GetComponent<BoxCollider>().size.y;
        base.Awake();
        navMeshObstacle = gameObject.GetComponent<NavMeshObstacle>();
    }

    public override void FinalPlace()
    {
        DecorationManager.instance.AddList(this);
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);

        if (playerControl.canBePlaced && GridManager.instance.GetGrid(mouseHit).GetExhibit() != null)
        {
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
        if (!playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }
        else
        {
            ChangeMaterial(1);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !tag.Equals("Placed"))
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

    public override void Remove()
    {
        DecorationManager.instance.decorations.Remove(this);
        base.Remove();

        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        gameObject.GetComponent<NavMeshObstacle>().enabled = true;
        LoadMenu.objectLoadedEvent.Invoke();
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class DecorationData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public int selectedPrefabId;
        public string tag;
        public int placeablePrice;

        public DecorationData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam)
        {
           _id = _idParam;
           position = positionParam;
           rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
        }
    }

    DecorationData data; 
    
    public string DataToJson(){
        DecorationData data = new DecorationData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<DecorationData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.rotation, data.selectedPrefabId, data.tag, data.placeablePrice);
    }
    
    public string GetFileName(){
        return "Decoration.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, Quaternion rotationParam, int selectedPrefabIdParam, string tagParam, int placeablePriceParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           transform.rotation = rotationParam;
           selectedPrefabId = selectedPrefabIdParam;
           tag = tagParam;
           placeablePrice = placeablePriceParam;
    }
    
    public DecorationData ToData(){
         return new DecorationData(_id, transform.position, transform.rotation, selectedPrefabId, tag, placeablePrice);
    }
    
    public void FromData(DecorationData data){
        
           _id = data._id;
           transform.position = data.position;
           transform.rotation = data.rotation;
           selectedPrefabId = data.selectedPrefabId;
           tag = data.tag;
           placeablePrice = data.placeablePrice;
    }
}
