using System;
using UnityEngine;

/////Saveable Attributes, DONT DELETE
//////string _id;Vector3 position;int selectedPrefabId;int placeablePrice;string tag;Quaternion rotation//////////
//////SERIALIZABLE:YES/

public class Path : Placeable, Saveable
{
    float curOffsetX = 0.5f;
    float curOffsetZ = 0.25f;
    public static float offsetDefault = 0.05f;
    public Path otherVariant;

    public void CheckTerrain(Vector3 pos)
    {
        float yOffset = 0f;
        Vector3 position1 = new Vector3(pos.x + curOffsetX, pos.y + 50f, pos.z + curOffsetZ);
        Vector3 position2 = new Vector3(pos.x - curOffsetX, pos.y + 50f, pos.z + curOffsetZ);
        Vector3 yPos = new Vector3(pos.x, pos.y + 50f, pos.z);

        RaycastHit[] hits1 = Physics.RaycastAll(position1, -transform.up);
        RaycastHit[] hits2 = Physics.RaycastAll(position2, -transform.up);
        RaycastHit[] hits3 = Physics.RaycastAll(yPos, -transform.up);

        foreach (RaycastHit hit in hits3)
        {
            if (playerControl.placedTags.Contains(hit.collider.tag))
            {
                playerControl.canBePlaced = false;
                yOffset = offsetDefault;
            }
            if (hit.collider.CompareTag("Terrain"))
            {
                yPos = hit.point;
            }
        }
        yPos.y += yOffset;

        foreach (RaycastHit hit in hits1)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                position1 = hit.point;
            }
        }

        foreach (RaycastHit hit in hits2)
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                position2 = hit.point;
            }
        }

        float yDifference = position1.y - position2.y;
        yDifference = RoundToDecimal(yDifference, 2);
        if (gameObject.CompareTag("Incline") && yDifference == 0 && position1.y % 0.5 == 0)
        {
            playerControl.ChangePath(this, pos, 0);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == 0 && (position1.y % 1 == 0.125f || position1.y % 1 == 0.625f || position1.y % 1 == -0.375f || position1.y % 1 == -0.875f))
        {
            playerControl.ChangePath(this, pos, 90);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == 0 && (position1.y % 1 == 0.375f || position1.y % 1 == 0.875f || position1.y % 1 == -0.125f || position1.y % 1 == -0.625f))
        {
            playerControl.ChangePath(this, pos, 270);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == -0.5f && position1.y % 0.5 == 0)
        {
            playerControl.ChangePath(this, pos, 180);
        }
        else if (gameObject.CompareTag("Flat") && yDifference == 0.5f && position1.y % 0.5 == 0)
        {
            playerControl.ChangePath(this, pos, 0);
        }
        else if (Math.Abs(yDifference) == 0.12f || Math.Abs(yDifference) == 0.38f || (Math.Abs(yDifference) == 0.5f && position1.y % 0.5 != 0))
        {
            if (gameObject.CompareTag("Incline"))
            {
                playerControl.ChangePath(this, pos, 0);
            }
            playerControl.canBePlaced = false;
            yPos.y += 0.5f;
        }
        transform.position = new Vector3(playerControl.Round(pos.x), yPos.y + offsetDefault, playerControl.Round(pos.z));
    }

    public override void FinalPlace()
    {
        PathManager.instance.AddList(this);
    }

    float RoundToDecimal(float number, int dec)
    {
        float newNumber = number * Mathf.Pow(10, dec);
        newNumber = Mathf.Round(newNumber);
        return newNumber / Mathf.Pow(10, dec);
    }

    public override void SetTag(string newTag)
    {
        base.SetTag(newTag);
        foreach (var transform in GetComponentsInChildren<Transform>())
        {
            transform.tag = newTag;
        }
    }

    public override void Remove()
    {
        if (CompareTag("Placed Path"))
        {
            PathManager.instance.pathList.Remove(this);
            Grid tempGrid = GridManager.instance.GetGrid(transform.position);
            tempGrid.isPath = false;
            //foreach (var exhibit in GridManager.instance.exhibits)
            //{
            //    exhibit.RemovePath(this);
            //    exhibit.DecideIfReachable();
            //}
            //foreach (var building in GridManager.instance.buildings)
            //{
            //    building.RemovePath(this);
            //    building.DecideIfReachable();
            //}
            //foreach (var bench in GridManager.instance.benches)
            //{
            //    bench.RemovePath(this);
            //    bench.DecideIfReachable();
            //}
            foreach (var visitable in VisitableManager.instance.visitableList)
            {
                visitable.RemovePath(this);
                visitable.DecideIfReachable();
            }
            Destroy(gameObject);
        }
    }

    public void LoadHelper()
    {
        playerControl.ReloadGuestNavMesh();
    }
///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

    [Serializable]
    public class PathData
    {
        public string _id;
        public Vector3 position;
        public int selectedPrefabId;
        public int placeablePrice;
        public string tag;
        public Quaternion rotation;

        public PathData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, int placeablePriceParam, string tagParam, Quaternion rotationParam)
        {
           _id = _idParam;
           position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           rotation = rotationParam;
        }
    }

    PathData data; 
    
    public string DataToJson(){
        PathData data = new PathData(_id, transform.position, selectedPrefabId, placeablePrice, tag, transform.rotation);
        return JsonUtility.ToJson(data);
    }
    
    public void FromJson(string json){
        data = JsonUtility.FromJson<PathData>(json);
        SetData(data._id, data.position, data.selectedPrefabId, data.placeablePrice, data.tag, data.rotation);
    }
    
    public string GetFileName(){
        return "Path.json";
    }
    
    void SetData(string _idParam, Vector3 positionParam, int selectedPrefabIdParam, int placeablePriceParam, string tagParam, Quaternion rotationParam){ 
        
           _id = _idParam;
           transform.position = positionParam;
           selectedPrefabId = selectedPrefabIdParam;
           placeablePrice = placeablePriceParam;
           tag = tagParam;
           transform.rotation = rotationParam;
    }
    
    public PathData ToData(){
         return new PathData(_id, transform.position, selectedPrefabId, placeablePrice, tag, transform.rotation);
    }
    
    public void FromData(PathData data){
        
           _id = data._id;
           transform.position = data.position;
           selectedPrefabId = data.selectedPrefabId;
           placeablePrice = data.placeablePrice;
           tag = data.tag;
           transform.rotation = data.rotation;
    }
}
