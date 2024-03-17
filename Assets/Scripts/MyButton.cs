using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyButton : Button
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(SetPlaceableIndex);
    }



    void SetPlaceableIndex()
    {
        UIMenu.Instance.SetPlaceable(transform.GetSiblingIndex(), 0);
    }
}
