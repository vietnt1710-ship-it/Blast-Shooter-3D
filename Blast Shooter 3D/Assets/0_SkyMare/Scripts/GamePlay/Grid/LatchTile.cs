using DG.Tweening;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatchTile : ObjectTile
{
    public bool isHead = false;
    Tile target;

    public List<LatchTile> bodies = new List<LatchTile>();
    public int ID;

    int targetRow;
    int targetCol;

    public List<GameObject> gos = new List<GameObject>();

    public void InitLatch()
    {
        float innerX = transform.localPosition.x - bodies[0].transform.localPosition.x;
        float innerZ = transform.localPosition.z - bodies[0].transform.localPosition.z;

        int dirIdx = 0;
        if (innerX > 0.5f)
        {
            Debug.Log($"InitLatch Right");
            dirIdx = 3;
        }
        else if (innerX < -0.5f)
        {
            Debug.Log($"InitLatch Left");
            dirIdx = 4;
        }
        else if (innerZ > 0.5f)
        {
            Debug.Log($"InitLatch Top");
            dirIdx = 1;
        }
        else
        {
            Debug.Log($"InitLatch Bottom");
            dirIdx = 2;
        }

        targetRow = row + GridManager.DIRS[dirIdx - 1].dr;
        targetCol = col + GridManager.DIRS[dirIdx - 1].dc;

        GameObject head = Instantiate(LevelManager.I.m_gridGenerate.latchHeadPrefab);
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, -0.65f, 0);
        head.transform.localEulerAngles = new Vector3(0, 0, 0);
        gos.Add(head);

        for (int i = 0; i < bodies.Count; i++)
        {
            GameObject body = Instantiate(LevelManager.I.m_gridGenerate.latchBodyPrefab);
            body.transform.SetParent(bodies[i].transform);
            body.transform.localPosition = new Vector3(0, -0.65f, 0);
            body.transform.localEulerAngles = new Vector3(0, 0, 0);
            gos.Add(body);
        }

        switch (dirIdx)
        {
            case 1:
                break;
            case 2:
                gos[0].transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case 3:
                gos[0].transform.localEulerAngles = new Vector3(0, 90, 0);
                for (int i = 1; i < gos.Count; i++)
                {
                    gos[i].transform.localEulerAngles = new Vector3(0, 90, 0);
                }
                break;
            case 4:
                gos[0].transform.localEulerAngles = new Vector3(0, -90, 0);
                for (int i = 1; i < gos.Count; i++)
                {
                    gos[i].transform.localEulerAngles = new Vector3(0, 90, 0);
                }
                break;
        }
    }
    public void OpenLatch()
    {
        if (target == null)
        {
            if (LevelManager.I.m_gridManager.grid[targetRow, targetCol] is ObjectTile o)
            {
                target = o;
            }
            else
            {
                Debug.LogError("Gara Error");
                return;
            }
        }

        if (target.status == CellStatus.empty && this.status != CellStatus.empty)
        {
            this.status = CellStatus.empty;
           // this.jar.jarAnimation.SpawmActiveFX();
            for (int i = 0; i < bodies.Count; i++)
            {
              //  bodies[i].jar.jarAnimation.SpawmActiveFX();
                bodies[i].status = CellStatus.empty;
            }

            for (int i = 0; i < gos.Count; i++)
            {
                gos[i].transform.DOScale(new Vector3(0, 0, gos[i].transform.localScale.z), 0.3f).SetEase(Ease.InBack);
            }
            DOVirtual.DelayedCall(0.3f, () =>
            {
                LevelManager.I.m_gridManager.AfterTileClick();
            });
        }
    }
    public override void Init()
    {
       shooter.gameObject.SetActive(false);
    }
    public override void Active()
    {
    }

    public override void ClickProcess(Slot item)
    {

    }
}
