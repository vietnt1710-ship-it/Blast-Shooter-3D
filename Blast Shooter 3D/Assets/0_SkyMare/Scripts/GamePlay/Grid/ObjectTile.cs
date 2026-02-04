using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectTile : Tile
{
    public Vector3 jarPos = new Vector3(0, -0.49999994f, 0.0829999447f);
    public Shooter shooter;
    public ShooterData data;
    public void SplitColorID(string af)
    {
        Debug.Log($"Shooter data string is : {af}");
        data = new ShooterData(af);
    }


    public abstract void Init();
    public abstract void Active();

    public abstract void ClickProcess(Slot item);


    public void Initizal()
    {
        shooter = GetComponentInChildren<Shooter>();
        shooter.InitShooter(this.data);
        Init();
    }

    public void ActiveTile()
    {
        status = CellStatus.active;
        Active();
        
        //sandbox.transform.DOScale(_jarStartScale * 1.5f, 0.5f);

    }

    public void OnClick()
    {
        Debug.Log($"Click on {row}, {col} _ clicked");

    //    if (status == CellStatus.empty) return;
    //    if (type == TileType.latch) return;

    //    if (HelperManager.I.IsHELPERPLAYING) return;

    //    SoundManager.I.Play(SoundManager.Sfx.Tab);



    //    if (HelperManager.I.IsHELPING)
    //    {
    //        if (HelperManager.I.currentType == HelperManager.HelperType.BottleUp)
    //        {
    //            HelperManager.I.DecreaseBoster();
    //            HelperManager.I.CloseNoti();
    //            HelperManager.I.IsHELPERPLAYING = true;

    //            HapticManager.PlayPreset(HapticManager.Preset.MediumImpact);
    //            HelperManager.I.magicBottleAnimation.StartAnim(this.transform, () =>
    //            {
    //                HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
    //                Slot item = LevelManager.I.m_slots.YoungestStackEmpty();
    //                if (item == null) return;
    //                status = CellStatus.empty;

    //                BottleUpClickProcess(item);

    //                MazeManager.I.AfterTileClick();
    //                HelperManager.I.EndBoster(false);

    //                DOVirtual.DelayedCall(0.9f, () =>
    //                {
    //                    Conveyror.I.CountineConveyror();
    //                    HelperManager.I.IsHELPERPLAYING = false;
    //                });

    //            });
    //        }
    //        if (HelperManager.I.currentType == HelperManager.HelperType.Hammer)
    //        {
    //            HelperManager.I.DecreaseBoster();
    //            HelperManager.I.CloseNoti();
    //            HelperManager.I.IsHELPERPLAYING = true;
    //            HelperManager.I.smashEffect.Show(jar.transform, () =>
    //            {
    //                HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
    //                ObjectPoolManager.I.Spawn("DestroyFX", jar.transform.position);
    //                jar.gameObject.SetActive(false);


    //                DOVirtual.DelayedCall(0.3f, () =>
    //                {
    //                    if (data.HaveThreeColor)
    //                    {
    //                        int[] adjustedIDs = data.threeColorIDs
    //                         .Where((id, index) => !data.filledindex.Contains(index))
    //                         .Select(id => id - 1)
    //                         .ToArray();

    //                        HelperManager.I.tubeColorScissors.CutColorSegments(adjustedIDs.ToList());
    //                    }
    //                    else
    //                    {
    //                        // ColorWithID color = ColorID.ColorWithID(jar.data.colorID);

    //                        SpritePixelEraser.I.ErasePixelsByIdFromBottom(jar.data.colorID, jar.pixelCount);
    //                    }
    //                    status = CellStatus.empty;

    //                    if (MazeManager.I.IsEmptyFull())
    //                    {
    //                        GameManger.I.ShowTapToSpeeUp();
    //                    }

    //                    if (this is LockTile lt) lt.target.DestroyKey();
    //                    if (this is KeyTile k) k.target.DestroyLock();
    //                    if (this is ConnectTile cn) cn.DestroyRope();

    //                    MazeManager.I.AfterTileClick();
    //                    DOVirtual.DelayedCall(0.9f, () =>
    //                    {
    //                        Conveyror.I.CountineConveyror();
    //                        HelperManager.I.IsHELPERPLAYING = false;
    //                    });

    //                    HelperManager.I.EndBoster(false);
    //                });

    //            });

    //        }
    //    }
    //    else
    //    {
    //        if (status == CellStatus.active && jar.jarAnimation.isReady)
    //        {
    //            Debug.Log($"Click on Active {row}, {col}");
    //            HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
    //            Slot item = LevelManager.I.m_slots.YoungestStackEmpty();
    //            if (item == null) return;
    //            status = CellStatus.empty;

    //            ClickProcess(item);

    //            MazeManager.I.AfterTileClick();
    //        }
    //        else if (status == CellStatus.watting)
    //        {
    //            Debug.Log($"Click on Watting {row}, {col}");
    //            HapticManager.PlayPreset(HapticManager.Preset.LightImpact);
    //            jar.jarAnimation.StartTilt(0.8f);
    //        }
    //    }


    }

}
