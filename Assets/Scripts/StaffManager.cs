using System.Collections.Generic;
using UnityEngine;

/////Attributes, DONT DELETE
//////List<Staff> staffList//////////

public class StaffManager : MonoBehaviour
{
    public static StaffManager instance;
    public List<Staff> staffList;

    void Awake()
    {
        instance = this;
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
}
