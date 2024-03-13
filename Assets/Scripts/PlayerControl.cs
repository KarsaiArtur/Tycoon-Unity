using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float cameraSpeed = 10;
    public float zoomSpeed = 10;
    public Camera GameCamera;
    public Placeable m_Selected = null;
    public int maxZoom = 5;
    public int minZoom = 20;
    public int maxLeft = 50;
    public int maxRight = 50;
    public int moveSpeed = 1;
    private float angle;
    private int index;
    private bool isNew = false;
    public float objectTimesRotated = 0;
    public int fenceIndex = 0;
    public bool canBePlaced = true;

    // Start is called before the first frame update
    void Start()
    {
        angle = 90 - transform.eulerAngles.y;
    }

    Vector3 placed;

    // Update is called once per frame
    void Update()
    {
        Move();
        Zoom();
        RotateObject();




        if (Input.GetMouseButtonDown(1) && isNew)
        {
            Destroy(m_Selected.gameObject);
            m_Selected = null;
            isNew = false;
            objectTimesRotated = 0;
        }
        else if (Input.GetMouseButtonDown(0) && m_Selected != null)
        {
            if (canBePlaced)
            {
                m_Selected.gameObject.tag = "Placed";
                m_Selected.ChangeMaterial(0);

                if (isNew)
                {
                    Spawn(index);
                    for (int i = 0; i < objectTimesRotated; i++)
                    {
                        m_Selected.RotateY(90);
                    }
                }
                else
                {
                    m_Selected = null;
                    objectTimesRotated = 0;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
        if (m_Selected != null)
        {
            var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    m_Selected.Place(hit);
                    break;
                }
            }
        }
    }

    void RotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R) && m_Selected != null)
        {
            m_Selected.RotateY(90);
            if (isNew)
                objectTimesRotated++;
        }
    }

    public float Round(float number)
    {
        return Mathf.Floor(number) + 0.5f;
    }

    void Move()
    {
        Vector2 move = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        transform.position = transform.position + new Vector3(move.x * (float)Math.Cos(angle * 0.0174532925) + move.y * (float)Math.Sin(angle * 0.0174532925), 0, move.x * (float)Math.Sin(angle * 0.0174532925) - move.y * (float)Math.Cos(angle * 0.0174532925)) * cameraSpeed * Time.deltaTime;      

        MovementSpeedChange();
    }

    void MovementSpeedChange()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            cameraSpeed *= 2;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            cameraSpeed /= 2;
        }
    }

    void Zoom()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");

        if (transform.position.y < maxZoom && zoom > 0)
        {
            zoom = 0;
        }
        if (transform.position.y > minZoom && zoom < 0)
        {
            zoom = 0;
        }
        transform.Translate(Vector3.forward * zoom * zoomSpeed);
    }

    public void HandleSelection()
    {
        var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Placed"))
            {
                var building = hit.collider.GetComponentInParent<Placeable>();
                m_Selected = building;
                m_Selected.gameObject.tag = "Untagged";
            }
        }
    }

    public Placeable[] prefabs;
    public Placeable[] fences;

    public void Spawn(int i)
    {
        Debug.Log(index);
        isNew = true;
        index = i;
        Debug.Log(index);
        m_Selected = Instantiate(prefabs[index], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0,0,0,0));
        if(m_Selected.gameObject.CompareTag("Fence") && fenceIndex != 0)
            ChangeFence(fenceIndex);
    }

    public void SpawnFence(int i)
    {
        isNew = true;
        fenceIndex = i;
        m_Selected = Instantiate(fences[fenceIndex], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0, 0, 0, 0));
    }

    public void ChangeFence(int index)
    {
        fenceIndex = index;
        Destroy(m_Selected.gameObject);
        SpawnFence(index);
        for (int i = 0; i < objectTimesRotated; i++)
        {
            m_Selected.RotateY(90);
        }
    }
}
