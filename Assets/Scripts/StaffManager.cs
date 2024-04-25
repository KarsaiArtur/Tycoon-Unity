using System.Collections.Generic;
using UnityEngine;

public class StaffManager : MonoBehaviour
{
    public static StaffManager instance;
    public List<Staff> staffs = new();

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (staffs.Count > 0 && GridManager.instance.exhibits.Count > 0)
        {
            //for (int i = 0; i < availableStaff.Count; i++)
            //{
            //    staff[i].FindJob();
            //}
            foreach (Staff staff in staffs)
            {
                if (staff.isAvailable)
                    staff.FindJob();
            }
        }
    }
}
