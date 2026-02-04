using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockTile : ObjectTile
{
    public KeyTile target;
    public int lockID;
    public override void Init()
    {
        //if (status != CellStatus.empty)
        //{
        //    jar.ApplyNormalWaittingJar(data, row);
        //}
        //else
        //{
        //    jar.gameObject.SetActive(false);
        //}

    }
    public override void Active()
    {
        //jar.ActiveNormal();
    }
    public override void ClickProcess(Slot item)
    {
        //jar.TransJar(item);
    }
}
