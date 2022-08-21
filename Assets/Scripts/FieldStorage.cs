using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldStorage
{
    private Dictionary<Vector2Int, StorageTile> storage = new Dictionary<Vector2Int, StorageTile>(9);
    private int currentAmount = 0;
    private Vector2Int[] storageSequence = new Vector2Int[9];

    public FieldStorage(RectInt storageArea)
    {
        storageSequence[0] = new Vector2Int(storageArea.xMin + 1, storageArea.yMin + 1);
        storageSequence[1] = new Vector2Int(storageArea.xMin + 1, storageArea.yMin + 0);
        storageSequence[2] = new Vector2Int(storageArea.xMin + 0, storageArea.yMin + 0);
        storageSequence[3] = new Vector2Int(storageArea.xMin + 0, storageArea.yMin + 1);
        storageSequence[4] = new Vector2Int(storageArea.xMin + 0, storageArea.yMin + 2);
        storageSequence[5] = new Vector2Int(storageArea.xMin + 1, storageArea.yMin + 2);
        storageSequence[6] = new Vector2Int(storageArea.xMin + 2, storageArea.yMin + 2);
        storageSequence[7] = new Vector2Int(storageArea.xMin + 2, storageArea.yMin + 1);
        storageSequence[8] = new Vector2Int(storageArea.xMin + 2, storageArea.yMin + 0);

        foreach (var position in storageArea.allPositionsWithin)
        {
            storage.Add(position, new StorageTile(position));
        }
    }

    public CropType TryPickup(FieldTilemap tilemapScript, Vector2Int playerPos)
    {
        StorageTile tile;
        if (storage.TryGetValue(playerPos, out tile))
        {
            var type = tile.cropType;
            tile.ChangeType(CropType.NONE, tilemapScript);
            currentAmount--;
            return type;
        }
        return CropType.NONE;
    }

    public bool TryPutIntoStorage(CropType cropType, FieldTilemap tilemapScript, Vector2Int playerPos)
    {
        StorageTile tile;
        if (storage.TryGetValue(playerPos, out tile) && tile.cropType == CropType.NONE)
        {
            tile.ChangeType(cropType, tilemapScript);
            currentAmount++;
            return true;
        }

        return false;
    }

    public bool TryAddNewCrop(CropType cropType, FieldTilemap tilemapScript)
    {
        if (!IsFull())
        {
            for (int i = 0; i < storage.Count; i++)
            {
                var tile = storage[storageSequence[i]];
                if (tile.cropType == CropType.NONE)
                {
                    tile.ChangeType(cropType, tilemapScript);
                    currentAmount++;
                    return true;
                }
            }
            
        }

        return false;
    }

    public bool IsFull()
    {
        return currentAmount == storage.Count;
    }


}

public class StorageTile
{
    private Vector2Int pos;
    public CropType cropType;

    public StorageTile(Vector2Int position, CropType cropType = CropType.NONE)
    {
        pos = position;
        this.cropType = cropType;
    }

    public void ChangeType(CropType type, FieldTilemap tilemapScript)
    {
        cropType = type;
        if (type != CropType.NONE)
        {
            tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), Utils.LoadTile(LogicalTile.cropStages[type][TileType.PICKED]));
        }
        else
        {
            tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), null);
        }


    }

}
