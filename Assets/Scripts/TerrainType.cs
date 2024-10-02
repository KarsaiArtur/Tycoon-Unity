using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Grass,
    Forest,
    Savannah,
    Snow,
    Sand,
    Dirt,
    Water,
    Stone,
    Ice,
    Rainforest,
    Mixed
}

static class TerrainTypeMethods
{
    public static float GetPrice(this TerrainType s1)
    {
        switch (s1)
        {
            case TerrainType.Grass:
                return 20;
            case TerrainType.Forest:
                return 40;
            case TerrainType.Savannah:
                return 56;
            case TerrainType.Snow:
                return 60;
            case TerrainType.Sand:
                return 48;
            case TerrainType.Dirt:
                return 44;
            case TerrainType.Water:
                return 100;
            case TerrainType.Stone:
                return 52;
            case TerrainType.Ice:
                return 80;
            case TerrainType.Rainforest:
                return 64;
            default:
                return 0;
        }
    }
}
