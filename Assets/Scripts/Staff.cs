using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;

public abstract class Staff : Placeable
{
    public NavMeshAgent agent;
    bool placed = false;
    bool collided = false;
    public float time = 0;
    float timeGoal = 0;
    public Exhibit destinationExhibit;
    public Exhibit insideExhibit;
    public bool isAvailable = true;
    public int salary;
    public WorkingState workingState;
    public bool destinationReached = false;

    public enum WorkingState
    {
        GoingToExhibitEntranceToEnter,
        GoingToExhibitExitToEnter,
        GoingToExhibitEntranceToLeave,
        GoingToExhibitExitToLeave,
        Working,
        Resting
    }

    public virtual void Start()
    {
        isAvailable = true;
        workingState = WorkingState.Resting;
    }

    public virtual void Update()
    {
        if (placed)
        {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            {
                destinationReached = true;
            }
            if (destinationReached && Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.2)
            {
                //agent.isStopped = true;
                switch (workingState)
                {
                    case WorkingState.GoingToExhibitEntranceToEnter:
                        time += Time.deltaTime;
                        destinationExhibit.StaffArrived(1);
                        if (!destinationExhibit.GetStaffsAtGate().Contains(this))
                            destinationExhibit.AddStaffsAtGate(this);
                        if (!destinationExhibit.GetStaffs().Contains(this))
                            destinationExhibit.AddStaffs(this);
                        if (time > 1)
                        {
                            workingState = WorkingState.GoingToExhibitExitToEnter;
                            FindDestination(destinationExhibit);
                        }
                        break;
                    case WorkingState.GoingToExhibitExitToEnter:
                        time += Time.deltaTime;
                        if (destinationExhibit.GetStaffsAtGate().Contains(this))
                            destinationExhibit.RemoveStaffsAtGate(this);
                        if (time > 1)
                        {
                            workingState = WorkingState.Working;
                            destinationExhibit.StaffArrived(2);
                            FindDestination(destinationExhibit);
                            insideExhibit = destinationExhibit;
                        }
                        break;
                    case WorkingState.GoingToExhibitEntranceToLeave:
                        time += Time.deltaTime;
                        if (insideExhibit.GetStaffsAtGate().Contains(this))
                            insideExhibit.RemoveStaffsAtGate(this);
                        if (insideExhibit.GetStaffs().Contains(this))
                            insideExhibit.RemoveStaffs(this);
                        if (time > 1)
                        {
                            if (destinationExhibit == null)
                            {
                                workingState = WorkingState.Resting;
                                FindDestination(insideExhibit);
                            }
                            else
                            {
                                workingState = WorkingState.GoingToExhibitEntranceToEnter;
                                FindDestination(destinationExhibit);
                            }
                            insideExhibit.StaffArrived(2);
                            insideExhibit = null;
                        }
                        break;
                    case WorkingState.GoingToExhibitExitToLeave:
                        time += Time.deltaTime;
                        insideExhibit.StaffArrived(1);
                        if (!insideExhibit.GetStaffsAtGate().Contains(this))
                            insideExhibit.AddStaffsAtGate(this);
                        if (time > 1)
                        {
                            workingState = WorkingState.GoingToExhibitEntranceToLeave;
                            FindDestination(insideExhibit);
                        }
                        break;
                    case WorkingState.Working:
                        time += Time.deltaTime;
                        agent.isStopped = false;
                        if (time > timeGoal)
                        {
                            if (!DoJob())
                            {
                                FindDestination(insideExhibit);
                            }
                            else
                            {
                                workingState = WorkingState.GoingToExhibitExitToLeave;
                                isAvailable = true;
                                destinationExhibit = null;
                                FindDestination(insideExhibit);
                            }
                        }
                        break;
                    case WorkingState.Resting:
                        agent.isStopped = true;
                        destinationReached = false;
                        break;
                    default:
                        break;
                }
            }
            if (agent.velocity == Vector3.zero)
            {
                time += Time.deltaTime;
                if (time > 30)
                {
                    SetToDefault();
                }
            }
        }
    }

