using UnityEngine;

public class TerraformCollider : MonoBehaviour
{
    PlayerControl playerControl;

    void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Placed"))
        {
            playerControl.terrainCollided = true;
        }
    }
}
