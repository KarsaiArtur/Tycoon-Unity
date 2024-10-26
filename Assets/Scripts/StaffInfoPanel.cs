using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffInfoPanel : MonoBehaviour
{
    Staff staff;
    private PlayerControl playerControl;
    public TextMeshProUGUI monthlyExpenses;
    public TextMeshProUGUI description;
    public GameObject jobIconPanel;
    List<GameObject> staffJobList = new List<GameObject>();
     void Start(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();

        SetData();
        foreach(var placeableButton in UIMenu.Instance.placeableListPanel.GetComponentsInChildren<PlaceableButton>()){
            placeableButton.m_onDown.AddListener(SetData);
        }
    }

    void SetData(){
        staff = playerControl.curPlaceable.GetComponent<Staff>();
        monthlyExpenses.text = staff.GetMonthlyExpense().ToString() + " $";
        description.text = staff.description;

        var staffJobIcon = jobIconPanel.transform.GetChild(0);
        staffJobList.Skip(1).ToList().ForEach(element => Destroy(element));
        staffJobList.Clear();
        foreach(var staffJob in staff.GetJobTypes()){
            var newstaffJobIcon = Instantiate(staffJobIcon);
            newstaffJobIcon.name = "StaffJob";
            staffJobList.Add(newstaffJobIcon.gameObject);
            newstaffJobIcon.transform.GetComponent<Image>().sprite = staffJob.GetIcon();
            newstaffJobIcon.transform.GetComponent<Tooltip>().tooltipText = staffJob.GetDescription();
            newstaffJobIcon.transform.SetParent(jobIconPanel.transform);
        }
        Destroy(jobIconPanel.transform.GetChild(0).gameObject);
    }
}
