using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlaceableButton : Button
{
    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        m_onDown.AddListener(SetPlaceableIndex);
    }

    void SetPlaceableIndex()
    {
        UIMenu.Instance.SetPlaceable(transform.GetSiblingIndex(), 0);
    }

    public UnityEvent m_onDown = new UnityEvent();
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!IsActive() || !IsInteractable()) { return; }
        m_onDown.Invoke();
    }
}