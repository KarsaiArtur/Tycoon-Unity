using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;Quaternion rotation;int placeablePrice;string tag//////////
//////SERIALIZABLE:YES/

public class Maintainer : Staff, Saveable
{
    public enum MaintainerJobs
    {
        RepairingFence,
        PickingUpTrash,
        EmptyingTrashCan,
        Nothing
    }

    Fence fenceToRepair;
    TrashCan trashCanToEmpty;
    Grid gridDestination;
    public MaintainerJobs job = MaintainerJobs.Nothing;

    public override void Start()
    {
        base.Start();
        job = MaintainerJobs.Nothing;
    }

    public override void FindJob()
    {
        isAvailable = false;

        var possibleJobs = new List<(Fence fence, TrashCan trashCan, MaintainerJobs maintainerJob, float percent)>();

        if (FenceManager.instance.fences.Count > 0)
        {
            foreach (Fence fence in FenceManager.instance.fences)
            {
                if (!fence.isBeingFixed && fence.health < fence.maxHealth)
                {
                    possibleJobs.Add((fence, null, MaintainerJobs.RepairingFence, (float)fence.health / fence.maxHealth * 100));
                }
            }
        }
        if (TrashCanManager.instance.trashCans.Count > 0)
        {
            foreach (TrashCan trashCan in TrashCanManager.instance.trashCans)
            {
                if (!trashCan.isBeingEmptied && trashCan.capacity < 25)
                {
                    possibleJobs.Add((null, trashCan, MaintainerJobs.EmptyingTrashCan, trashCan.capacity));
                }
            }
        }
        if (TrashCanManager.instance.trashOnTheGround.Count > 0 && !TrashCanManager.instance.trashIsBeingPickedUp)
        {
            possibleJobs.Add((null, null, MaintainerJobs.PickingUpTrash, 100 - 5 * TrashCanManager.instance.trashOnTheGround.Count));
        }

        if (possibleJobs.Count > 0)
            ChooseJob(possibleJobs);
        else
            isAvailable = true;
    }

    public void ChooseJob(List<(Fence fence, TrashCan trashCan, MaintainerJobs maintainerJob, float percent)> possibleJobs)
    {
        possibleJobs = possibleJobs.OrderBy(x => x.percent).ToList();
        job = possibleJobs[0].maintainerJob;

        fenceToRepair = possibleJobs[0].fence;
        if (fenceToRepair != null)
            fenceToRepair.isBeingFixed = true;

        trashCanToEmpty = possibleJobs[0].trashCan;
        if (trashCanToEmpty != null)
            trashCanToEmpty.isBeingEmptied = true;
        //itt tartok!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        if (insideExhibit != null)
        {
            if (fenceToRepair.grid1.GetExhibit() != null && fenceToRepair.grid2.GetExhibit() != null)
            {
                if (fenceToRepair.grid1.GetExhibit() == insideExhibit)
                {
                    gridDestination = fenceToRepair.grid1;
                    workingState = WorkingState.Working;
                    FindDestination(insideExhibit);
                    return;
                }
                else if (fenceToRepair.grid2.GetExhibit() == insideExhibit)
                {
                    gridDestination = fenceToRepair.grid2;
                    workingState = WorkingState.Working;
                    FindDestination(insideExhibit);
                    return;
                }
                else
                {
                    workingState = WorkingState.GoingToExhibitExitToLeave;
                    fenceToRepair.isBeingFixed = false;
                    fenceToRepair = null;
                    FindDestination(insideExhibit);
                    return;
                }
            }
            else
            {
                workingState = WorkingState.GoingToExhibitExitToLeave;
                fenceToRepair.isBeingFixed = false;
                fenceToRepair = null;
                FindDestination(insideExhibit);
                return;
            }
        }
        else
        {
            if (fenceToRepair.grid1.GetExhibit() != null && fenceToRepair.grid2.GetExhibit() != null)
            {
                if (!fenceToRepair.grid1.GetExhibit().unreachableForStaff)
                {
                    gridDestination = fenceToRepair.grid1;
                    destinationExhibit = fenceToRepair.grid1.GetExhibit();
                    workingState = WorkingState.GoingToExhibitEntranceToEnter;
                    FindDestination(fenceToRepair.grid1.GetExhibit());
                    return;
                }
                else if (!fenceToRepair.grid2.GetExhibit().unreachableForStaff)
                {
                    gridDestination = fenceToRepair.grid2;
                    destinationExhibit = fenceToRepair.grid2.GetExhibit();
                    workingState = WorkingState.GoingToExhibitEntranceToEnter;
                    FindDestination(fenceToRepair.grid1.GetExhibit());
                    return;
                }
                else
                {
                    SetToDefault();
                }
            }
            else
            {
                if (fenceToRepair.grid1.GetExhibit() == null)
                    gridDestination = fenceToRepair.grid1;
                else
                    gridDestination = fenceToRepair.grid2;

                workingState = WorkingState.Working;
                FindDestination(null);
                return;
            }
        }

        isAvailable = true;
    }

