using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Nature : Placeable
{
    public List<GameObject> foligeVariants;
    public GameObject selectedVariantInstance;
    float height;
    NavMeshObstacle navMeshObstacle;

    public override void Awake()
    {
        var chosenPrefab = foligeVariants[Random.Range(0, foligeVariants.Count)];
        //var trunkPos = chosenPrefab.transform.Find("trunk").transform.position;
        selectedVariantInstance = Instantiate(chosenPrefab, transform.position, transform.rotation);
        selectedVariantInstance.transform.SetParent(transform);
        height = selectedVariantInstance.GetComponent<BoxCollider>().size.y;
        base.Awake();

        navMeshObstacle = selectedVariantInstance.GetComponent<NavMeshObstacle>();

        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<BoxCollider>())
        {
            var newCollider = gameObject.AddComponent<BoxCollider>();
            newCollider.size = collider.size;
            //newCollider.center = new Vector3(collider.center.x - trunkPos.x, collider.center.y - trunkPos.y + 0.5f, collider.center.z - trunkPos.z);
            newCollider.center = collider.center;
            Destroy(collider);
        }
        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<SphereCollider>())
        {
            var newCollider = gameObject.AddComponent<SphereCollider>();
            newCollider.radius = collider.radius;
            //newCollider.center = new Vector3(collider.center.x - trunkPos.x, collider.center.y - trunkPos.y + 0.5f, collider.center.z - trunkPos.z);
            newCollider.center = collider.center;
            Destroy(collider);
        }
        foreach (var collider in selectedVariantInstance.GetComponentsInChildren<CapsuleCollider>())
        {
            var newCollider = gameObject.AddComponent<CapsuleCollider>();
            newCollider.radius = collider.radius;
            newCollider.height = collider.height;
            newCollider.center = collider.center;
            Destroy(collider);
        }
    }

    public override void FinalPlace()
    {
        ChangeMaterial(0);
        navMeshObstacle.enabled = true;
        if (GridManager.instance.GetGrid(transform.position).isExhibit)
        {
            GridManager.instance.GetGrid(transform.position).exhibit.foliages.Add(this);
        }
    }

    public override void Place(Vector3 mouseHit)
    {
        base.Place(mouseHit); 
        transform.position = new Vector3(mouseHit.x, mouseHit.y + height / 2, mouseHit.z);
        if (!playerControl.canBePlaced)
        {
            ChangeMaterial(2);
        }
        else{
            ChangeMaterial(1);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (playerControl.placedTags.Contains(collision.collider.tag) && !tag.Equals("Placed"))
        {
            playerControl.canBePlaced = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (!tag.Equals("Placed"))
        {
            playerControl.canBePlaced = true;
            ChangeMaterial(1);
        }
    }

    public void Remove()
    {
        var tempGrid = GridManager.instance.GetGrid(transform.position);
        if (tempGrid.isExhibit)
        {
            tempGrid.exhibit.RemoveNature(this);
        }
        Destroy(gameObject);
    }
}
