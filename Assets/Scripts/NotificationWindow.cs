using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NotificationWindow : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    public Button closeButton;
    PlayerControl playerControl;

    private void Awake()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        closeButton.onClick.AddListener(() => Destroy(gameObject));
    }

    public void SetText(string text)
    {
        notificationText.text = text;
    }


    public void SetPosition(Vector3 position)
    {
        transform.SetParent(playerControl.canvas.transform.Find("Notification").transform);
        transform.localPosition = position;
    }
}
