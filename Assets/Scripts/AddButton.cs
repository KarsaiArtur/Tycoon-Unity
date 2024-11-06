using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AddButton : Button
{
    public Action action;
    private bool isPressed = false;
    override protected void Start()
    {
        base.Start();
        m_onDown.AddListener(Event);
    }

    void Event(){
        action.Invoke();
    }

    public UnityEvent m_onDown = new UnityEvent();
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        isPressed = true;
        StartCoroutine(Add());
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressed = false;
    }

    IEnumerator Add(){
        float i = 0.3f;
        int j = 0;
        while(isPressed){
            if(j >= 2 && i >= 0.05f){
                i -= 0.025f;
            }
            m_onDown.Invoke();
            j++;
            yield return new WaitForSeconds(i);
        }
    }
}
