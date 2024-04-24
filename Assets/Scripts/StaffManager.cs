using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffManager : MonoBehaviour
{
    public static StaffManager instance;
    public List<Staff> staff = new();
    public List<Staff> availableStaff = new();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (availableStaff.Count > 0 && GridManager.instance.exhibits.Count > 0)
        {
            for (int i = 0; i < availableStaff.Count; i++)
            {
                staff[i].FindJob();
            }
        }
    }
}
