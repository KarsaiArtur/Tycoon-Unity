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
    Fence fenceToRepair;
    Grid gridDestination;

    public override void Start()
    {
        base.Start();
        salary = 200;
    }

    public override void FindJob()
    {
        isAvailable = false;

        var fenceHealths = new List<(Fence fence, float closeness)>();
        if (FenceManager.instance.fences.Count > 0)
        {
            foreach (Fence fence in FenceManager.instance.fences)
            {
                if (!fence.isBeingFixed && fence.health < fence.maxHealth)
                {
                    fenceHealths.Add((fence, Vector3.Distance(transform.position, fence.transform.position)));
                }
            }
        }

        if (fenceHealths.Count > 0)
            FindFenceToRepair(fenceHealths);
    }

    public void FindFenceToRepair(List<(Fence fence, float closeness)> fenceHealths)
    {
        fenceHealths = fenceHealths.OrderBy(x => x.fence.health).ThenBy(x => x.closeness).ToList();
        fenceToRepair = fenceHealths[0].fence;
        fenceToRepair.isBeingFixed = true;

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
                    destinationExhibit = insideExhibit;
                    FindDestination(insideExhibit);
                    return;
                }
            }
            else
            {
                workingState = WorkingState.GoingToExhibitExitToLeave;
                destinationExhibit = insideExhibit;
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
        if ((gridDestination.coords[0] + new Vector3(0.5f, 0, 0.5f)).x > fenceToRepair.transform.position.x)
        {
            agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.1f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
        }
        if ((gridDestination.coords[0] + new Vector3(0.5f, 0, 0.5f)).x < fenceToRepair.transform.position.x)
        {
            agent.SetDestination(new Vector3(gridDestination.coords[0].x + 0.9f, gridDestination.coords[0].y, gridDestination.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
        }
        if ((gridDestination.coords[0] + new Vector3(0.5f, 0, 0.5f)).z > fenceToRepair.transform.position.z)
        {
            agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.1f));
        }
        if ((gridDestination.coords[0] + new Vector3(0.5f, 0, 0.5f)).z < fenceToRepair.transform.position.z)
        {
            agent.SetDestination(new Vector3(gridDestination.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), gridDestination.coords[0].y, gridDestination.coords[0].z + 0.9f));
        }

        var path = new NavMeshPath();
        agent.CalculatePath(agent.destination, path);
        if (path.status != NavMeshPathStatus.PathComplete)
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
        return "Maintainter.json";
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