    public override bool DoJob()
    {
        if (fenceToRepair != null)
        {
            fenceToRepair.health += UnityEngine.Random.Range(1, fenceToRepair.maxHealth - fenceToRepair.health + 1);
            fenceToRepair.isBeingFixed = false;
        }
        return true;
    }

    public override void FindWorkDestination()
    {
        if (fenceToRepair != null)
        {
            if (fenceToRepair.timesRotated == 0)
            {
                if (fenceToRepair.grid1 == gridDestination)
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.9f));
                else
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.1f));
            }
            else if (fenceToRepair.timesRotated == 1)
            {
                if (fenceToRepair.grid1 == gridDestination)
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.9f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
                else
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.1f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
            }
            else if (fenceToRepair.timesRotated == 2)
            {
                if (fenceToRepair.grid1 == gridDestination)
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.1f));
                else
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.9f));
            }
            else if (fenceToRepair.timesRotated == 3)
            {
                if (fenceToRepair.grid1 == gridDestination)
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.1f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
                else
                    agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.9f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
            }
        }

        var path = new NavMeshPath();
        agent.CalculatePath(agent.destination, path);
        if (path.status != NavMeshPathStatus.PathComplete || fenceToRepair == null)
        {
            SetToDefault();
        }
    }

    public override string GetCurrentAction()
    {
        return "Repairing fence";
    }

    public override void SetToDefault()
    {
        base.SetToDefault();
        if (fenceToRepair != null)
        {
            fenceToRepair.isBeingFixed = false;
            fenceToRepair = null;
        }
    }

    public override void Remove()
    {
        base.Remove();

        if (fenceToRepair != null)
            fenceToRepair.isBeingFixed = false;
        Destroy(gameObject);
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class MaintainerData : StaffData
    {
        public string _id;
        public int placeablePrice;
        public string tag;

        public MaintainerData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, Quaternion rotationParam, int placeablePriceParam, string tagParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           rotation = rotationParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
        }
    }

    MaintainerData data; 
    
    public string DataToJson(){
        MaintainerData data = new MaintainerData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
        return JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });;
    }
    
    public void FromJson(string json){
        data = JsonConvert.DeserializeObject<MaintainerData>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });
        SetData(data._id, data.position, data.selectedPrefabId, data.rotation, data.placeablePrice, data.tag);
    }
    
    public string GetFileName(){
        return "Maintainer.json";
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
        return new MaintainerData(_id, transform.position, selectedPrefabId, transform.rotation, placeablePrice, tag);
    }
    
    public override void FromData(StaffData data){
        var castedData = (MaintainerData)data;
           _id = castedData._id;
           transform.position = castedData.position;
           selectedPrefabId = castedData.selectedPrefabId;
           transform.rotation = castedData.rotation;
           placeablePrice = castedData.placeablePrice;
           tag = castedData.tag;
    }
}