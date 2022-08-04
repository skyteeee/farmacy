using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static Vector2Int Vec3IntToVec2 (Vector3Int vector3Int)
    {
        return new Vector2Int(vector3Int.x, vector3Int.y);
    }
}
