using UnityEngine;

[CreateAssetMenu(menuName = "Voxels/Voxel Volume Data")]
public class VoxelVolumeData : ScriptableObject
{
    public Vector3 origin;     // world-space origin of grid (min corner)
    public float voxelSize;
    public int nx, ny, nz;

    // 0 empty, 1 rind, 2 flesh, 3 seed
    public byte[] voxels;

    public int Index(int x, int y, int z) => x + nx * (y + ny * z);

    public bool InBounds(int x, int y, int z)
        => x >= 0 && y >= 0 && z >= 0 && x < nx && y < ny && z < nz;
}
