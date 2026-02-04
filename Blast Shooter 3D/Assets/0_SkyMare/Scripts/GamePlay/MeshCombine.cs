using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombine : MonoBehaviour
{
    public int maxVerticesPerMesh = 65534;

    const string Combined = "Combined";

    public Material overrideMaterial;   // material gỗ bạn muốn dùng cho toàn bộ mesh
    public float uvPerUnit = 0.5f;      // mật độ vân: 0.5 nghĩa là 1 unit lặp 0.5 lần (texture to hơn)
    public Vector2 uvOffset = Vector2.zero;
    public bool regenerateUV = true;
    public UVAxis uvAxis = UVAxis.XZ;   // Trục để tính UV (XZ cho sàn, XY cho tường)

    public enum UVAxis
    {
        XY,
        XZ,
        YZ
    }

    struct SubInfo
    {
        public int subMeshIndex;
        public Material material;
    }

    class MeshEntry
    {
        public Mesh mesh;
        public Transform transform;
        public int vertexCount;
        public List<SubInfo> subs = new List<SubInfo>();
        public Vector3[] worldVertices; // Lưu vị trí đỉnh trong không gian thế giới
    }

    public void CombineNow()
    {
        List<MeshEntry> entries = new List<MeshEntry>();
        GetChildsMesh(out entries);
        SplitMesh(entries);

        WallTile[] wallTiles = GetComponentsInChildren<WallTile>();
        for (int i = 0; i < wallTiles.Length; i++)
        {
            if (wallTiles[i].row == -1 || wallTiles[i].col == -1)
            {
                Destroy(wallTiles[i].gameObject);
            }
            else
            {
                wallTiles[i].gameObject.gameObject.SetActive(false);
            }
        }
    }

    void GetChildsMesh(out List<MeshEntry> entries)
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>();
        entries = new List<MeshEntry>();

        foreach (var render in meshFilters)
        {
            if (render.sharedMesh == null) continue;
            if (render.gameObject == this.gameObject) continue;
            if (render.transform.name.Contains(Combined)) continue;

            var mr = render.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            if (!mr.enabled || !render.gameObject.activeInHierarchy) continue;

            var mesh = render.sharedMesh;
            var mats = mr.sharedMaterials;
            int subCount = mesh.subMeshCount;

            if (mats == null || mats.Length < subCount)
            {
                var fixedMats = new Material[subCount];
                for (int i = 0; i < subCount; i++)
                    fixedMats[i] = (mats != null && i < mats.Length) ? mats[i] : null;
                mats = fixedMats;
            }

            // Lấy vị trí đỉnh trong không gian thế giới
            Vector3[] worldVertices = new Vector3[mesh.vertexCount];
            Vector3[] localVertices = mesh.vertices;
            for (int i = 0; i < localVertices.Length; i++)
            {
                worldVertices[i] = render.transform.TransformPoint(localVertices[i]);
            }

            var entry = new MeshEntry
            {
                mesh = mesh,
                transform = render.transform,
                vertexCount = mesh.vertexCount,
                worldVertices = worldVertices
            };

            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                entry.subs.Add(new SubInfo
                {
                    subMeshIndex = s,
                    material = mats[s]
                });
            }

            entries.Add(entry);
        }
    }

    void SplitMesh(List<MeshEntry> entries)
    {
        var worldToLocal = transform.worldToLocalMatrix;
        var groups = new List<List<MeshEntry>>();
        var current = new List<MeshEntry>();
        int sum = 0;

        foreach (var e in entries)
        {
            if (current.Count > 0 && sum + e.vertexCount > maxVerticesPerMesh)
            {
                groups.Add(current);
                current = new List<MeshEntry>();
                sum = 0;
            }
            current.Add(e);
            sum += e.vertexCount;
        }
        if (current.Count > 0) groups.Add(current);

        var createdNames = new HashSet<string>();
        for (int i = 0; i < groups.Count; i++)
        {
            string name = "Combined_" + i;
            CreateCombined(name, groups[i], worldToLocal);
            createdNames.Add(name);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var obj = transform.GetChild(i);
            if (createdNames.Contains(obj.name)) continue;
            if (obj.GetComponent<WallTile>() == null || !obj.gameObject.activeSelf) Destroy(obj.gameObject);
        }
    }

    void CreateCombined(string groupName, List<MeshEntry> group, Matrix4x4 parentWorldToLocal)
    {
        var combineListByMaterial = new Dictionary<Material, List<CombineInstance>>();

        // Tìm bounding box của toàn bộ group trong không gian thế giới
        Bounds worldBounds = new Bounds();
        bool firstBounds = true;

        foreach (var entry in group)
        {
            foreach (var vertex in entry.worldVertices)
            {
                if (firstBounds)
                {
                    worldBounds = new Bounds(vertex, Vector3.zero);
                    firstBounds = false;
                }
                else
                {
                    worldBounds.Encapsulate(vertex);
                }
            }
        }

        foreach (var entry in group)
        {
            var localMatrix = parentWorldToLocal * entry.transform.localToWorldMatrix;

            foreach (var sub in entry.subs)
            {
                var cb = new CombineInstance
                {
                    mesh = entry.mesh,
                    subMeshIndex = sub.subMeshIndex,
                    transform = localMatrix
                };

                // Sử dụng overrideMaterial nếu có, nếu không dùng material gốc
                Material materialToUse = overrideMaterial != null ? overrideMaterial : sub.material;

                if (!combineListByMaterial.TryGetValue(materialToUse, out var list))
                {
                    list = new List<CombineInstance>();
                    combineListByMaterial[materialToUse] = list;
                }
                list.Add(cb);
            }
        }

        // Nếu có overrideMaterial, chỉ dùng một material
        if (overrideMaterial != null && combineListByMaterial.Count > 1)
        {
            var allCombines = new List<CombineInstance>();
            foreach (var list in combineListByMaterial.Values)
            {
                allCombines.AddRange(list);
            }

            combineListByMaterial.Clear();
            combineListByMaterial[overrideMaterial] = allCombines;
        }

        var tempMeshes = new List<Mesh>();
        var materials = new List<Material>();

        foreach (var kvp in combineListByMaterial)
        {
            var temp = new Mesh();
            temp.CombineMeshes(kvp.Value.ToArray(), true, true, false);

            // Tính toán lại UV dựa trên bounding box
            if (regenerateUV)
            {
                RegenerateUV(temp, worldBounds, parentWorldToLocal);
            }

            tempMeshes.Add(temp);
            materials.Add(kvp.Key);
        }

        var finalCombiners = new List<CombineInstance>();
        foreach (var tm in tempMeshes)
        {
            finalCombiners.Add(new CombineInstance
            {
                mesh = tm,
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            });
        }

        var go = new GameObject(groupName);
        go.layer = LayerMask.NameToLayer("Wall");
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.SetAsFirstSibling();
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;

        var mfOut = go.AddComponent<MeshFilter>();
        var mrOut = go.AddComponent<MeshRenderer>();

        var finalMesh = new Mesh();
        bool mergeSubMeshes = (overrideMaterial != null) || (materials.Count == 1);
        finalMesh.CombineMeshes(finalCombiners.ToArray(), mergeSubMeshes, true, false);

        mfOut.sharedMesh = finalMesh;
        mrOut.sharedMaterials = materials.ToArray();

        foreach (var tm in tempMeshes)
        {
            Destroy(tm);
        }
    }

    void RegenerateUV(Mesh mesh, Bounds worldBounds, Matrix4x4 parentWorldToLocal)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        // Chuyển đổi vertices từ local space của mesh combined về world space
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = parentWorldToLocal.inverse.MultiplyPoint(vertices[i]);
            Vector2 uv = CalculateUV(worldPos, worldBounds);
            uvs[i] = uv;
        }

        mesh.uv = uvs;
    }

    Vector2 CalculateUV(Vector3 worldPosition, Bounds bounds)
    {
        Vector2 uv = Vector2.zero;

        switch (uvAxis)
        {
            case UVAxis.XY:
                uv.x = (worldPosition.x - bounds.min.x) / bounds.size.x;
                uv.y = (worldPosition.y - bounds.min.y) / bounds.size.y;
                break;

            case UVAxis.XZ:
                uv.x = (worldPosition.x - bounds.min.x) / bounds.size.x;
                uv.y = (worldPosition.z - bounds.min.z) / bounds.size.z;
                break;

            case UVAxis.YZ:
                uv.x = (worldPosition.y - bounds.min.y) / bounds.size.y;
                uv.y = (worldPosition.z - bounds.min.z) / bounds.size.z;
                break;
        }

        // Áp dụng tỷ lệ và offset
        uv.x = uv.x * uvPerUnit + uvOffset.x;
        uv.y = uv.y * uvPerUnit + uvOffset.y;

        return uv;
    }
}