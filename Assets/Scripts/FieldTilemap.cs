using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class FieldTilemap : MonoBehaviour
{

    

    GridLayout gridLayout;
    public Tilemap groundMap;
    public Tilemap soilMap;
    public Tilemap cropsMap;
    public GameObject tileSelectorPrefab;
    private Vector2Int playerPos;
    private GameObject selector;
    private Tile selectedTile;
    private RectInt cropsArea;
    private const int cropsAreaXOffset = 1;
    private const int cropsAreaYOffset = 3;
    private const int cropsAreaHeightOffset = -4;
    private const int cropsAreaWidthOffset = -2;
    private Dictionary<Vector2Int, LogicalTile> fieldMap = new Dictionary<Vector2Int, LogicalTile>();
    private List<LogicalTile> tilesToUpdate = new List<LogicalTile>();
    public FieldStorage storage;
    private RectInt storageArea;

    void Start()
    {
        gridLayout = GetComponentInParent<GridLayout>();
        groundMap = GetComponent<Tilemap>();
        var tilemapSize = groundMap.size;
        var minPos = groundMap.cellBounds.min;
        var maxPos = groundMap.cellBounds.max;
        cropsArea = new RectInt(minPos.x + cropsAreaXOffset, minPos.y + cropsAreaYOffset, tilemapSize.x + cropsAreaWidthOffset, tilemapSize.y + cropsAreaHeightOffset);
        storageArea = new RectInt(maxPos.x - 3, minPos.y, 3, 3);
        storage = new FieldStorage(storageArea);
        CreateCropMap();
    }

    private void FixedUpdate()
    {
        if (tilesToUpdate.Count != 0)
        {
            var updateList = new List<LogicalTile>(tilesToUpdate);
            foreach (var item in updateList)
            {
                item.Update(this);
            }
        }
        
    }

    public void AddUpdatedTile(LogicalTile tile)
    {
        tilesToUpdate.Remove(tile);
        tilesToUpdate.Add(tile);
    }

    public void RemoveUpdatedTile(LogicalTile tile)
    {
        tilesToUpdate.Remove(tile);
    }


    void CreateCropMap()
    {
        var xMin = cropsArea.xMin;
        var xMax = cropsArea.xMax - 1;
        var yMin = cropsArea.yMin;
        var yMax = cropsArea.yMax - 1;

        foreach (var position in cropsArea.allPositionsWithin)
        {

            int posTypeVal = 0;
            if (position.x == xMin)
            {
                posTypeVal += ((int)PositionType.Left);
            }
            if (position.x == xMax)
            {
                posTypeVal += ((int)PositionType.Right);
            }
            if (position.y == yMin)
            {
                posTypeVal += ((int)PositionType.Bottom);
            }
            if (position.y == yMax)
            {
                posTypeVal += ((int)PositionType.Top);
            }


            fieldMap.Add(position, new LogicalTile(position, (PositionType) posTypeVal));
        }
    }


    void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.tag == Tags.Player)
        {

            collision.gameObject.GetComponent<PlayerControlls>().tilemapScript = this;

            var pos1 = gridLayout.WorldToCell(collision.bounds.center);
            var pos2 = Utils.Vec3IntToVec2(pos1);

            if (groundMap.HasTile(pos1) && (cropsArea.Contains(pos2) || storageArea.Contains(pos2)))
            {
                var tile1 = groundMap.GetTile<Tile>(pos1);
                selectedTile = tile1;

                if (selector == null)
                {
                    selector = Instantiate(tileSelectorPrefab, groundMap.GetCellCenterWorld(pos1), Quaternion.identity);
                    playerPos = pos2;
                }

                if (playerPos != pos2)
                {
                    playerPos = pos2;
                    selector.transform.Translate(-selector.transform.position + groundMap.GetCellCenterWorld(pos1));
                }

            }
            else
            {
                DestroySelector();
            }

        }
    }

    public bool ApplyToolToTile(PlayerTool tool)
    {
        LogicalTile tile = null;
        if (fieldMap.TryGetValue(playerPos, out tile))
        {
            return tile.ApplyTool(tool, this);
        }
        return false;
    }

    public CropType TryPickup ()
    {

        return storage.TryPickup(this, playerPos);


    }

    public bool TryPutIntoStorage(CropType cropType)
    {
        return storage.TryPutIntoStorage(cropType, this, playerPos);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerControlls controlls;
        if (collision.TryGetComponent<PlayerControlls>(out controlls))
        {
            controlls.tilemapScript = null;
        }

        DestroySelector();
    }

    private void DestroySelector ()
    {
        if (selector != null)
        {
            Destroy(selector);
            selector = null;
            playerPos = Consts.ImpossibleTileCoords;
        }
    }

    



}






public enum TileType
{
    GRASSY, MOWED, PLOUGHED, SEEDED, WATERED, GROWING1, GROWING2, GROWING3, GROWING4, GROWN, PICKED
}

public enum CropType
{
    NONE,
    Radish,
    Lettuce,
    Carrot,
    Potato,
    Tomato,
    Eggplant,
    Strawberry,
    Corn,
    Pumpkin,
    Watermelon,
    Wheat
}

public enum PositionType
{
    TopRight = 6, Top = 4, TopLeft = 5, Left = 1, Center = 0, Right = 2, BottomRight = 10, Bottom = 8, BottomLeft = 9
}

