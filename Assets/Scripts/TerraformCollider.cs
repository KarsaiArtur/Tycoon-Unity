using System.Linq;
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
        if (playerControl.placedTags.Contains(other.tag) && other.tag != "ZooFence")
        {
            playerControl.terrainCollided = true;
        }
    }
}
