using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerToolClass
{
    NONE, MOWER, PLOUGH, SEED, WATER, PICK, UNPICK, TIME
}

public class PlayerTool
{
    public PlayerToolClass clazz;
    public CropType cropType = CropType.NONE;
    public PlayerTool(PlayerToolClass toolClass = PlayerToolClass.NONE, CropType cropType = CropType.NONE)
    {
        clazz = toolClass;
        this.cropType = cropType;
    }

}

