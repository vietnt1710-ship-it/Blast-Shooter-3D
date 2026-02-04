using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GridGenerate;
public class WallTile : Tile
{
    public enum ConnerType
    {
        connerTopLeft,
        connerTopRight,

        connerBottomLeft,
        connerBottomRight,

        connerLeft,
        connerRight,
        connerTop,
        connerBottom,

        edgeLeft,
        edgeRight,
        edgeTop,
        edgeBottom,

        connectVetircal,
        connectHorizontal,

        center,
    }
    public GameObject center;
    public GameObject roof; 
    public GameObject coner;
    public GameObject conner2;
    public GameObject edge;
    public GameObject connect;
    //public GameObject connerTopRight;
    //public GameObject connerBottomLeft;
    //public GameObject connerBottomRight;
    //public GameObject connerLeft;
    //public GameObject connerRight;
    //public GameObject connerTop;
    //public GameObject connerBottom;
    //public GameObject connerCenter;

    public int wrow,wcol;
    public void SetConner(ConnerType type)
    {
        coner.gameObject.SetActive(true);
        switch (type)
        {
            case ConnerType.connerTopLeft: coner.transform.localEulerAngles = new Vector3(0,90,0); break;
            case ConnerType.connerTopRight: coner.transform.localEulerAngles = new Vector3(0, 180, 0); break;
            case ConnerType.connerBottomLeft: coner.transform.localEulerAngles = new Vector3(0, 0, 0); break;
            case ConnerType.connerBottomRight: coner.transform.localEulerAngles = new Vector3(0, -90, 0); break;
        }

    }
    public void SetConner2(ConnerType type)
    {
        conner2.gameObject.SetActive(true);
        switch (type)
        {
            case ConnerType.connerLeft: conner2.transform.localEulerAngles = new Vector3(0, 0, 0); break;
            case ConnerType.connerRight: conner2.transform.localEulerAngles = new Vector3(0, 180, 0); break;
            case ConnerType.connerTop: conner2.transform.localEulerAngles = new Vector3(0, 90, 0); break;
            case ConnerType.connerBottom: conner2.transform.localEulerAngles = new Vector3(0, -90, 0); break;

        }

    }
    public void SetEdge(ConnerType type)
    {
        edge.gameObject.SetActive(true);
        switch (type)
        {
            case ConnerType.edgeLeft: edge.transform.localEulerAngles = new Vector3(0, 0, 0); break;
            case ConnerType.edgeRight: edge.transform.localEulerAngles = new Vector3(0, 180, 0); break;
            case ConnerType.edgeTop: edge.transform.localEulerAngles = new Vector3(0, 90, 0); break;
            case ConnerType.edgeBottom: edge.transform.localEulerAngles = new Vector3(0, -90, 0); break;

        }

    }
    public void SetConnect(ConnerType type)
    {
        connect.gameObject.SetActive(true);
        switch (type)
        {
            case ConnerType.connectHorizontal: connect.transform.localEulerAngles = new Vector3(0, 0, 0); break;
            case ConnerType.connectVetircal: connect.transform.localEulerAngles = new Vector3(0, 90, 0); break;
    
        }

    }
    public void CheckCorner(StringWrapper[,] wrapperGrid)
    {
        bool up = IsWall(wrow - 1, wcol, wrapperGrid);
        bool down = IsWall(wrow + 1, wcol, wrapperGrid);
        bool left = IsWall(wrow, wcol - 1, wrapperGrid);
        bool right = IsWall(wrow, wcol + 1, wrapperGrid);

        int mask = (up ? 1 : 0) | (right ? 2 : 0) | (down ? 4 : 0) | (left ? 8 : 0);
        Debug.Log($"row{wrow} , col{wcol}, up {up}, down{down}, left {left}, right{right} , count {mask}");
        switch (mask)
        {
            case 0: center.gameObject.SetActive(true); break; // không mặt nào chạm tường
            case 15: roof.gameObject.SetActive(true); break; // 4 mặt chạm tường
            
                //1 mặt chạm tường
            case 1: SetConner2(ConnerType.connerBottom); break; // up
            case 2: SetConner2(ConnerType.connerLeft); break;   // right
            case 4: SetConner2(ConnerType.connerTop); break;    // down
            case 8: SetConner2(ConnerType.connerRight); break;  // left
                // 3 mặt chạm tường
            case 13: SetEdge(ConnerType.edgeRight); break; // up + left + down 
            case 7: SetEdge(ConnerType.edgeLeft); break;   // up + right  + down
            case 14: SetEdge(ConnerType.edgeTop); break;    // right + left + down 
            case 11: SetEdge(ConnerType.edgeBottom); break;  //  right + left + up
                // 2 mặt chạm tường 
            case 6: SetConner(ConnerType.connerTopLeft); break;      // down+right
            case 12: SetConner(ConnerType.connerTopRight); break;     // down+left
            case 3: SetConner(ConnerType.connerBottomLeft); break;   // up+right
            case 9: SetConner(ConnerType.connerBottomRight); break;  // up+left

            case 5: SetConnect(ConnerType.connectVetircal); break; // up + down
            case 10: SetConnect(ConnerType.connectHorizontal); break; // right + left

            default: break;
        }
    }
    bool InBounds(int r, int c, StringWrapper[,]  wrapperGrid)
    {
        return r >= 0 && r < wrapperGrid.GetLength(0) && c >= 0 && c < wrapperGrid.GetLength(1);
    }

    bool IsWall(int r, int c , StringWrapper[,] wrapperGrid)
    {
        if (!InBounds(r, c, wrapperGrid)) return false;

        if (GridParse.TypeOf(wrapperGrid[r, c].content) == 0) return true;

        return false;
    }
}
