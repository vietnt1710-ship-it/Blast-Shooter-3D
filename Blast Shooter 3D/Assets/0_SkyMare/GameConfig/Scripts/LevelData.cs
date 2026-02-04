using System.Collections;
using System.Collections.Generic;
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
}
