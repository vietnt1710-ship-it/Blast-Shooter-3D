using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public ColorID colorData;

    public GridManager m_gridManager;
    public GridGenerate m_gridGenerate;

    public TextAsset levelTest;

    private void Start()
    {
        LoadLevel();
    }
    public void LoadLevel()
    {
        var grid = TxTToGrid(levelTest);

        m_gridManager.InitGrid(grid);
        m_gridGenerate.LoadLevel(grid);

        m_gridManager.StartActiceLevel();
    }

    /// <summary>
    /// Load nội dung từ file csv vào grid
    /// </summary>
    /// <returns></returns>
    public string[,] TxTToGrid(TextAsset textAssset)
    {
        string gridData = textAssset.text;

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
