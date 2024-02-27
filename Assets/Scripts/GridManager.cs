using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject grass;
    public int mapSizeZ = 1;
    public int mapSizeX = 1;


    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < mapSizeZ; i++)
        {
            for (int j = 0; j < mapSizeX; j++)
            {
                Instantiate(grass, new Vector3(0+(i*20), 0, 0+(j*20)), transform.rotation);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
