using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    [Header("Settings")]
    int rows;
    int cols;

    public Tile[,] grid;

   

    public List<FrezzeTile> frezzeTiles = new List<FrezzeTile>();

    public List<PipeTile> pipeTiles = new List<PipeTile>();
    public List<LatchTile> latchs = new List<LatchTile>();

    public static readonly (int dr, int dc)[] DIRS =
    {
     (-1, 0), // up
     ( 1, 0), // down
     ( 0, 1), // right
     ( 0,-1), // left
    };

    public void Reset()
    {
        ClearGrid();
        frezzeTiles.Clear();
        pipeTiles.Clear();
        latchs.Clear();
    }
    [Serializable]
    public class StorgePixelData
    {
        public int pixelCount;
        int pixelTempCount = 0;
        public Color color;
        public List<ShooterData> jars = new List<ShooterData>();
        public void LoadPixelEachJar()
        {
            pixelTempCount = pixelCount;

            int avg = (int)Math.Ceiling((double)pixelCount / jars.Count);

            for (int i = 0; i < jars.Count; i++)
            {
                if (avg >= pixelTempCount)
                {
                    //jars[i].pixelCount = pixelTempCount;

                }
                else
                {
                    //jars[i].pixelCount = avg;
                }
                //pixelTempCount -= jars[i].pixelCount;
            }
        }
    }
    //public List<StorgePixelData> storgePixelDatas;
    //public void LoadPixelCount()
    //{
    //    for (int i = 0; i < PixelSpriteViewer.I.stats.Count; i++)
    //    {
    //        var data = PixelSpriteViewer.I.stats[i];
    //        var newStorge = new StorgePixelData();
    //        newStorge.pixelCount = data.pixelCount;
    //        newStorge.color = data.color;
    //        for (int h = 0; h < rows; h++)
    //        {
    //            for (int k = 0; k < cols; k++)
    //            {
    //                if (grid[h, k] is ObjectTile ot)
    //                {
    //                    if (!(ot is LatchTile) && ot.status != Tile.CellStatus.empty)
    //                    {
    //                        if (ot.data.colorID == data.indexColor)
    //                        {
    //                            newStorge.jars.Add(ot.data);
    //                            //.jars.Add(ot.jar);
    //                        }
    //                    }

    //                }
    //                else if (grid[h, k] is PipeTile pipe)
    //                {
    //                    for (int m = 0; m < pipe.dataWaitings.Count; m++)
    //                    {
    //                        if (pipe.dataWaitings[m].colorID == data.indexColor)
    //                        {
    //                            newStorge.jars.Add(pipe.dataWaitings[m]);
    //                            //.jars.Add(ot.jar);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        newStorge.LoadPixelEachJar();
    //        storgePixelDatas.Add(newStorge);

    //        Debug.Log($"LoadPixelCount id {PixelSpriteViewer.I.stats[i].indexColor}  have {storgePixelDatas.Count}");
    //    }


    //}

    public bool IsEmptyFull()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j] is ObjectTile ot)
                {
                    if (ot.status != CellStatus.empty)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public ObjectTile GetRandomEmptyTile()
    {
        List<ObjectTile> emptyTiles = new List<ObjectTile>();

        // Thu thập tất cả các tile empty
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j] is ObjectTile ot)
                {
                    if (ot.status == CellStatus.watting || ot.status == CellStatus.active)
                    {
                        emptyTiles.Add(ot);
                    }
                }
            }
        }

        // Nếu không có tile empty thì return null
        if (emptyTiles.Count == 0)
        {
            return null;
        }

        // Chọn ngẫu nhiên 1 tile từ danh sách
        int randomIndex = Random.Range(0, emptyTiles.Count);
        Debug.Log($"ShowHandHammmer {emptyTiles[randomIndex].row} : {emptyTiles[randomIndex].col}");
        return emptyTiles[randomIndex];
    }
    public void InitGrid(string[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        this.grid = new Tile[rows, cols];
    }
    public void StartActiceLevel()
    {
        DOVirtual.DelayedCall(0.2f, () =>
        {
            float delayTime = (grid.GetLength(0)) * 0.1f + (grid.GetLength(0)) * 0.1f;
            DOVirtual.DelayedCall(delayTime, () =>
            {
                CheckAvtive();
                //LoadPixelCount();
            });
        });

    }
    public void FindCorners()
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        //for (int i = 0; i < rows; i++)
        //{
        //    for (int j = 0; j < cols; j++)
        //    {
        //        if (grid[i, j].status != Tile.CellStatus.wall)
        //        {
        //            CheckCorner(i, j);
        //        }
        //    }
        //}
        CombineAllCornersToWallCombine();
    }
    void CombineAllCornersToWallCombine()
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        Transform wallCombineTransform = LevelManager.I.m_gridGenerate.wallCombine.transform;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j].status != CellStatus.wall)
                {
                    Transform cellTransform = grid[i, j].transform;

                    // Set parent cho children từ 0 đến 3
                    int count = 0;
                    while (count < 4)
                    {
                        count++;
                        //Transform child = cellTransform.GetChild(0);
                        //child.SetParent(wallCombineTransform, true);
                    }
                }
            }
        }

        Debug.Log($"<color=green>✓ Hoàn thành combine corners vào wallCombine</color>");
    }
    void CheckCorner(int row, int col)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Định nghĩa 4 loại góc với các hướng cần check
        var cornerTypes = new[]
        {
            new { Name = "TOP-LEFT", Dirs = new[] { (-1, 0), (0, -1), (-1, -1) } },
            new { Name = "TOP-RIGHT", Dirs = new[] { (-1, 0), (0, 1), (-1, 1) } },
            new { Name = "BOTTOM-LEFT", Dirs = new[] { (1, 0), (0, -1), (1, -1) } },
            new { Name = "BOTTOM-RIGHT", Dirs = new[] { (1, 0), (0, 1), (1, 1) } }
        };

        foreach (var corner in cornerTypes)
        {
            int wallCount = 0;

            foreach (var dir in corner.Dirs)
            {
                int newRow = row + dir.Item1;
                int newCol = col + dir.Item2;

                if (newRow >= rows || newCol < 0 || newCol >= cols)
                {
                    wallCount++;
                }
                else if (newRow < 0)
                {
                    // Điểm tiếp nối
                }
                else if (grid[newRow, newCol].status == CellStatus.wall)
                {
                    wallCount++;
                }
            }

            if (wallCount == 3)
            {
                GameObject cornerObject = null;

                // Xử lý cho từng loại góc
                switch (corner.Name)
                {
                    case "TOP-LEFT":
                        cornerObject = grid[row, col].transform.GetChild(0).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "TOP-RIGHT":
                        cornerObject = grid[row, col].transform.GetChild(1).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "BOTTOM-LEFT":
                        cornerObject = grid[row, col].transform.GetChild(2).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "BOTTOM-RIGHT":
                        cornerObject = grid[row, col].transform.GetChild(3).gameObject;
                        cornerObject.SetActive(true);
                        break;
                }

                // Raycast từ cornerObject theo trục Y để tìm Base_Straight
                if (cornerObject != null)
                {
                    DOVirtual.DelayedCall(0.02f, () =>
                    {
                        RaycastCornerToBaseStraight(cornerObject);
                    });

                }
            }
        }
        void RaycastCornerToBaseStraight(GameObject cornerObject)
        {
            Transform cornerTransform = cornerObject.transform;

            // Sử dụng local position và local direction
            Vector3 localOrigin = cornerTransform.position;
            Vector3 localDirection = new Vector3(0, -0.7660f, 0.6428f);

            float maxDistance = 100f;

            // Debug.DrawRay(localOrigin, localDirection * maxDistance, Color.yellow, 100f);

            RaycastHit[] hits = Physics.RaycastAll(localOrigin, localDirection, maxDistance);

            foreach (RaycastHit hit in hits)
            {

                /// Kiểm tra xem object bị hit có tên chứa "Base_Straight" không
                if (hit.collider.gameObject.name.Contains("Base_Straight"))
                {
                    GameObject baseStraight = hit.collider.gameObject;

                    // Ẩn Base_Straight đi
                    baseStraight.SetActive(false);

                    // Debug để kiểm tra
                    Debug.Log($"<color=cyan>Raycast hit:</color> {baseStraight.name} tại vị trí {hit.point}");
                }
            }
        }
    }

    public void UnFrezze()
    {
        for (int i = 0; i < frezzeTiles.Count; i++)
        {
            frezzeTiles[i].UnFezze();
        }
    }
    public void PipeAtice()
    {
        for (int i = 0; i < pipeTiles.Count; i++)
        {
            pipeTiles[i].SpawmJar();
        }
    }
    public void LatchOpen()
    {
        for (int i = 0; i < latchs.Count; i++)
        {
            latchs[i].OpenLatch();
        }
    }
    public void AfterTileClick(bool bosterUp = false)
    {
        PipeAtice();
        LatchOpen();
        if (!bosterUp)
        {
            UnFrezze();
        }
        CheckAvtive();
    }
    public void CheckAvtive()
    {
        rows = grid.GetLength(0);
        cols = grid.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] is ObjectTile ot)
                {
                    if (NeedCheck(ot))
                    {
                        if (HavePath(row, col))
                        {
                            ot.ActiveTile();
                        }
                    }
                }
            }
        }
    }
    bool NeedCheck(ObjectTile ot)
    {
        if (ot.status != CellStatus.watting) return false;
        if (ot.type != TileType.normal && ot.type != TileType.hiden && ot.type != TileType.key && ot.type != TileType.connect) return false;

        return true;
    }
    public bool HavePath(int row, int col)
    {
        ResetVisited();
        return DFS((row, col));
    }
    private bool DFS((int row, int col) node)
    {
        int row = node.row, col = node.col;

        grid[row, col].visited = true;

        for (int t = 0; t < 4; t++)
        {
            int nrow = row + DIRS[t].dr;
            int ncol = col + DIRS[t].dc;

            if (nrow < 0) return true; // cần tìm

            if (ncol < 0 || nrow >= rows || ncol >= cols) continue;

            if (grid[nrow, ncol].type == TileType.pipe) continue;

            if (grid[nrow, ncol].status != CellStatus.empty) continue;

            if (grid[nrow, ncol].visited) continue;

            if (DFS((nrow, ncol))) return true;

        }

        return false;
    }
    public void ResetVisited()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] is ObjectTile)

                    grid[row, col].visited = false;
            }
        }
    }
    public void ClearGrid()
    {
        if (grid == null) return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Destroy toàn bộ Tile trong grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);

                    grid[x, y] = null;
                }
            }
        }

        // Clear tham chiếu
        grid = null;
    }

}
