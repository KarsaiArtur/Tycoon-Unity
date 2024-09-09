using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Saveable
{
    public string DataToJson();
    public void FromJson(string json);
    public string GetFileName();
}
