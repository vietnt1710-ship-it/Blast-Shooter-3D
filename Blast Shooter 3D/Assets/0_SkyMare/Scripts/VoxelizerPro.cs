using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static VoxelLayerPainter;

public class VoxelizerPro : MonoBehaviour
{
    public Transform sourceRoot;

    [Header("Voxel")]
    [Min(0.001f)] public float voxelSize = 0.05f;

    [Tooltip("Thu nhỏ cube để tạo khe/đường lưới giữa các voxel (giống hình #3). 0.96 là đẹp phổ biến.")]
    [Range(0.85f, 1f)] public float cubeFill = 0.96f;

    [Tooltip("Inset nhỏ để overlap bớt 'dính' khi chỉ chạm nhẹ. Thường 0.02*voxelSize là ổn.")]
    [Range(0f, 0.25f)] public float overlapInsetRatio = 0.02f;

    [Header("Grid Anchor")]
    public bool anchorToBoundsCenter = true;
    public Vector3 gridOffset = Vector3.zero;
    public int paddingVoxels = 1;

    // =========================
    // LAYERS / PRIORITY (numeric)
    // type = index+1, 0 = Empty
    // =========================
    [System.Serializable]
    public class LayerRule
    {
        public string name;
        public LayerMask mask;
    }

    [Header("Layers / Priority (top = highest)")]
    [Tooltip("Ưu tiên theo thứ tự trong list: phần tử ở trên cùng ưu tiên cao nhất.\nVoxel type = ruleIndex+1 (0 là Empty).")]
    public List<LayerRule> layerRules = new List<LayerRule>();

    [Header("Center Check (optional)")]
    [Tooltip("Nếu voxel 'chạm' rule này, sẽ check thêm hình cầu ở tâm voxel. Fail thì fallback sang rule khác (theo priority). -1 = tắt.")]
    public int centerCheckRuleIndex = 0;

    [Tooltip("0.2 nghĩa là radius = 0.2*voxelSize.")]
    [Range(0f, 0.49f)] public float centerCheckRadiusRatio = 0.20f;

    [Header("Post process")]
    [Tooltip("Số vòng lấp lỗ (1-3 thường đủ).")]
    [Range(0, 5)] public int fillHolesIterations = 2;

    [Tooltip("Ngưỡng hàng xóm (trong 26 ô) để lấp một ô trống. 18-22 khá 'chặt', ít bự form.")]
    [Range(0, 26)] public int fillHolesNeighborThreshold = 18;

    [Tooltip("Loại voxel đơn lẻ (nhiễu). 0=tắt. 1 là hay dùng.")]
    [Range(0, 6)] public int pruneIsolatedIfNeighborsLE = 1;

    [Header("Output")]
    public bool surfaceOnly = true;
    public Transform outputRoot;
    public GameObject cubePrefab;

    [Tooltip("Material phải hỗ trợ _BaseColor (URP Lit/Unlit) hoặc _Color (Standard).")]
    public Material voxelMaterial;

    // Editor
    [Header("Editor")]
    public VoxelLayerPainter voxelLayerPainter;

    [Header("Solid Fill")]
    public bool fillInteriorSolid = true;

    // Internal
    private readonly Collider[] _hits = new Collider[128];

    private struct Voxel
    {
        public ushort type;     // 0 = Empty, 1..N = ruleIndex+1
        public Color32 color;
    }

    private void Start()
    {
        Generate();
        if (sourceRoot != null) sourceRoot.gameObject.SetActive(false);
    }

    [ContextMenu("Generate Voxels (Pro)")]
    public void Generate()
    {
        if (outputRoot != null) outputRoot.gameObject.SetActive(true);
        if (sourceRoot != null) sourceRoot.gameObject.SetActive(true);

        if (sourceRoot == null || cubePrefab == null)
        {
            Debug.LogError("Assign sourceRoot and cubePrefab.");
            return;
        }

        if (layerRules == null || layerRules.Count == 0)
        {
            Debug.LogError("layerRules is empty. Add at least 1 rule.");
            return;
        }

        if (outputRoot == null)
        {
            var go = new GameObject(sourceRoot.name + "_Voxels");
            go.transform.SetParent(transform, false);
            outputRoot = go.transform;
        }

        ClearOutput();

        Physics.SyncTransforms();

        var colliders = sourceRoot.GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
        {
            Debug.LogError("No Colliders found under sourceRoot. Add MeshCollider to source objects.");
            return;
        }

        Bounds b = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++) b.Encapsulate(colliders[i].bounds);

