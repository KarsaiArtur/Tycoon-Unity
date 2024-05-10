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
            foreach (Staff staff in staffs)
            {
                if (staff.isAvailable)
                    staff.FindJob();
            }
        }
    }
}
