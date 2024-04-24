using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Staff : Placeable
{
    public NavMeshAgent agent;
    bool atDestination = true;
    bool placed = false;
    float terraintHeight;
    float time = 0;

    public void Update()
    {
        if (placed)
        {
            if (atDestination)
            {
                if (!StaffManager.instance.availableStaff.Contains(this))
                    StaffManager.instance.availableStaff.Add(this);
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(agent.destination.x, agent.destination.z)) <= 0.1)
            {
                agent.isStopped = true;
                time += Time.deltaTime;
                if (time > 10)
                {
                    atDestination = true;
                }
            }
            else if (agent.velocity == Vector3.zero)
            {
                time += Time.deltaTime;
                if (time > 10)
                {
                    atDestination = true;
                }
            }
        }
    }

    public virtual void FindJob()
    {

    }

    public void FindDestination(Exhibit exhibit)
    {
        agent.SetDestination(exhibit.exitGrid.coords[0]);
        atDestination = false;
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
        StaffManager.instance.availableStaff.Add(this);
        StaffManager.instance.staff.Add(this);
        placed = true;
    }
}
