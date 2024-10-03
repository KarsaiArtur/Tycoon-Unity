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
    public enum VetJobs
    {
        HealingAnimal,
        PuttingAnimalToSleep,
        GoingToAnimal,
        CarryingAnimal,
        Nothing
    }

    Animal animalOccupied;
    public VetJobs job = VetJobs.Nothing;

    public override void Start()
    {
        base.Start();
        job = VetJobs.Nothing;
    }

    public override void Update()
    {
        base.Update();

        if (workingState == WorkingState.Working && animalOccupied != null && animalOccupied.agent.isOnNavMesh)
        {
            animalOccupied.agent.SetDestination(new Vector3(animalOccupied.transform.position.x, animalOccupied.transform.position.y, animalOccupied.transform.position.z));
            animalOccupied.atDestination = false;
        }
    }

    public override void FindJob()
    {
        isAvailable = false;

        var possibleJobs = new List<(Exhibit exhibit, Animal animal, float percent)>();
        foreach (Exhibit exhibit in ExhibitManager.instance.exhibitList)
        {
            if (exhibit.GetAnimals().Count > 0 && !exhibit.unreachableForStaff)
            {
                foreach (var animal in exhibit.GetAnimals())
                {
                    if (!animal.isOccupiedByVet)
                        possibleJobs.Add((exhibit, animal, animal.health));
                }
            }
        }
        //foreach (var animal in AnimalManager.instance.freeAnimals)
        //{
        //    if (!animal.isOccupiedByVet)
        //        possibleJobs.Add((null, animal, 50));
        //}

        if (possibleJobs.Count > 0)
            FindAnimalToHeal(possibleJobs);
        else
            isAvailable = true;
    }

    public void FindAnimalToHeal(List<(Exhibit exhibit, Animal animal, float health)> animalSickness)
    {
        //ez jön!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        animalSickness = animalSickness.OrderBy(x => x.health).ToList();
        animalOccupied = animalSickness[0].animal;
        if (animalOccupied.health < 75)
        {
            animalOccupied.isOccupiedByVet = true;

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
        if (animalOccupied != null)
        {
            float healthRecovered = UnityEngine.Random.Range(40, 60);
            animalOccupied.health = animalOccupied.health + healthRecovered > 100 ? 100 : animalOccupied.health + healthRecovered;
            animalOccupied.isSick = false;
            animalOccupied.isOccupiedByVet = false;
        }
        return true;
    }

    public override void FindWorkDestination()
    {
        if (animalOccupied != null)
        {
            agent.SetDestination(new Vector3(animalOccupied.transform.position.x, animalOccupied.transform.position.y, animalOccupied.transform.position.z));
        }
    }

    public override string GetCurrentAction()
    {
        return "Healing animal";
    }

    public override void SetToDefault()
    {
        base.SetToDefault();
        job = VetJobs.Nothing;

        if (animalOccupied != null)
        {
            animalOccupied.isOccupiedByVet = false;
            animalOccupied = null;
        }
    }

    public override void Remove()
    {
        base.Remove();

        if (animalOccupied != null)
            animalOccupied.isOccupiedByVet = false;
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
