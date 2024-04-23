using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffManager : MonoBehaviour
{
    public List<Staff> staff = new();
    public List<Staff> availableStaff = new();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (availableStaff.Count > 0)
        {
            foreach (Staff staff in availableStaff)
            {
                FindJob();
            }
        }
    }

    public void FindJob()
    {

    }
}
