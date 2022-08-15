using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ToolDestinations = System.Collections.Generic.Dictionary<PlayerToolClass, TileType>;
using PositionTypeToTile = System.Collections.Generic.Dictionary<PositionType, System.Collections.Generic.List<string>>;
using TileTypeToCropTile = System.Collections.Generic.Dictionary<TileType, string>;
using CropStages = System.Collections.Generic.Dictionary<CropType, System.Collections.Generic.Dictionary<TileType, string>>;


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
            {TileType.SEEDED, new ToolDestinations(){ {PlayerToolClass.WATER, TileType.GROWING1 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED } } },
            {TileType.GROWING1, new ToolDestinations(){ {PlayerToolClass.TIME, TileType.GROWING2 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING2, new ToolDestinations(){ {PlayerToolClass.TIME, TileType.GROWING3 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING3, new ToolDestinations(){ {PlayerToolClass.TIME, TileType.GROWING4 }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWING4, new ToolDestinations(){ {PlayerToolClass.TIME, TileType.GROWN }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.GROWN, new ToolDestinations(){ {PlayerToolClass.PICK, TileType.PICKED }, {PlayerToolClass.PLOUGH, TileType.PLOUGHED }, {PlayerToolClass.MOWER, TileType.MOWED } } },
            {TileType.PICKED, new ToolDestinations(){ {PlayerToolClass.UNPICK, TileType.GROWN}, {PlayerToolClass.TIME, TileType.GRASSY } } }
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

    public static CropStages cropStages = new CropStages()
    {
        {CropType.Carrot, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_0"},
                {TileType.GROWING1, "crops_1"},
                {TileType.GROWING2, "crops_2"},
                {TileType.GROWING3, "crops_3"},
                {TileType.GROWING4, "crops_4"},
                {TileType.GROWN, "crops_5"},
                {TileType.PICKED, "item_carry_0"}
            }
        },
        {CropType.Tomato, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_6"},
                {TileType.GROWING1, "crops_7"},
                {TileType.GROWING2, "crops_8"},
                {TileType.GROWING3, "crops_9"},
                {TileType.GROWING4, "crops_10"},
                {TileType.GROWN, "crops_11"},
                {TileType.PICKED, "item_carry_1"}
            }
        },
        {CropType.Strawberry, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_12"},
                {TileType.GROWING1, "crops_13"},
                {TileType.GROWING2, "crops_14"},
                {TileType.GROWING3, "crops_15"},
                {TileType.GROWING4, "crops_16"},
                {TileType.GROWN, "crops_17"},
                {TileType.PICKED, "item_carry_2"}
            }
        },
        {CropType.Pumpkin, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_18"},
                {TileType.GROWING1, "crops_19"},
                {TileType.GROWING2, "crops_20"},
                {TileType.GROWING3, "crops_21"},
                {TileType.GROWING4, "crops_22"},
                {TileType.GROWN, "crops_23"},
                {TileType.PICKED, "item_carry_3"}
            }
        },
        {CropType.Corn, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_24"},
                {TileType.GROWING1, "crops_25"},
                {TileType.GROWING2, "crops_26"},
                {TileType.GROWING3, "crops_27"},
                {TileType.GROWING4, "crops_28"},
                {TileType.GROWN, "crops_29"},
                {TileType.PICKED, "item_carry_4"}
            }
        },
        {CropType.Potato, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_30"},
                {TileType.GROWING1, "crops_31"},
                {TileType.GROWING2, "crops_32"},
                {TileType.GROWING3, "crops_33"},
                {TileType.GROWING4, "crops_34"},
                {TileType.GROWN, "crops_35"},
                {TileType.PICKED, "item_carry_5"}
            }
        },
        {CropType.Watermelon, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_36"},
                {TileType.GROWING1, "crops_37"},
                {TileType.GROWING2, "crops_38"},
                {TileType.GROWING3, "crops_39"},
                {TileType.GROWING4, "crops_40"},
                {TileType.GROWN, "crops_41"},
                {TileType.PICKED, "item_carry_6"}
            }
        },
        {CropType.Radish, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_42"},
                {TileType.GROWING1, "crops_43"},
                {TileType.GROWING2, "crops_44"},
                {TileType.GROWING3, "crops_45"},
                {TileType.GROWING4, "crops_46"},
                {TileType.GROWN, "crops_47"},
                {TileType.PICKED, "item_carry_7"}
            }
        },
        {CropType.Lettuce, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_48"},
                {TileType.GROWING1, "crops_49"},
                {TileType.GROWING2, "crops_50"},
                {TileType.GROWING3, "crops_51"},
                {TileType.GROWING4, "crops_52"},
                {TileType.GROWN, "crops_53"},
                {TileType.PICKED, "item_carry_8"}
            }
        },
        {CropType.Wheat, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_54"},
                {TileType.GROWING1, "crops_55"},
                {TileType.GROWING2, "crops_56"},
                {TileType.GROWING3, "crops_57"},
                {TileType.GROWING4, "crops_58"},
                {TileType.GROWN, "crops_59"},
                {TileType.PICKED, "item_carry_9"}
            }
        },
        {CropType.Eggplant, new TileTypeToCropTile()
            {
                {TileType.SEEDED, "crops_60"},
                {TileType.GROWING1, "crops_61"},
                {TileType.GROWING2, "crops_62"},
                {TileType.GROWING3, "crops_63"},
                {TileType.GROWING4, "crops_64"},
                {TileType.GROWN, "crops_65"},
                {TileType.PICKED, "item_carry_10"}
            }
        },
    };

    public TileType type = TileType.GRASSY;
    public Vector2Int pos;
    public PositionType positionType;
    public CropType cropType = CropType.NONE;
    private int timePassed = 0;
    private int targetTime = 0;
    private static PlayerTool timeTool = new PlayerTool(PlayerToolClass.TIME);
    private static PlayerTool unpickTool = new PlayerTool(PlayerToolClass.UNPICK);

    public bool ApplyTool(PlayerTool tool, FieldTilemap tilemapScript)
    {
        
        var toolClass = tool.clazz;
        TileType destination;
        if (tileToolMap[type].TryGetValue(toolClass, out destination))
        {
            if (tool.clazz == PlayerToolClass.SEED && destination == TileType.SEEDED)
            {
                cropType = tool.cropType;
            }
            ChangeTypeTo(destination, tilemapScript);
            return true;
        }
        return false;
    }

    public void Update(FieldTilemap tilemapScript)
    {
        timePassed++;
        if (targetTime != 0 && timePassed == targetTime)
        {
            tilemapScript.RemoveUpdatedTile(this);
            ApplyTool(timeTool, tilemapScript);
        }
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

            case TileType.SEEDED:
            case TileType.GROWING1:
            case TileType.GROWING2:
            case TileType.GROWING3:
            case TileType.GROWING4:
                { 
                    StartCropTimer(tilemapScript);
                    Tile tile = Utils.LoadTile(cropStages[cropType][type]);
                    tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), tile);
                    break;
                }



            case TileType.GROWN:
                {
                    Tile tile = Utils.LoadTile(cropStages[cropType][type]);
                    tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), tile);
                    break;
                }

            case TileType.PICKED:
                {
                    bool added = tilemapScript.storage.TryAddNewCrop(cropType, tilemapScript);
                    if (added)
                    {
                        tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), null);
                        StartTimer(50, tilemapScript);
                    } else
                    {
                        ApplyTool(unpickTool, tilemapScript);
                    }
                    break;
                }


        }

        

    }

    void StartCropTimer(FieldTilemap tilemapScript)
    {
        StartTimer((int)cropType * 100 * 3 / 4, tilemapScript);
    }

    void StartTimer(int time, FieldTilemap tilemapScript)
    {
        timePassed = 0;
        targetTime = time;
        tilemapScript.AddUpdatedTile(this);
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

    public bool TryAddNewCrop(CropType cropType, FieldTilemap tilemapScript)
    {
        if (!IsFull())
        {
            var tile = storage[storageSequence[currentAmount]];
            tile.ChangeType(cropType, tilemapScript);
            currentAmount++;
            return true;
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

    public StorageTile (Vector2Int position, CropType cropType = CropType.NONE)
    {
        pos = position;
        this.cropType = cropType;
    }

    public void ChangeType (CropType type, FieldTilemap tilemapScript)
    {
        cropType = type;
        if (type != CropType.NONE)
        {
            tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), Utils.LoadTile(LogicalTile.cropStages[type][TileType.PICKED]));
        } else
        {
            tilemapScript.cropsMap.SetTile(Utils.Vec2IntToVec3(pos), null);
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

