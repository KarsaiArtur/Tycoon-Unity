using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id,Vector3 position,int selectedPrefabId,Quaternion rotation,int placeablePrice,string tag/////
//////SERIALIZABLE:YES/


public class Nature : Placeable, Saveable
{
    float height;
    public NavMeshObstacle navMeshObstacle;
    public int selectedPrefabId;

    public override void Awake()
    {
        List<GameObject> foligeVariants2 = PrefabManager.instance.naturePrefabs.Where(prefab => prefab.name.Contains(name.Replace("(Clone)", ""))).ToList();
        var chosenPrefab = foligeVariants2[UnityEngine.Random.Range(0, foligeVariants2.Count)];
        selectedPrefabId = chosenPrefab.GetInstanceID();
        //var trunkPos = chosenPrefab.transform.Find("trunk").transform.position;
        var selectedVariantInstance = Instantiate(chosenPrefab, transform.position, transform.rotation);
        selectedVariantInstance.transform.SetParent(transform);
        height = selectedVariantInstance.GetComponent<BoxCollider>().size.y;
        base.Awake();

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
        NatureManager.instance.AddList(this);
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        GridManager.instance.GetGrid(transform.position).natures.Add(this);
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
        {
            GridManager.instance.GetGrid(transform.position).GetExhibit().foliages.Add(this);
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

        NatureManager.instance.natures.Remove(this);
        ZooManager.instance.ChangeMoney(placeablePrice * 0.1f);
        GridManager.instance.GetGrid(transform.position).natures.Remove(this);
        var tempGrid = GridManager.instance.GetGrid(transform.position);
        if (tempGrid.GetExhibit() != null)
        {
            tempGrid.GetExhibit().RemoveNature(this);
        }
        Destroy(gameObject);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class NatureData
    {
        public string _id;
        public Vector3 position;
        public int selectedPrefabId;
        public Quaternion rotation;
        public int placeablePrice;
        public string tag;

        public NatureData(string _id, Vector3 position, int selectedPrefabId, Quaternion rotation, int placeablePrice, string tag)
        {
           this._id = _id;
           this.position = position;
           this.selectedPrefabId = selectedPrefabId;
           this.rotation = rotation;
           this.placeablePrice = placeablePrice;
           this.tag = tag;
        }
    }

    NatureData data; 
    
    public string DataToJson(){
        NatureData data = new NatureData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<NatureData>(json);
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag);
    }
    
    public string GetFileName(){
        return "Nature.json";
    }
    
    void SetData(string _id, Vector3 position, int selectedPrefabId, Quaternion rotation, int placeablePrice, string tag){ 
        
           this._id = _id;
           this.transform.position = position;
           this.selectedPrefabId = selectedPrefabId;
           this.transform.rotation = rotation;
           this.placeablePrice = placeablePrice;
           this.tag = tag;
    }
    
    public NatureData ToData(){
         return new NatureData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
    }
    
    public void FromData(NatureData data){
        
           this._id = data._id;
           this.transform.position = data.position;
           this.selectedPrefabId = data.selectedPrefabId;
           this.transform.rotation = data.rotation;
           this.placeablePrice = data.placeablePrice;
           this.tag = data.tag;
    }
}
