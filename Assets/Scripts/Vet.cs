using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Vet : Staff, Saveable
{

    Animal animalOccupied;
    public bool aiming = false;
    int shootingDistance = 5;
    IEnumerator coroutine;
    bool CRRunning = false;

    public override List<StaffJob> GetJobTypes() => new List<StaffJob> { StaffJob.HealingAnimal, StaffJob.PuttingAnimalToSleep };

    public override void Start()
    {
        base.Start();
        job = StaffJob.Nothing;
    }

    public override void Update()
    {
        base.Update();

        if (workingState == WorkingState.Working && job == StaffJob.HealingAnimal && animalOccupied != null && animalOccupied.agent.isOnNavMesh)
        {
            animalOccupied.agent.SetDestination(animalOccupied.transform.position);
            animalOccupied.atDestination = false;
        }
        else if (workingState == WorkingState.Working && job == StaffJob.PuttingAnimalToSleep && animalOccupied != null && animalOccupied.agent.isOnNavMesh)
        {
            if (!CRRunning)
            {
                coroutine = Aiming();
                StartCoroutine(coroutine);
            }
            if (!aiming)
                agent.SetDestination(animalOccupied.transform.position);
            else
            {
                //animation?
                time += Time.deltaTime;
                if (time > 3)
                {
                    DoJob();
                }
            }
        }
    }

    IEnumerator Aiming()
    {
        CRRunning = true;

        while (!aiming && animalOccupied != null && workingState == WorkingState.Working && job == StaffJob.PuttingAnimalToSleep)
        {
            Vector3 shootPos = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            Vector3 targetPos = new Vector3(animalOccupied.transform.position.x, animalOccupied.transform.position.y + 0.25f, animalOccupied.transform.position.z);
            RaycastHit hit;

            if (Physics.Raycast(shootPos, (targetPos - shootPos), out hit, shootingDistance) && hit.transform.gameObject == animalOccupied.gameObject)
            {
                agent.isStopped = true;
                aiming = true;
                time = 0;
            }

            yield return new WaitForSeconds(1);
        }
    }

    public override void FindJob()
    {
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
            insideExhibit = GridManager.instance.GetGrid(transform.position).GetExhibit();
        else if (workingState != WorkingState.GoingToExhibitEntranceToLeave)
            insideExhibit = null;

        isAvailable = false;
        aiming = false;
        CRRunning = false;

        var possibleJobs = new List<(Exhibit exhibit, Animal animal, StaffJob vetJob, float percent)>();
        foreach (Exhibit exhibit in ExhibitManager.instance.exhibitList)
        {
            if (exhibit.GetAnimals().Count > 0 && !exhibit.unreachableForStaff)
            {
                foreach (var animal in exhibit.GetAnimals())
                {
                    if (!animal.isOccupiedByVet && animal.health < 75)
                        possibleJobs.Add((exhibit, animal, StaffJob.HealingAnimal, animal.health));
                }
            }
        }
        foreach (var animal in AnimalManager.instance.freeAnimals)
        {
            if (!animal.isOccupiedByVet && !animal.isSlept)
                possibleJobs.Add((null, animal, StaffJob.PuttingAnimalToSleep, 50));
        }

        if (possibleJobs.Count > 0)
            ChooseAnimal(possibleJobs);
        else
            isAvailable = true;
    }

    public void ChooseAnimal(List<(Exhibit exhibit, Animal animal, StaffJob vetJob, float percent)> possibleJobs)
    {
        possibleJobs = possibleJobs.OrderBy(x => x.percent).ToList();
        animalOccupied = possibleJobs[0].animal;
        animalOccupied.isOccupiedByVet = true;
        destinationExhibit = possibleJobs[0].exhibit;
        job = possibleJobs[0].vetJob;

        if (insideExhibit != null)
        {
            if (insideExhibit == destinationExhibit)
            {
                workingState = WorkingState.Working;
                FindDestination(destinationExhibit);
                return;
            }
            else if (destinationExhibit != null)
            {
                workingState = WorkingState.GoingToExhibitExitToLeave;
                FindDestination(insideExhibit);
                return;
            }
            else
            {
                workingState = WorkingState.GoingToExhibitExitToLeave;
                animalOccupied.isOccupiedByVet = false;
                animalOccupied = null;
                FindDestination(insideExhibit);
                return;
            }
        }
        if (destinationExhibit != null)
        {
            workingState = WorkingState.GoingToExhibitEntranceToEnter;
            FindDestination(destinationExhibit);
            return;
        }
        workingState = WorkingState.Working;
        FindDestination(destinationExhibit);
    }

    public override bool DoJob()
    {
        if (animalOccupied != null && job == StaffJob.HealingAnimal)
        {
            float healthRecovered = UnityEngine.Random.Range(40, 60);
            animalOccupied.health = animalOccupied.health + healthRecovered > 100 ? 100 : animalOccupied.health + healthRecovered;
            animalOccupied.isSick = false;
            animalOccupied.isOccupiedByVet = false;
            job = StaffJob.Nothing;
        }
        else if (animalOccupied != null && job == StaffJob.PuttingAnimalToSleep)
        {
            agent.isStopped = false;
            aiming = false;
            CRRunning = false;
            animalOccupied.isOccupiedByVet = false;
            animalOccupied.Sleep();
            SetToDefault();
        }
        return true;
    }

    public override void FindWorkDestination()
    {
        if (animalOccupied != null && (job == StaffJob.HealingAnimal || job == StaffJob.PuttingAnimalToSleep))
        {
            agent.SetDestination(animalOccupied.transform.position);
        }
    }

    public override void SetToDefault()
    {
        base.SetToDefault();
        aiming = false;
            CRRunning = false;

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
