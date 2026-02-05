
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class ToolManager : Singleton<ToolManager>
{
    [Serializable]
    public class TypeAndID
    {
        public TileType type;
        public int tileID;
    }

    [Serializable]
    public class TypeAndIDAndButton
    {
        public TypeAndID type;
        public Button button;

        public string GetButtonName()
        {
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            return text.text;
        }
        public Color GetButtonColor()
        {
            return button.image.color;
        }
    }

    public List<TypeAndIDAndButton> typeButtonList;
    
    protected void Awake()
    {
        for(int i = 0; i < typeButtonList.Count; i++)
        {
            int index = i;
            typeButtonList[index].button.onClick.AddListener( ()=> { ChangeType(typeButtonList[index]); });
        }
    }
    public TMP_Text typeName;
    public TypeAndIDAndButton selectTypeAndIDandButton;
    public Color currentColor;
    public int colorID;
    public ColorID colorWithID;
    public VoxelLayerPainter painter;
    public Tool_SlotManager slotManager;
    public List<int> cubeVerUniqueColors;
    public LevelDataConfigPicker picker;
    public void UpdateCount()
    {
        cubeVerUniqueColors = new List<int>(painter.cubeVerUniqueColors);

        Dictionary<int, int> colorCount = new Dictionary<int, int>();

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
               Tool_Slot slot = slotManager.gridTile[row, col];

               if (slot.id != 6)
               {
                    int colorID = slot.colorID;

                    if (colorCount.ContainsKey(colorID))
                    {
                        colorCount[colorID]+= slot.bulletCount;
                    }
                    else
                    {
                        colorCount[colorID] = slot.bulletCount;
                    }
                }
                else
                {
                    for (int k = 0; k < slot.dataIngaras.Count; k++)
                    {
                        string blc;
                        string cli;
                        GridParse.OnSplitBeAf(slot.dataIngaras[k], out blc, out cli);

                        int colorID = int.Parse(cli);
                        int bulletCount = int.Parse(blc);
                        if (colorCount.ContainsKey(colorID))
                        {
                            colorCount[colorID] += bulletCount;
                        }
                        else
                        {
                            colorCount[colorID] = bulletCount;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < painter.uniqueColors.Count; i++)
        {
            int ID = colorWithID.ColorWithID3(painter.uniqueColors[i]).ID;
            if (colorCount.ContainsKey(ID))
            {
                cubeVerUniqueColors[i] -= colorCount[ID];
                painter._colorButtons[i].GetComponentInChildren<TMP_Text>().text = cubeVerUniqueColors[i].ToString();
            }
            //else
            //{
            //    bottle.UpdateValue(0);
            //}
        }
    }

    public void ChangeColor(Color color)
    {
        currentColor = color;
        colorID = colorWithID.ColorWithID3(currentColor).ID;
    }
    public Color color => colorWithID.ColorWithID2(colorID).color;
    public void ChangeType(TypeAndIDAndButton typeAndIDAndButton)
    {
        typeName.text = typeAndIDAndButton.GetButtonName();
        this.selectTypeAndIDandButton = typeAndIDAndButton;
    }
    public List<Tool_SlotTileInGara> tool_SlotTileInGaras;
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        }

    }
    public void SaveData()
    {
        Export();
    }
    public void LoadData()
    {
        if (picker.select.grid == null) return;

        slotManager.ReLoad(ExpandGrid());

        DOVirtual.DelayedCall(0.2f, () =>
        {
            UpdateCount();
        });
    }

    private void Export()
    {

        LevelData newData = picker.select;

        var a = TrimGrid(slotManager.Grid());
        SaveGrid(a, newData);

        // Đánh dấu là đã thay đổi
        EditorUtility.SetDirty(newData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Đã cập nhật level: " + newData.name);

    }
    public string levelDataPath; 
    public async void SaveGrid(string[,] grid, LevelData levelData)
    {
        // Tạo timestamp tránh ký tự không hợp lệ
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fullPath = $"{levelDataPath}/{picker.select}_TextGrid_{timestamp}.txt";

        StringBuilder sb = new StringBuilder();

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                sb.Append(grid[i, j]);
                if (j < cols - 1)
                    sb.Append(" ");
            }
            if (i < rows - 1)
                sb.AppendLine();
        }

        await Task.Run(() =>
        {
            string directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else if (string.IsNullOrEmpty(directory))
            {
                // Nếu không có thư mục, dùng thư mục hiện tại
                directory = Directory.GetCurrentDirectory();
                fullPath = Path.Combine(directory, Path.GetFileName(fullPath));
            }

            File.WriteAllText(fullPath, sb.ToString());
        });

        // Làm mới AssetDatabase để nhận diện file mới
        AssetDatabase.Refresh();

        // Load lại file vừa tạo thành TextAsset
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
        levelData.grid = textAsset;

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Đã tạo file mới tại: {fullPath}");
        //levelDataLoader.RefreshLevelDataList();
    }
    /// <summary>
    /// Loại bỏ các hàng và cột chỉ chứa toàn số 0 từ mảng 2 chiều
    /// </summary>
    /// <param name="grid">Mảng 2 chiều string[,]</param>
    /// <returns>Mảng 2 chiều đã được trim</returns>
    public static string[,] TrimGrid(string[,] grid)
    {
        if (grid == null || grid.Length == 0)
            return new string[0, 0];

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Tìm hàng đầu tiên không toàn 0
        int firstRow = -1;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j] != "0")
                {
                    firstRow = i;
                    break;
                }
            }
            if (firstRow != -1) break;
        }

        // Nếu toàn grid là 0
        if (firstRow == -1)
            return new string[0, 0];

        // Tìm hàng cuối cùng không toàn 0
        int lastRow = -1;
        for (int i = rows - 1; i >= 0; i--)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j] != "0")
                {
                    lastRow = i;
                    break;
                }
            }
            if (lastRow != -1) break;
        }

        // Tìm cột đầu tiên không toàn 0
        int firstCol = -1;
        for (int j = 0; j < cols; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                if (grid[i, j] != "0")
                {
                    firstCol = j;
                    break;
                }
            }
            if (firstCol != -1) break;
        }

        // Tìm cột cuối cùng không toàn 0
        int lastCol = -1;
        for (int j = cols - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                if (grid[i, j] != "0")
                {
                    lastCol = j;
                    break;
                }
            }
            if (lastCol != -1) break;
        }

        // Tạo mảng mới với kích thước đã trim
        int newRows = lastRow - firstRow + 1;
        int newCols = lastCol - firstCol + 1;
        string[,] trimmedGrid = new string[newRows, newCols];

        // Copy dữ liệu
        for (int i = 0; i < newRows; i++)
        {
            for (int j = 0; j < newCols; j++)
            {
                trimmedGrid[i, j] = grid[firstRow + i, firstCol + j];
            }
        }

        return trimmedGrid;
    }
    /// <summary>
    /// Đưa grid đã trim vào lại grid có kích thước cố định (mặc định 10x10)
    /// </summary>
    /// <param name="trimmedGrid">Grid đã được trim</param>
    /// <param name="targetRows">Số hàng mong muốn (mặc định 10)</param>
    /// <param name="targetCols">Số cột mong muốn (mặc định 10)</param>
    /// <param name="offsetRow">Vị trí hàng bắt đầu đặt grid (mặc định 0)</param>
    /// <param name="offsetCol">Vị trí cột bắt đầu đặt grid (mặc định 0)</param>
    /// <returns>Grid mới với kích thước cố định</returns>
    public string[,] ExpandGrid(int targetRows = 10, int targetCols = 10)
    {
        var trimmedGrid = picker.select.TxTToGrid();

        int trimRows = trimmedGrid.GetLength(0);
        int trimCols = trimmedGrid.GetLength(1);

        // Tính toán offset để căn giữa trimmedGrid
        int offsetRow = 0;// (targetRows - trimRows) / 2;
        int offsetCol = (targetCols - trimCols) / 2;

        // Tạo grid mới với toàn bộ là "0"
        string[,] expandedGrid = CreateEmptyGrid(targetRows, targetCols);

        // Copy dữ liệu từ trimmed grid vào vị trí trung tâm
        for (int i = 0; i < trimRows && (offsetRow + i) < targetRows; i++)
        {
            for (int j = 0; j < trimCols && (offsetCol + j) < targetCols; j++)
            {
                expandedGrid[offsetRow + i, offsetCol + j] = trimmedGrid[i, j];
                Debug.Log($"Value of ExpandGrid Tile({offsetRow + i}, {offsetCol + j}): {expandedGrid[offsetRow + i, offsetCol + j]}");
            }
        }

        return expandedGrid;
    }

    /// <summary>
    /// Tạo grid rỗng với toàn bộ giá trị là "0"
    /// </summary>
    private static string[,] CreateEmptyGrid(int rows, int cols)
    {
        string[,] grid = new string[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                grid[i, j] = "0";
            }
        }
        return grid;
    }

}


