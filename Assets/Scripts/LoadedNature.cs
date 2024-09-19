using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class LoadedNature : Nature
{
    public override void Awake(){
        playerControl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PlayerControl>();
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        currentPlacingPrice = GameObject.Find("Placing Price").GetComponent<TextMeshProUGUI>();

        if (placeableName != null && placeableName.Equals(""))
        {
            placeableName = name.Remove(name.Length - "(Clone)".Length);
        }

        defaultMaterials = new List<(int, Material)>();
        renderers = new List<Renderer>(GetComponentsInChildren<MeshRenderer>());
        List<Renderer> renderers2 = new List<Renderer>(GetComponentsInChildren<SkinnedMeshRenderer>());

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                defaultMaterials.Add((renderer.GetHashCode(), material));
            }
        }
        foreach (var renderer in renderers2)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                defaultMaterials.Add((renderer.GetHashCode(), material));
            }
        }

        renderers2.ForEach(r => renderers.Add(r));

        navMeshObstacle = GetComponent<NavMeshObstacle>();
        NatureManager.instance.natures.Add(this);
        transform.SetParent(NatureManager.instance.transform);
        navMeshObstacle.enabled = true;
    }
}
