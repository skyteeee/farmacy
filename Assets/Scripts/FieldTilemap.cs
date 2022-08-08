using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ToolDestinations = System.Collections.Generic.Dictionary<PlayerToolClass, TileType>;
using PositionTypeToTile = System.Collections.Generic.Dictionary<PositionType, System.Collections.Generic.List<string>>;



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

    void Start()
    {
        gridLayout = GetComponentInParent<GridLayout>();
        groundMap = GetComponent<Tilemap>();
        var tilemapSize = groundMap.size;
        var minPos = groundMap.cellBounds.min;
        cropsArea = new RectInt(minPos.x + cropsAreaXOffset, minPos.y + cropsAreaYOffset, tilemapSize.x + cropsAreaWidthOffset, tilemapSize.y + cropsAreaHeightOffset);
        CreateCropMap();
    }

    private void FixedUpdate()
    {
        foreach (var item in tilesToUpdate)
        {
            item.Update();
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

            if (groundMap.HasTile(pos1) && cropsArea.Contains(new Vector2Int(pos1.x, pos1.y)))
            {
                var tile1 = groundMap.GetTile<Tile>(pos1);
                selectedTile = tile1;
                Debug.Log($"{tile1.name}: {pos1.x}, {pos1.y}, {pos1.z}");

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

    public static PositionTypeToTile soilTilePaths = new PositionTypeToTile()
    {
        {PositionType.TopLeft, new List<string>(){"soil_24"} },
        {PositionType.Top, new List<string>(){"soil_16", "soil_17", "soil_18"} },
        {PositionType.Left, new List<string>(){"soil_3", "soil_11"} },
        {PositionType.TopRight, new List<string>(){"soil_26"} },
        {PositionType.Right, new List<string>(){"soil_4", "soil_12"} },
        {PositionType.BottomRight, new List<string>(){"soil_42"} },
        {PositionType.BottomLeft, new List<string>(){"soil_40"} },
        {PositionType.Bottom, new List<string>(){"soil_5", "soil_6"} },
        {PositionType.Center, new List<string>(){"soil_33"} }
    };

    public static PositionTypeToTile grassyCropsPaths = new PositionTypeToTile()
    {
        {PositionType.Center, new List<string>(){"landTex_5", "landTex_6", "landTex_7", "landTex_8", "landTex_9", "landTex_10", "landTex_11"} }
    };

    public TileType type = TileType.GRASSY;
    public Vector2Int pos;
    public PositionType positionType;

    public bool ApplyTool(PlayerTool tool, FieldTilemap tilemapScript)
    {

        var toolClass = tool.clazz;
        TileType destination;
        if (tileToolMap[type].TryGetValue(toolClass, out destination))
        {
            ChangeTypeTo(destination, tilemapScript);
            return true;
        }
        return false;
    }

    public void Update()
    {

    }

    private void ChangeTypeTo (TileType type, FieldTilemap tilemapScript)
    {
        this.type = type;

        switch (type)
        {
            case TileType.MOWED:
                {
                    Tile tile = Utils.LoadTile("landTex_48");                   

                    tilemapScript.soilMap.SetTile(Utils.Vec2IntToVec3(pos), tile);
                    tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), null);
                    SetTilesAround(positionType, tilemapScript.soilMap, null);

                    
                    break;
                }

            case TileType.PLOUGHED:
                {
                    tilemapScript.soilMap.SetTile(Utils.Vec2IntToVec3(pos), Utils.LoadTile(soilTilePaths[PositionType.Center][0]));
                    tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), null);

                    SetTilesAround(positionType, tilemapScript.soilMap, soilTilePaths);


                    break;
                }

            case TileType.GRASSY:
                {
                    Tile tile = Utils.LoadTile("landTex_60");
                    SetTilesAround(positionType, tilemapScript.soilMap, null);
                    tilemapScript.soilMap.SetTile(Utils.Vec2IntToVec3(pos), tile);
                    tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), Utils.LoadTile(grassyCropsPaths, PositionType.Center));
                    break;
                }

        }

        

    }

    void SetTilesAround(PositionType positionType, Tilemap tilemap, PositionTypeToTile positionTypeToTile)
    {
        switch (positionType)
        {
            case PositionType.TopLeft:
                {
                    tilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Left));
                    tilemap.SetTile(new Vector3Int(pos.x - 1, pos.y + 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.TopLeft));
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Top));
                    break;
                }
            case PositionType.Top:
                {
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Top));
                    break;
                }
            case PositionType.Left:
                {
                    tilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Left));
                    break;
                }
            case PositionType.TopRight:
                {
                    tilemap.SetTile(new Vector3Int(pos.x + 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Right));
                    tilemap.SetTile(new Vector3Int(pos.x + 1, pos.y + 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.TopRight));
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y + 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Top));
                    break;
                }
            case PositionType.Right:
                {
                    tilemap.SetTile(new Vector3Int(pos.x + 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Right));
                    break;
                }
            case PositionType.BottomRight:
                {
                    tilemap.SetTile(new Vector3Int(pos.x + 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Right));
                    tilemap.SetTile(new Vector3Int(pos.x + 1, pos.y - 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.BottomRight));
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y - 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Bottom));
                    break;
                }
            case PositionType.Bottom:
                {
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y - 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Bottom));
                    break;
                }
            case PositionType.BottomLeft:
                {
                    tilemap.SetTile(new Vector3Int(pos.x - 1, pos.y, 0), Utils.LoadTile(positionTypeToTile, PositionType.Left));
                    tilemap.SetTile(new Vector3Int(pos.x - 1, pos.y - 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.BottomLeft));
                    tilemap.SetTile(new Vector3Int(pos.x, pos.y - 1, 0), Utils.LoadTile(positionTypeToTile, PositionType.Bottom));
                    break;
                }

        }
    }


    public LogicalTile(Vector2Int pos, PositionType positionType, TileType tileType = TileType.GRASSY)
    {
        this.pos = pos;
        this.type = tileType;
        this.positionType = positionType;
    }
}

public enum TileType
{
    GRASSY, MOWED, PLOUGHED, SEEDED, WATERED, GROWING1, GROWING2, GROWING3, GROWING4, GROWN
}

public enum PositionType
{
    TopRight = 6, Top = 4, TopLeft = 5, Left = 1, Center = 0, Right = 2, BottomRight = 10, Bottom = 8, BottomLeft = 9
}

