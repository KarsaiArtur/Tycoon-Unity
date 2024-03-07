using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControll : MonoBehaviour
{
    public float cameraSpeed = 10;
    public float zoomSpeed = 10;
    public Camera GameCamera;
    private Grass m_Selected = null;
    public int maxZoom = 5;
    public int minZoom = 20;
    public int maxLeft = 50;
    public int maxRight = 50;
    public int moveSpeed = 1;
    private float angle;

    // Start is called before the first frame update
    void Start()
    {
        angle = 90 - transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Zoom();
        RotateObject();




        if (Input.GetMouseButtonDown(0) && m_Selected != null)
        {
            m_Selected = null;
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
                    m_Selected.transform.position = new Vector3(Round(hit.point.x), hit.point.y+0.5f, Round(hit.point.z));
                    Debug.Log(m_Selected.transform.position+ " selected");
                    Debug.Log(new Vector3(Round(hit.point.x), hit.point.y + 0.5f, Round(hit.point.z)) + " hit");
                }
            }
        }
    }

    void RotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R) && m_Selected != null)
        {
            m_Selected.RotateY(90);
        }
    }

    float Round(float number)
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
            var grass = hit.collider.GetComponentInParent<Grass>();
            m_Selected = grass;
        }
    }

    public Grass[] prefabs;

    public void Spawn(int index)
    {
        m_Selected = Instantiate(prefabs[index], new Vector3(Round(Input.mousePosition.x), 5, Round(Input.mousePosition.z)), new Quaternion(0,0,0,0));
    }
}
