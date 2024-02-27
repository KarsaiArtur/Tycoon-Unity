using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int[] rotations = { 0, 90, 180, 270 };
        transform.Rotate(0, rotations[Random.Range(0, 4)], 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
