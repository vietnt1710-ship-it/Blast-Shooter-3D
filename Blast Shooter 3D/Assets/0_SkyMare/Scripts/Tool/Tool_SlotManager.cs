using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool_SlotManager : MonoBehaviour
{
    public int columns = 10;

    public Tool_Slot[,] gridTile;

    private void Start()
    {
        GetGridSlot();
    }
    //public void ReLoad(string[,] sgrid)
    //{
    //    int rows = gridTile.GetLength(0);
    //    int cols = gridTile.GetLength(1);
    //    var grid = new string[rows, cols];

    //    for (int row = 0; row < rows; row++)
    //    {
    //        for (int col = 0; col < cols; col++)
    //        {
    //            Debug.Log($"Value of tile{row} , {col} : {sgrid[row, col]}");
    //            gridTile[row, col].ReLoad(sgrid[row, col]);
    //        }
    //    }
    //}
    public void Clear()
    {
        if (Time.time < 0.2f) return;

        int rows = gridTile.GetLength(0);
        int cols = gridTile.GetLength(1);
        var grid = new string[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                gridTile[row, col].Clear();
            }
        }
    }
    public string[,] Grid()
    {
        int rows = gridTile.GetLength(0);
        int cols = gridTile.GetLength(1);
        var grid = new string[rows, cols];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                //gridTile[row, col].UpdateValue();
                //grid[row, col] = gridTile[row, col].value;
            }
        }
        return grid;
    }
    private void GetGridSlot()
    {
        // Lấy danh sách các child đang active
        List<Transform> activeChildren = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                activeChildren.Add(child);
            }
        }

        int count = activeChildren.Count;
        int cols = Mathf.Clamp(columns, 0, count);
        if (count == 0 || cols <= 0) return;

        // Số hàng cần thiết (ceil)
        int rows = (count + cols - 1) / cols;

        //gridTile = new SlotTile[rows, cols];

        for (int i = 0; i < count; i++)
        {
            var t = activeChildren[i];

            // --- Tính vị trí theo hàng/cột (tính từ góc trên-left) ---
            int col = i % cols;
            int row = i / cols;

            //SlotTile s = t.GetComponentInChildren<SlotTile>();
            //if (s != null)
           // {
            //    s.row = row;
            //    s.col = col;
           // }
          //  gridTile[row, col] = s;

        }
    }
}

