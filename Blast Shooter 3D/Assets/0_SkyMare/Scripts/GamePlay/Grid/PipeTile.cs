using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PipeTile : Tile
{
    public List<ShooterData> dataWaitings = new List<ShooterData>();
    Tile target;
    public int targetRow;
    public int targetCol;

    public Transform gara;
    public TMP_Text countInGara;
    //public SandJar sandJar;


    public void InitPipe(string be, string af)
    {
        List<string> ims = GridParse.OnSplitPipeIDs(af);
        for (int i = 0; i < ims.Count; i++)
        {
            ShooterData data = new ShooterData(ims[i]);
            dataWaitings.Add(data);
        }
        countInGara.text = dataWaitings.Count.ToString();

        int dirIdx = int.Parse(be[1].ToString());
        targetRow = row + GridManager.DIRS[dirIdx - 1].dr;
        targetCol = col + GridManager.DIRS[dirIdx - 1].dc;

        switch (dirIdx)
        {
            case 1: gara.transform.localEulerAngles = new Vector3(90, 0, 0); break;
            case 2: gara.transform.localEulerAngles = new Vector3(90, 0, 180); break;
            case 3: gara.transform.localEulerAngles = new Vector3(90, 0, -90); break;
            case 4: gara.transform.localEulerAngles = new Vector3(90, 0, 90); break;
        }
    }
    public void SpawmJar()
    {
        //if (target == null)
        //{
        //    if (MazeManager.I.grid[targetRow, targetCol] is ObjectTile o)
        //    {
        //        target = o;
        //    }
        //    else
        //    {
        //        Debug.LogError("Gara Error");
        //        return;
        //    }
        //}

        //if (target.status == CellStatus.empty && dataWaitings.Count > 0)
        //{
        //    var newData = dataWaitings[0];
        //    dataWaitings.RemoveAt(0);

        //    //Spawm new object tile
        //    ObjectTile t = target.AddComponent<NormalPipeTile>();
        //    MazeManager.I.grid[target.row, target.col] = t;

        //    t.status = CellStatus.watting;
        //    t.row = target.row;
        //    t.col = target.col;
        //    t.data = newData;

        //    var jar = Instantiate(sandJar, transform.position, sandJar.transform.rotation);
        //    jar.transform.SetParent(t.transform);

        //    // jar.jarAnimation.quad = t.transform.GetChild(0).transform.GetChild(2).gameObject;

        //    jar.jarAnimation.ScaleToNormal(t.jarPos, 0.5f, () =>
        //    {
        //        countInGara.text = dataWaitings.Count.ToString();
        //        jar.ActivePipe();
        //    });
        //    jar.gameObject.SetActive(true);

        //    jar.ApplyNormalWaittingJar(newData, t.row);
        //    t.jar = jar;
        //    Destroy(target);
        //    target = t;
        //}
    }
}