        // Padding
        float pad = paddingVoxels * voxelSize;
        b.Expand(new Vector3(pad, pad, pad) * 2f);

        // Compute snapped grid
        Vector3 origin;
        Vector3 size = b.size;

        int nx = Mathf.CeilToInt(size.x / voxelSize);
        int ny = Mathf.CeilToInt(size.y / voxelSize);
        int nz = Mathf.CeilToInt(size.z / voxelSize);

        Vector3 snappedSize = new Vector3(nx, ny, nz) * voxelSize;

        if (anchorToBoundsCenter)
            origin = b.center - snappedSize * 0.5f;
        else
            origin = b.min;

        origin += gridOffset;

        int Count = nx * ny * nz;
        var voxels = new Voxel[Count];

        float inset = voxelSize * overlapInsetRatio;
        Vector3 halfExt = Vector3.one * (voxelSize * 0.5f - inset);

        float centerCheckRadius = voxelSize * centerCheckRadiusRatio;

        int Idx(int x, int y, int z) => x + nx * (y + ny * z);

        Vector3 CenterOf(int x, int y, int z)
            => origin + new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize, (z + 0.5f) * voxelSize);

        if (voxelLayerPainter != null)
            voxelLayerPainter.SaveLastOrigin(origin, nx, ny, nz);

        // Build masks + priority map
        int allMask = 0;

        // map layer -> best rule index (nhỏ hơn = ưu tiên cao hơn)
        int[] bestRuleIndexByLayer = new int[32];
        for (int i = 0; i < 32; i++) bestRuleIndexByLayer[i] = int.MaxValue;

        for (int ri = 0; ri < layerRules.Count; ri++)
        {
            var rule = layerRules[ri];
            allMask |= rule.mask.value;

            int m = rule.mask.value;
            for (int layer = 0; layer < 32; layer++)
            {
                if ((m & (1 << layer)) != 0)
                {
                    if (ri < bestRuleIndexByLayer[layer])
                        bestRuleIndexByLayer[layer] = ri;
                }
            }
        }

        // Optional center-check rule mask
        int centerCheckMask = 0;
        bool centerCheckEnabled =
            centerCheckRuleIndex >= 0 &&
            centerCheckRuleIndex < layerRules.Count &&
            centerCheckRadius > 0f;

        if (centerCheckEnabled)
            centerCheckMask = layerRules[centerCheckRuleIndex].mask.value;

        // ---- Pass 1: classify
        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    Vector3 c = CenterOf(x, y, z);

                    int hitCount = Physics.OverlapBoxNonAlloc(
                        c, halfExt, _hits, Quaternion.identity, allMask, QueryTriggerInteraction.Ignore);

                    if (hitCount <= 0) continue;

                    // Determine best hit by priority (order in layerRules)
                    Collider best = null;
                    int bestRule = int.MaxValue;

                    for (int i = 0; i < hitCount; i++)
                    {
                        var col = _hits[i];
                        if (col == null) continue;

                        int layer = col.gameObject.layer;
                        int ri = bestRuleIndexByLayer[layer];
                        if (ri == int.MaxValue) continue;

                        if (ri < bestRule)
                        {
                            bestRule = ri;
                            best = col;
                            if (bestRule == 0) break;
                        }
                    }

                    if (best == null) continue;

                    // Optional "anti-bloat": require center sphere check for a chosen rule
                    if (centerCheckEnabled && bestRule == centerCheckRuleIndex && centerCheckMask != 0)
                    {
                        if (!Physics.CheckSphere(c, centerCheckRadius, centerCheckMask, QueryTriggerInteraction.Ignore))
                        {
                            // fallback: pick best rule but SKIP centerCheckRuleIndex
                            Collider best2 = null;
                            int bestRule2 = int.MaxValue;

                            for (int i = 0; i < hitCount; i++)
                            {
                                var col = _hits[i];
                                if (col == null) continue;

                                int layer = col.gameObject.layer;
                                int ri = bestRuleIndexByLayer[layer];
                                if (ri == int.MaxValue) continue;
                                if (ri == centerCheckRuleIndex) continue;

                                if (ri < bestRule2)
                                {
                                    bestRule2 = ri;
                                    best2 = col;
                                    if (bestRule2 == 0) break;
                                }
                            }

                            if (best2 == null) continue;
                            best = best2;
                            bestRule = bestRule2;
                        }
                    }

                    // Copy color from source Renderer material
                    Color32 col32 = new Color32(255, 255, 255, 255);
                    var rend = best.GetComponentInParent<Renderer>();
                    if (rend != null && rend.sharedMaterial != null)
                    {
                        if (rend.sharedMaterial.HasProperty("_BaseColor"))
                            col32 = (Color32)rend.sharedMaterial.GetColor("_BaseColor");
                        else if (rend.sharedMaterial.HasProperty("_Color"))
                            col32 = (Color32)rend.sharedMaterial.GetColor("_Color");
                    }

                    voxels[Idx(x, y, z)] = new Voxel
                    {
                        type = (ushort)(bestRule + 1), // 1..N
                        color = col32
                    };
                }

        // ---- Post: fill holes (majority in 26 neighbors)
        if (fillHolesIterations > 0)
        {
            var neighborOffsets = Build26Neighbors();
            int ruleCount = layerRules.Count;

            for (int it = 0; it < fillHolesIterations; it++)
            {
                var next = (Voxel[])voxels.Clone();

                for (int z = 0; z < nz; z++)
                    for (int y = 0; y < ny; y++)
                        for (int x = 0; x < nx; x++)
                        {
                            int id = Idx(x, y, z);
                            if (voxels[id].type != 0) continue; // not empty

                            int[] counts = new int[ruleCount + 1]; // index = type (0..N)
                            ushort chosenType = 0;
                            Color32 chosenColor = default;

                            int filled = 0;

                            foreach (var o in neighborOffsets)
                            {
                                int xx = x + o.x;
                                int yy = y + o.y;
                                int zz = z + o.z;
                                if (xx < 0 || yy < 0 || zz < 0 || xx >= nx || yy >= ny || zz >= nz) continue;

                                var v = voxels[Idx(xx, yy, zz)];
                                if (v.type == 0) continue;

                                filled++;
                                if (v.type <= ruleCount) counts[v.type]++;
                            }

                            if (filled < fillHolesNeighborThreshold) continue;

                            // majority type, tie -> smaller type (higher priority)
                            int bestC = -1;
                            for (ushort t = 1; t <= ruleCount; t++)
                            {
                                int c = counts[t];
                                if (c > bestC || (c == bestC && c > 0 && (chosenType == 0 || t < chosenType)))
                                {
                                    bestC = c;
                                    chosenType = t;
                                }
                            }

                            if (chosenType == 0) continue;

                            // pick color from first neighbor of that type
                            foreach (var o in neighborOffsets)
                            {
                                int xx = x + o.x;
                                int yy = y + o.y;
                                int zz = z + o.z;
                                if (xx < 0 || yy < 0 || zz < 0 || xx >= nx || yy >= ny || zz >= nz) continue;

                                var v = voxels[Idx(xx, yy, zz)];
                                if (v.type == chosenType)
                                {
                                    chosenColor = v.color;
                                    break;
                                }
                            }

                            next[id] = new Voxel { type = chosenType, color = chosenColor };
                        }

                voxels = next;
            }
        }

        // ---- Post: prune isolated (6-neighbor)
        if (pruneIsolatedIfNeighborsLE > 0)
        {
            var next = (Voxel[])voxels.Clone();
            var neighbor6 = Build6Neighbors();

            for (int z = 0; z < nz; z++)
                for (int y = 0; y < ny; y++)
                    for (int x = 0; x < nx; x++)
                    {
                        int id = Idx(x, y, z);
                        if (voxels[id].type == 0) continue;

                        int n = 0;
                        foreach (var o in neighbor6)
                        {
                            int xx = x + o.x;
                            int yy = y + o.y;
                            int zz = z + o.z;
                            if (xx < 0 || yy < 0 || zz < 0 || xx >= nx || yy >= ny || zz >= nz) continue;

                            if (voxels[Idx(xx, yy, zz)].type != 0) n++;
                        }

                        if (n <= pruneIsolatedIfNeighborsLE)
                            next[id] = default; // empty
                    }

            voxels = next;
        }

        if (fillInteriorSolid)
        {
            FillInteriorSolid(voxels, nx, ny, nz, layerRules.Count);
        }

        var colorSet = new HashSet<uint>();
        var layerMap = new Dictionary<int, LayerGroup>(ny);

        var mpb = new MaterialPropertyBlock();
        Vector3 scale = Vector3.one * (voxelSize * cubeFill);

        // reset group data
        if (voxelLayerPainter != null)
            voxelLayerPainter.groupedByY.Clear();

        // ---- Spawn cubes
        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    int id = Idx(x, y, z);
                    var v = voxels[id];
                    if (v.type == 0) continue;

                    if (surfaceOnly && !IsSurface(voxels, nx, ny, nz, x, y, z, Idx))
                        continue;

                    Vector3 c = CenterOf(x, y, z);

                    uint key = (uint)(v.color.r | (v.color.g << 8) | (v.color.b << 16) | (v.color.a << 24));
                    colorSet.Add(key);

                    var go = Instantiate(cubePrefab, c, Quaternion.identity, outputRoot);
                    go.transform.localScale = scale;

                    // add group by y
                    if (!layerMap.TryGetValue(y, out var g))
                    {
                        g = new LayerGroup { yIndex = y, cubes = new List<CubeData>() };
                        layerMap[y] = g;
                    }

                    // IMPORTANT: CubeData là class -> reference sẽ update được khi paint
                    var cd = new CubeData
                    {
                        xIndex = x,
                        zIndex = z,
                        cube = go.transform,
                        color = v.color
                    };
                    g.cubes.Add(cd);

                    // Tag để khi click biết CubeData nào cần update
                    var tag = go.GetComponent<VoxelCubeTag>();
                    if (tag == null) tag = go.AddComponent<VoxelCubeTag>();
                    tag.data = cd;
                    tag.yIndex = y;

                    var r = go.GetComponent<Renderer>();
                    if (r != null)
                    {
                        if (voxelMaterial != null) r.sharedMaterial = voxelMaterial;

                        mpb.Clear();
                        mpb.SetColor("_BaseColor", v.color);
                        mpb.SetColor("_Color", v.color);
                        r.SetPropertyBlock(mpb);
                    }
                }

        if (voxelLayerPainter != null)
        {
            voxelLayerPainter.groupedByY.AddRange(layerMap.Values);
            voxelLayerPainter.groupedByY.Sort((a, b) => a.yIndex.CompareTo(b.yIndex));

            voxelLayerPainter.uniqueColors.Clear();
            foreach (var k in colorSet)
            {
                voxelLayerPainter.uniqueColors.Add(new Color32(
                    (byte)(k & 255),
                    (byte)((k >> 8) & 255),
                    (byte)((k >> 16) & 255),
                    (byte)((k >> 24) & 255)
                ));
            }

            voxelLayerPainter.LoadUI(); // build UI fresh
        }
    }

    // Fill all interior empties (not connected to boundary) with a boundary-majority type (tie -> higher priority / smaller type)
    private static void FillInteriorSolid(Voxel[] voxels, int nx, int ny, int nz, int ruleCount)
    {
        int Idx(int x, int y, int z) => x + nx * (y + ny * z);
        int Count = nx * ny * nz;

        // 1) Flood fill OUTSIDE empty từ biên
        var outside = new bool[Count];
        var q = new Queue<int>(Count / 8);

        void TryEnqueueOutside(int x, int y, int z)
        {
            int id = Idx(x, y, z);
            if (outside[id]) return;
            if (voxels[id].type != 0) return;
            outside[id] = true;
            q.Enqueue(id);
        }

        // seed all boundary empties
        for (int x = 0; x < nx; x++)
            for (int y = 0; y < ny; y++)
            {
                TryEnqueueOutside(x, y, 0);
                TryEnqueueOutside(x, y, nz - 1);
            }

        for (int x = 0; x < nx; x++)
            for (int z = 0; z < nz; z++)
            {
                TryEnqueueOutside(x, 0, z);
                TryEnqueueOutside(x, ny - 1, z);
            }

        for (int y = 0; y < ny; y++)
            for (int z = 0; z < nz; z++)
            {
                TryEnqueueOutside(0, y, z);
                TryEnqueueOutside(nx - 1, y, z);
            }

        var n6 = Build6Neighbors();
        while (q.Count > 0)
        {
            int id = q.Dequeue();
            int z = id / (nx * ny);
            int rem = id - z * nx * ny;
            int y = rem / nx;
            int x = rem - y * nx;

            foreach (var d in n6)
            {
                int xx = x + d.x, yy = y + d.y, zz = z + d.z;
                if (xx < 0 || yy < 0 || zz < 0 || xx >= nx || yy >= ny || zz >= nz) continue;

                int nid = Idx(xx, yy, zz);
                if (outside[nid]) continue;
                if (voxels[nid].type != 0) continue;

                outside[nid] = true;
                q.Enqueue(nid);
            }
        }

        // 2) Inside components -> fill theo boundary majority
        var visited = new bool[Count];
        var comp = new List<int>(1024);
        var cq = new Queue<int>(1024);
        var n26 = Build26Neighbors();

        for (int z = 0; z < nz; z++)
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    int start = Idx(x, y, z);

                    if (outside[start]) continue;
                    if (visited[start]) continue;
                    if (voxels[start].type != 0) continue;

                    comp.Clear();
                    cq.Clear();

                    visited[start] = true;
                    cq.Enqueue(start);

                    int[] boundaryCounts = new int[ruleCount + 1];
                    Color32[] boundaryColor = new Color32[ruleCount + 1];
                    bool[] hasColor = new bool[ruleCount + 1];

                    while (cq.Count > 0)
                    {
                        int id = cq.Dequeue();
                        comp.Add(id);

                        int zz = id / (nx * ny);
                        int rem = id - zz * nx * ny;
                        int yy = rem / nx;
                        int xx = rem - yy * nx;

                        foreach (var d in n6)
                        {
                            int x2 = xx + d.x, y2 = yy + d.y, z2 = zz + d.z;
                            if (x2 < 0 || y2 < 0 || z2 < 0 || x2 >= nx || y2 >= ny || z2 >= nz) continue;

                            int nid = Idx(x2, y2, z2);
                            if (outside[nid] || visited[nid]) continue;
                            if (voxels[nid].type != 0) continue;

                            visited[nid] = true;
                            cq.Enqueue(nid);
                        }

                        foreach (var o in n26)
                        {
                            int x2 = xx + o.x, y2 = yy + o.y, z2 = zz + o.z;
                            if (x2 < 0 || y2 < 0 || z2 < 0 || x2 >= nx || y2 >= ny || z2 >= nz) continue;

                            int nid = Idx(x2, y2, z2);
                            ushort t = voxels[nid].type;
                            if (t == 0 || t > ruleCount) continue;

                            boundaryCounts[t]++;

                            if (!hasColor[t])
                            {
                                boundaryColor[t] = voxels[nid].color;
                                hasColor[t] = true;
                            }
                        }
                    }

                    // choose boundary majority, tie -> smaller type (higher priority)
                    ushort chosen = 1;
                    int best = -1;

                    for (ushort t = 1; t <= ruleCount; t++)
                    {
                        int c = boundaryCounts[t];
                        if (c > best || (c == best && c > 0 && t < chosen))
                        {
                            best = c;
                            chosen = t;
                        }
                    }

                    Color32 col = hasColor[chosen]
                        ? boundaryColor[chosen]
                        : new Color32(255, 255, 255, 255);

                    foreach (var id in comp)
                        voxels[id] = new Voxel { type = chosen, color = col };
                }
    }

    private static bool IsSurface(Voxel[] voxels, int nx, int ny, int nz, int x, int y, int z, System.Func<int, int, int, int> idx)
    {
        if (voxels[idx(x, y, z)].type == 0) return false;

        if (x == 0 || voxels[idx(x - 1, y, z)].type == 0) return true;
        if (x == nx - 1 || voxels[idx(x + 1, y, z)].type == 0) return true;
        if (y == 0 || voxels[idx(x, y - 1, z)].type == 0) return true;
        if (y == ny - 1 || voxels[idx(x, y + 1, z)].type == 0) return true;
        if (z == 0 || voxels[idx(x, y, z - 1)].type == 0) return true;
        if (z == nz - 1 || voxels[idx(x, y, z + 1)].type == 0) return true;

        return false;
    }

    private static List<Vector3Int> Build6Neighbors() => new List<Vector3Int>
    {
        new Vector3Int(-1,0,0), new Vector3Int(1,0,0),
        new Vector3Int(0,-1,0), new Vector3Int(0,1,0),
        new Vector3Int(0,0,-1), new Vector3Int(0,0,1),
    };

    private static List<Vector3Int> Build26Neighbors()
    {
        var list = new List<Vector3Int>(26);
        for (int z = -1; z <= 1; z++)
            for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    list.Add(new Vector3Int(x, y, z));
                }
        return list;
    }

    [ContextMenu("Clear Output")]
    public void ClearOutput()
    {
        if (outputRoot == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = outputRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(outputRoot.GetChild(i).gameObject);
            return;
        }
#endif
        for (int i = outputRoot.childCount - 1; i >= 0; i--)
            Destroy(outputRoot.GetChild(i).gameObject);
    }
}
