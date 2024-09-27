using System.Collections.Generic;
using UnityEngine;

/////Attributes, DONT DELETE
//////List<Staff> staffList//////////

public class StaffManager : MonoBehaviour, Manager
{
    public static StaffManager instance;
    public List<Staff> staffList;

    void Awake()
    {
        instance = this;
        if(LoadMenu.loadedGame != null){
            LoadMenu.currentManager = this;
            //LoadMenu.instance.LoadData(this);
            LoadMenu.objectLoadedEvent.Invoke();
        }
    }

    public void AddList(Staff staff){
        staffList.Add(staff);
        staff.transform.SetParent(StaffManager.instance.transform);
    }

    void Update()
    {
        if (staffList.Count > 0 && ExhibitManager.instance.exhibitList.Count > 0)
        {
            foreach (Staff staff in staffList)
            {
                if (staff.isAvailable)
                    staff.FindJob();
            }
        }
    }

    public bool GetIsLoaded()
    {
        return staffList.Count + 1 == LoadMenu.loadedObjects;
    }
}
