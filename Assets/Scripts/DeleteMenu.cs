using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteMenu : ExtraMenu
{
    public PlayerControl playerControl;
    public GameObject icon;
    public GameObject deleteIconPrefab;

    public void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        icon = Instantiate(deleteIconPrefab, Vector3.zero, Quaternion.identity);
        icon.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        playerControl.ChangeDelete();
    }
    
    public void Update(){
        if(icon != null){
            icon.transform.position = new Vector2(Input.mousePosition.x + 70, Input.mousePosition.y - 45);
        }
    }

    public override void Destroy()
    {
        if(gameObject.active){
            playerControl.ChangeDelete();
        }

        Destroy(icon.gameObject);
        Destroy(gameObject);
    }
    public override void SetActive(bool isVisible)
    {
        if(gameObject.active != isVisible)
        {
            playerControl.ChangeDelete();
        }
        icon.gameObject.SetActive(isVisible);
        gameObject.SetActive(isVisible);
    }

    public override void SetPosition(Vector3 position)
    {
        transform.SetParent(playerControl.canvas.transform.Find("Extra Menu").transform);
        transform.localPosition = position;
    }

    public override string GetName()
    {
        return "delete";
    }

    public override void UpdateWindow()
    {
        
    }
}
