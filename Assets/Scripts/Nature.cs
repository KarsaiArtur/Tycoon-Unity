using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Nature : Placeable, Saveable
{
    float height;
    public NavMeshObstacle navMeshObstacle;
    GameObject chosenPrefab;

    public override void Awake()
    {
        List<GameObject> foligeVariants2 = PrefabManager.instance.naturePrefabs.Where(prefab => prefab.name.Contains(name.Replace("(Clone)", ""))).ToList();
        chosenPrefab = foligeVariants2[UnityEngine.Random.Range(0, foligeVariants2.Count)];
        //var trunkPos = chosenPrefab.transform.Find("trunk").transform.position;
        var selectedVariantInstance = Instantiate(chosenPrefab, transform.position, transform.rotation);
        selectedVariantInstance.transform.SetParent(transform);
        height = selectedVariantInstance.GetComponent<BoxCollider>().size.y;
        base.Awake();
        Debug.Log(selectedPrefabId);

        navMeshObstacle = selectedVariantInstance.GetComponent<NavMeshObstacle>();

        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<BoxCollider>())
        {
            var newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.size = collider.size;
            //newCollider.center = new Vector3(collider.center.x - trunkPos.x, collider.center.y - trunkPos.y + 0.5f, collider.center.z - trunkPos.z);
            newCollider.center = collider.center;
            Destroy(collider);
        }
        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<SphereCollider>())
        {
            var newCollider = gameObject.AddComponent<SphereCollider>();
            newCollider.radius = collider.radius;
            //newCollider.center = new Vector3(collider.center.x - trunkPos.x, collider.center.y - trunkPos.y + 0.5f, collider.center.z - trunkPos.z);
            newCollider.center = collider.center;
            Destroy(collider);
        }
        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<CapsuleCollider>())
        {
            var newCollider = gameObject.AddComponent<CapsuleCollider>();
            newCollider.radius = collider.radius;
            newCollider.height = collider.height;
            newCollider.center = collider.center;
            Destroy(collider);
        }
    }

    public override void FinalPlace()
    {
        selectedPrefabId = chosenPrefab.GetInstanceID();
        NatureManager.instance.AddList(this);
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        GridManager.instance.GetGrid(transform.position).AddNatures(this);
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().GetFoliages().Add(this);
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit); 
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);
        if (!playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }
        else{
            ChangeMaterial(1);
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

    public override void Remove()
    {
        base.Remove();

        NatureManager.instance.natureList.Remove(this);
        GridManager.instance.GetGrid(transform.position).RemoveNatures(this);
        var tempGrid = GridManager.instance.GetGrid(transform.position);
        if (tempGrid.GetExhibit() != null)
        {
            tempGrid.GetExhibit().RemoveNature(this);
        }
        Destroy(gameObject);
    }

    public void LoadHelper()
    {
        GridManager.instance.GetGrid(transform.position).AddNatures(this);
        Debug.Log("Nature LoadHelper");
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class NatureData
    {
        public string _id;
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 position;
        public int selectedPrefabId;
        [JsonConverter(typeof(QuaternionConverter))]
        public Quaternion rotation;
        public int placeablePrice;
        public string tag;

        public NatureData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
        }
    }

    NatureData data; 
    
    public string DataToJson(){
        NatureData data = new NatureData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<NatureData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag);
    }
    
    public string GetFileName(){
        return "Nature.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
    }
    
    public NatureData ToData(){
         return new NatureData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
    }
    
    public void FromData(NatureData data){
        
           _id = data._id;
           transform.position = data.position;
           selectedPrefabId = data.selectedPrefabId;
           transform.rotation = data.rotation;
           placeablePrice = data.placeablePrice;
           tag = data.tag;
    }
}
