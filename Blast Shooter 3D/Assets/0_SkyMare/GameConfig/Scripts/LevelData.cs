using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VoxelLayerPainter;


public class LevelData : ScriptableObject
{
    public float voxelSize;
    public Vector3 origin;

    public List<LayerGroup> voxelData = new List<LayerGroup>(); // lấy từ groupedByY
    public List<Color> colors = new List<Color>();
    public List<int> colorIndex = new List<int>();
    public TextAsset grid;

    public string[,] TxTToGrid()
    {
        string gridData = grid.text;

        string[] rows = gridData.Split('\n');

        string[] lends = rows[0].Split(' ');

        string[,] gridS = new string[rows.Length, lends.Length];

        for (int i = 0; i < rows.Length; i++)
        {
            string[] cells = rows[i].Split(' ');
            for (int j = 0; j < cells.Length; j++)
            {
                // Xử lý từng cell: cells[j]
                gridS[i, j] = cells[j].Trim();
                Debug.Log($"Row {i}, Col {j}: {cells[j]}");
            }
        }
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
        return gridS;
    }
}
