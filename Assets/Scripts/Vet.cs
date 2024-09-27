using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Vet : Staff, Saveable
{
    Animal animalToHeal;

    public override void Start()
    {
        base.Start();
        salary = 500;
    }

    public override void Update()
    {
        base.Update();

        if (workingState == WorkingState.Working && animalToHeal != null && animalToHeal.agent.isOnNavMesh)
        {
            animalToHeal.agent.SetDestination(new Vector3(animalToHeal.transform.position.x, animalToHeal.transform.position.y, animalToHeal.transform.position.z));
            animalToHeal.atDestination = false;
        }
    }

    public override void FindJob()
    {
        isAvailable = false;

        var animalSickness = new List<(Exhibit exhibit, Animal animal, float health)>();
        foreach (Exhibit exhibit in ExhibitManager.instance.exhibitList)
        {
            if (exhibit.GetAnimals().Count > 0 && !exhibit.unreachableForStaff)
            {
                foreach (var animalId in exhibit.GetAnimals())
                {
                    var animal = animalId;
                    if (!animal.isGettingHealed)
                        animalSickness.Add((exhibit, animal, animal.health));
                }
            }
        }

        if (animalSickness.Count > 0)
            FindAnimalToHeal(animalSickness);
        else
            isAvailable = true;
    }

    public void FindAnimalToHeal(List<(Exhibit exhibit, Animal animal, float health)> animalSickness)
    {
        animalSickness = animalSickness.OrderBy(x => x.health).ToList();
        animalToHeal = animalSickness[0].animal;
        if (animalToHeal.health < 75)
        {
            animalToHeal.isGettingHealed = true;

            destinationExhibit = animalSickness[0].exhibit;

            if (insideExhibit != null)
            {
                if (insideExhibit == destinationExhibit)
                {
                    workingState = WorkingState.Working;
                    FindDestination(destinationExhibit);
                    return;
                }
                else
                {
                    workingState = WorkingState.GoingToExhibitExitToLeave;
                    FindDestination(insideExhibit);
                    return;
                }
            }
            workingState = WorkingState.GoingToExhibitEntranceToEnter;
            FindDestination(destinationExhibit);
            return;
        }
        isAvailable = true;
    }

    public override bool DoJob()
    {
        if (animalToHeal != null)
        {
            float healthRecovered = UnityEngine.Random.Range(40, 60);
            animalToHeal.health = animalToHeal.health + healthRecovered > 100 ? 100 : animalToHeal.health + healthRecovered;
            animalToHeal.isSick = false;
            animalToHeal.isGettingHealed = false;
        }
        return true;
    }

    public override void FindInsideDestination()
    {
        if (animalToHeal != null)
        {
            agent.SetDestination(new Vector3(animalToHeal.transform.position.x, animalToHeal.transform.position.y, animalToHeal.transform.position.z));
        }
    }

    public override string GetCurrentAction()
    {
        return "Healing animal";
    }

    public override void SetToDefault()
    {
        base.SetToDefault();
        if (animalToHeal != null)
        {
            animalToHeal.isGettingHealed = false;
            animalToHeal = null;
        }
    }

    public override void Remove()
    {
        base.Remove();

        if (animalToHeal != null)
            animalToHeal.isGettingHealed = false;
        Destroy(gameObject);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class VetData : StaffData
    {
        public string _id;
        public int placeablePrice;
        public string tag;

        public VetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
        }
    }

    VetData data; 
    
    public string DataToJson(){
        VetData data = new VetData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<VetData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag);
    }
    
    public string GetFileName(){
        return "Vet.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           transform.rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
    }
    
    public override StaffData ToData(){
         return new VetData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
    }
    
    public override void FromData(StaffData data){
        var castedData = (VetData)data;
           _id = castedData._id;
           transform.position = castedData.position;
           selectedPrefabId = castedData.selectedPrefabId;
           transform.rotation = castedData.rotation;
           placeablePrice = castedData.placeablePrice;
           tag = castedData.tag;
    }
}
