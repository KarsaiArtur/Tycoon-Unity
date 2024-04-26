using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Staff : Placeable
{
    public NavMeshAgent agent;
    bool placed = false;
    float time = 0;
    Exhibit destinationExhibit;
    public int destinationTypeIndex = -1;
    public bool isAvailable = true;

    public void Start()
    {
        isAvailable = true;
        destinationTypeIndex = -1;
    }

    public void Update()
    {
        if (placed && destinationTypeIndex >= 0)
        {
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            {
                agent.isStopped = true;
                if (destinationTypeIndex == 5)
                {
                    time += Time.deltaTime;
                    if (time > 2)
                    {
                        destinationTypeIndex = -1;
                        isAvailable = true;
                    }
                }
                else if (destinationTypeIndex == 0 || destinationTypeIndex == 3)
                {
                    time += Time.deltaTime;

                    destinationExhibit.StaffArrived(1);
                    if (time > 2)
                    {
                        destinationTypeIndex = (destinationTypeIndex + 1) % 6;
                        FindDestination(destinationExhibit);
                    }
                }
                else if(destinationTypeIndex == 1 || destinationTypeIndex == 4)
                {
                    time += Time.deltaTime;

                    destinationExhibit.StaffArrived(2);
                    if (time > 2)
                    {
                        destinationTypeIndex = (destinationTypeIndex + 1) % 6;
                        FindDestination(destinationExhibit);
                    }
                }
                else if(destinationTypeIndex == 2)
                {
                    time += Time.deltaTime;
                    agent.isStopped = false;
                    if (time > 10)
                    {
                        destinationTypeIndex = (destinationTypeIndex + 1) % 6;
                        FindDestination(destinationExhibit);
                    }
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                time += Time.deltaTime;
                if (time > 11)
                {
                    FindDestination(destinationExhibit);
                }
            }
        }
    }

    public virtual void FindJob() { }

    public virtual void FindInsideDestination() { }

    public void FindDestination(Exhibit exhibit)
    {
        if (destinationTypeIndex == 0 || destinationTypeIndex == 4)
            agent.SetDestination(new Vector3(exhibit.entranceGrid.coords[0].x + 0.5f, exhibit.entranceGrid.coords[0].y, exhibit.entranceGrid.coords[0].z + 0.5f));
        if (destinationTypeIndex == 1 || destinationTypeIndex == 3)
            agent.SetDestination(new Vector3(exhibit.exitGrid.coords[0].x + 0.5f, exhibit.exitGrid.coords[0].y, exhibit.exitGrid.coords[0].z + 0.5f));
        if (destinationTypeIndex == 2)
            FindInsideDestination();
        destinationExhibit = exhibit;
        time = 0;
        agent.isStopped = false;
    }

    public override void ClickedOn()
    {
        throw new System.NotImplementedException();
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit);
        playerControl.canBePlaced = true;
        transform.position = new Vector3(playerControl.Round(mouseHit.x), mouseHit.y + 0.5f, playerControl.Round(mouseHit.z));
    }

    public override void FinalPlace()
    {
        agent.Warp(transform.position);
        StaffManager.instance.staffs.Add(this);
        placed = true;
    }
}
