using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using PositionTypeToTile = System.Collections.Generic.Dictionary<PositionType, System.Collections.Generic.List<string>>;

public class Utils
{ 

    public static Vector2Int Vec3IntToVec2 (Vector3Int vector3Int)
    {
        return new Vector2Int(vector3Int.x, vector3Int.y);
    }

    public static Vector3Int Vec2IntToVec3 (Vector2Int vector2Int, int z = 0)
    {
        return new Vector3Int(vector2Int.x, vector2Int.y, z);
    }

    public static Tile LoadTile (string name)
    {
        return Resources.Load<Tile>("Sprites/LandTexTiles/" + name);
    }

    public static Tile LoadTile (PositionTypeToTile positionTypeToTile, PositionType positionType)
    {

        if (positionTypeToTile == null)
        {
            return null;
        }

        var namesList = positionTypeToTile[positionType];
        if (namesList.Count == 1)
        {
            return LoadTile(namesList[0]);
        }
        int index = Random.Range(0, namesList.Count);
        
        return LoadTile(namesList[index]);
    }

}
