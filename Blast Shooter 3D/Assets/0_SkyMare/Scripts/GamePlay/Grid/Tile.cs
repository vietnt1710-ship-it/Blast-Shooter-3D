using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
   
    public CellStatus status;
    public TileType type;

    public int row;
    public int col;

    [HideInInspector] public bool visited = false;



}
[Serializable]
public class ShooterData
{
    public int colorID;
    public Color color;
    public int bulletCount;
    public ShooterData(string af)
    {
        SplitColorID(af);
    }
    public void SplitColorID(string af)
    {
        string bulletCount = "" ;
        string colorID = "";
        GridParse.OnSplitBeAf(af, out bulletCount , out colorID);

        this.bulletCount = int.Parse(bulletCount);
        this.colorID = int.Parse(colorID);
        this.color = ColorID.ColorWithID(this.colorID).color;
    }
}

[Serializable]
public enum CellStatus
{
    empty,
    active,
    watting,
    wall,
    other

}

[Serializable]
public enum TileType
{
    normal,
    hiden,
    frezee,
    pipe,
    key,
    _lock,
    connect,
    latch,
    latchHead,
    latchBody,
}