    public void FindDestination(Exhibit exhibit)
    {
        time = 0;
        timeGoal = UnityEngine.Random.Range(9, 11);
        if (exhibit != null)
        {
            if (workingState == WorkingState.GoingToExhibitEntranceToEnter || workingState == WorkingState.GoingToExhibitEntranceToLeave)
            {
                agent.SetDestination(new Vector3(exhibit.entranceGrid.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), exhibit.entranceGrid.coords[0].y, exhibit.entranceGrid.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
                if (workingState == WorkingState.GoingToExhibitEntranceToEnter)
                {
                    var path = new NavMeshPath();
                    agent.CalculatePath(agent.destination, path);
                    if (path.status != NavMeshPathStatus.PathComplete)
                    {
                        exhibit.SetUnreachableForStaff();
                        SetToDefault();
                    }
                }
            }
            else if (workingState == WorkingState.GoingToExhibitExitToEnter || workingState == WorkingState.GoingToExhibitExitToLeave)
                agent.SetDestination(new Vector3(exhibit.exitGrid.coords[0].x + UnityEngine.Random.Range(0.1f, 0.9f), exhibit.exitGrid.coords[0].y, exhibit.exitGrid.coords[0].z + UnityEngine.Random.Range(0.1f, 0.9f)));
            else if (workingState == WorkingState.Working)
                FindInsideDestination();
            else if (workingState == WorkingState.Resting && exhibit.entranceGrid.neighbours.Length > 0)
            {
                foreach (Grid grid in exhibit.entranceGrid.trueNeighbours)
                {
                    if (grid != null)
                    {
                        agent.SetDestination(new Vector3(grid.coords[0].x + UnityEngine.Random.Range(0, 1.0f), grid.coords[0].y, grid.coords[0].z + UnityEngine.Random.Range(0, 1.0f)));
                        break;
                    }
                }
            }
            agent.isStopped = false;
            destinationReached = false;
        }
    }

    public virtual void FindJob() { }

    public virtual bool DoJob() { return true; }

    public virtual void FindInsideDestination() { }

    public virtual void SetToDefault()
    {
        time = 0;
        destinationReached = false;
        destinationExhibit = null;
        if (GridManager.instance.GetGrid(transform.position).GetExhibit() != null)
            insideExhibit = GridManager.instance.GetGrid(transform.position).GetExhibit();
        else
            insideExhibit = null;
        isAvailable = true;
        workingState = WorkingState.Resting;
    }

    public override void ClickedOn()
    {
        playerControl.SetFollowedObject(this.gameObject, 5);
        playerControl.DestroyCurrentInfopopup();
        var newInfopopup = new GameObject().AddComponent<StaffInfoPopup>();
        newInfopopup.SetClickable(this);
        playerControl.SetInfopopup(newInfopopup);
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);

        Vector3 position = new Vector3(mouseHit.x, mouseHit.y + 0.01f, mouseHit.z);

        RaycastHit[] hits = Physics.RaycastAll(position, -transform.up);

        if (playerControl.canBePlaced)
            ChangeMaterial(1);

        if (!collided)
            playerControl.canBePlaced = true;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Terrain") && playerControl.canBePlaced && gridManager.GetGrid(hit.point).GetExhibit() != null)
            {
                playerControl.canBePlaced = false;
                ChangeMaterial(2);
            }
        }

        transform.position = position;
    }

    void OnCollisionStay(Collision collision)
    {
        var isTagPlaced = playerControl.placedTags.Where(tag => tag.Equals(collision.collider.tag) && collision.collider.tag != "Placed Path");
        if (isTagPlaced.Any() && !playerControl.placedTags.Contains(gameObject.tag))
        {
            collided = true;
            playerControl.canBePlaced = false;
            ChangeMaterial(2);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collided = false;
    }

    public override void FinalPlace()
    {
        agent.Warp(transform.position);
        StaffManager.instance.AddList(this);
        placed = true;
    }

    public void OnDestroy()
    {
        StaffManager.instance.staffList.Remove(this);
    }

    public abstract string GetCurrentAction();

    public void LoadHelper()
    {
        agent.Warp(transform.position);
        placed = true;
        SetToDefault();
        LoadMenu.objectLoadedEvent.Invoke();
    }
    
    public abstract StaffData ToData();
    public abstract void FromData(StaffData data);
}

[Serializable]
public class StaffData
{
    [JsonConverter(typeof(Vector3Converter))]
    public Vector3 position;
    public int selectedPrefabId;
    [JsonConverter(typeof(QuaternionConverter))]
    public Quaternion rotation;
}
