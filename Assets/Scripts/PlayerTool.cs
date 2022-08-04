using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerToolClass
{
    NONE, MOWER, PLOUGH, SEED, WATER, PICK
}

public class PlayerTool
{
    public PlayerToolClass clazz;

    public PlayerTool(PlayerToolClass toolClass = PlayerToolClass.NONE)
    {
        clazz = toolClass;
    }

}

