using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ExtraMenu : MonoBehaviour
{
    public abstract void Destroy();
    public abstract void SetActive(bool isVisible);
    public abstract void SetPosition(Vector3 position);
    public abstract string GetName();

    public abstract void UpdateWindow();
}
