using System.Collections.Generic;
using UnityEngine;

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
        if (staffList.Count > 0 && GridManager.instance.exhibits.Count > 0)
        {
            foreach (Staff staff in staffList)
            {
                if (staff.isAvailable)
                    staff.FindJob();
            }
        }
    }
}
