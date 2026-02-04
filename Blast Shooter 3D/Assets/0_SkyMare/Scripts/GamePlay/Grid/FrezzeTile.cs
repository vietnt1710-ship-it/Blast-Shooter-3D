using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrezzeTile : ObjectTile
{
    public int invativeCount = 3;
    public override void Init()
    {
       // jar.ApplyFrezzWaittingJar(data, row, invativeCount);
    }

    public void UnFezze()
    {
        if (invativeCount <= 0) return;

        invativeCount--;
        //jar.frezzeText.text = invativeCount.ToString();

        //if (invativeCount == 0)
        //{
        //    jar.frezzeText.gameObject.SetActive(false);
        //    type = TileType.normal;
        //    jar.UnFrezze();
        //}

    }
    public void ForceUnFrezze()
    {
        int index = LevelManager.I.m_gridManager.frezzeTiles.IndexOf(this);
        LevelManager.I.m_gridManager.frezzeTiles.RemoveAt(index);

        type = TileType.normal;
       // jar.UnFrezze();
    }

    public override void Active()
    {
        //jar.ActiveFrezze();
    }
    public override void ClickProcess(Slot item)
    {
       // StackItem item1 = new StackItem();
        //jar.TransJar(item);
    }
}
