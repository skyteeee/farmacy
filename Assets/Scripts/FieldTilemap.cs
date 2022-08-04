using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ToolDestinations = System.Collections.Generic.Dictionary<PlayerToolClass, TileType>;



public class FieldTilemap : MonoBehaviour
{

    

    GridLayout gridLayout;
    Tilemap tilemap;
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

    void Start()
    {
        gridLayout = GetComponentInParent<GridLayout>();
        tilemap = GetComponent<Tilemap>();
        var tilemapSize = tilemap.size;
        var minPos = tilemap.cellBounds.min;
        cropsArea = new RectInt(minPos.x + cropsAreaXOffset, minPos.y + cropsAreaYOffset, tilemapSize.x + cropsAreaWidthOffset, tilemapSize.y + cropsAreaHeightOffset);
        CreateCropMap();
    }

    void CreateCropMap()
    {
        foreach (var position in cropsArea.allPositionsWithin)
        {
            fieldMap.Add(position, new LogicalTile(position));
        }
    }


    void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.tag == Tags.Player)
        {

            collision.gameObject.GetComponent<PlayerControlls>().tilemapScript = this;

            var pos1 = gridLayout.WorldToCell(collision.bounds.center);
            var pos2 = Utils.Vec3IntToVec2(pos1);

            if (tilemap.HasTile(pos1) && cropsArea.Contains(new Vector2Int(pos1.x, pos1.y)))
            {
                var tile1 = tilemap.GetTile<Tile>(pos1);
                selectedTile = tile1;
                Debug.Log($"{tile1.name}: {pos1.x}, {pos1.y}");

                if (selector == null)
                {
                    selector = Instantiate(tileSelectorPrefab, tilemap.GetCellCenterWorld(pos1), Quaternion.identity);
                    playerPos = pos2;
                }

                if (playerPos != pos2)
                {
                    playerPos = pos2;
                    selector.transform.Translate(-selector.transform.position + tilemap.GetCellCenterWorld(pos1));
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
            return tile.ApplyTool(tool);
        }
        return false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
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


public class LogicalTile
{
    public static Dictionary<TileType, ToolDestinations> tileToolMap = new Dictionary<TileType, ToolDestinations>()
        {
            {TileType.GRASSY, new ToolDestinations(){ {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.MOWED, new ToolDestinations(){ {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.WATER, TileType.GRASSY } } },
            {TileType.PLOUGHED, new ToolDestinations(){ {PlayerToolClass.SEED, TileType.SEEDED }, {PlayerToolClass.WATER, TileType.MOWED } } },
            {TileType.SEEDED, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.WATERED }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED } } },
            {TileType.WATERED, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWING1 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED } } },
            {TileType.GROWING1, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWING2 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING2, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWING3 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING3, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWING4 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING4, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWN }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWN, new ToolDestinations(){ {PlayerToolClass.PICK, TileType.GRASSY }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
        };

    public TileType type = TileType.GRASSY;
    public Vector2Int pos;

    public bool ApplyTool(PlayerTool tool)
    {

        var toolClass = tool.clazz;
        TileType destination;
        if (tileToolMap[type].TryGetValue(toolClass, out destination))
        {
            ChangeTypeTo(destination);
            return true;
        }
        return false;
    }

    private void ChangeTypeTo (TileType type)
    {
        this.type = type;
    }

    public LogicalTile(Vector2Int pos)
    {
        this.pos = pos;
    }
}

public enum TileType
{
    GRASSY, MOWED, PLOUGHED, SEEDED, WATERED, GROWING1, GROWING2, GROWING3, GROWING4, GROWN
}

