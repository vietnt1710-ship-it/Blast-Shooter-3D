using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class VoxelLayerPainter : MonoBehaviour
{
    // Group Cube by Y
    [System.Serializable]
    public class LayerGroup
    {
        public int yIndex;
        public List<CubeData> cubes = new List<CubeData>();
    }

    [System.Serializable]
    public class CubeData
    {
        public int xIndex, zIndex;
        [HideInInspector] public Transform cube;
        public Color32 color;
    }

    [Header("Grouped Output")]
    public List<LayerGroup> groupedByY = new List<LayerGroup>();
    [HideInInspector] public Vector3 lastOrigin;
    [HideInInspector] public int lastNx, lastNy, lastNz;

    public void SaveLastOrigin(Vector3 origin, int nx, int ny, int nz)
    {
        lastOrigin = origin;
        lastNx = nx; lastNy = ny; lastNz = nz;
    }

    [Header("Palette (Unique Colors)")]
    public List<Color32> uniqueColors = new List<Color32>();

    [Header("Count cubes per unique color")]
    public List<int> cubeVerUniqueColors = new List<int>(); // <- cái bạn cần
    
    [Header("Button")]
    public Transform colorButtonPanel;
    public Button buttonColor;
    public Image selectedColorImage;

    [Header("Camera")]
    public Transform cameraRoot;

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public Vector2 pitchClamp = new Vector2(-80f, 80f);

    float _yaw;
    float _pitch;

    [Header("Editor color")]
    public LayerMask paintMask = ~0;
    public float rayDistance = 1000f;
    MaterialPropertyBlock _mpb;

    [Header("Layer Selector")]
    public TMP_Dropdown dropdown;
    public Button Btn_LoadAll;
    public int selectedLayerIndex = 0;

    [Header("Refs")]
    public VoxelizerPro voxelizerPro;
    public LevelData data;

    int currentIndex = -1;
    Color32 currentColor;

    // ====== internal for fast count update ======
    public List<Button> _colorButtons = new List<Button>();
    readonly Dictionary<uint, int> _paletteIndex = new Dictionary<uint, int>(256);

    static uint Pack(Color32 c) => (uint)(c.r | (c.g << 8) | (c.b << 16) | (c.a << 24));

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        if (Btn_LoadAll != null)
        {
            Btn_LoadAll.onClick.RemoveListener(LoadAll);
            Btn_LoadAll.onClick.AddListener(LoadAll);
        }
    }

    void OnEnable()
    {
        if (cameraRoot != null)
        {
            var e = cameraRoot.eulerAngles;
            _yaw = e.y;
            _pitch = e.x;
        }
    }

    void Update()
    {
        if (cameraRoot != null && Input.GetMouseButton(1))
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _yaw += mx;
            _pitch -= my;
            _pitch = Mathf.Clamp(_pitch, pitchClamp.x, pitchClamp.y);

            cameraRoot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        // PAINT: update BOTH renderer + DATA + COUNTS
        if (Input.GetMouseButton(0) && currentIndex != -1)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, paintMask, QueryTriggerInteraction.Ignore))
            {
                var r = hit.collider.GetComponent<Renderer>();
                if (r == null) return;

                var tag = hit.collider.GetComponent<VoxelCubeTag>();
                if (tag == null || tag.data == null) return;

                // old/new color
                Color32 oldColor = tag.data.color;
                Color32 newColor = currentColor;

                // nếu cùng màu thì thôi
                if (Pack(oldColor) == Pack(newColor)) return;

                // 1) Update DATA
                tag.data.color = newColor;

                // 2) Update Renderer
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", (Color)newColor);
                _mpb.SetColor("_Color", (Color)newColor);
                r.SetPropertyBlock(_mpb);

                // 3) Update counts + button text realtime
                UpdateCountsAfterPaint(oldColor, newColor);
            }
        }
    }

    // ===================== UI =====================

    public void LoadUI()
    {
        BuildPaletteIndex();
        RecountCubesPerColor();

        ClearColorButtons();
        LoadColorButton();      // tạo button + set text = count
        InitWhenDropDown();
    }

    void BuildPaletteIndex()
    {
        _paletteIndex.Clear();
        for (int i = 0; i < uniqueColors.Count; i++)
        {
            uint k = Pack(uniqueColors[i]);
            if (!_paletteIndex.ContainsKey(k))
                _paletteIndex.Add(k, i);
        }

        // đảm bảo list count đúng size
        if (cubeVerUniqueColors == null) cubeVerUniqueColors = new List<int>();
        cubeVerUniqueColors.Clear();
        for (int i = 0; i < uniqueColors.Count; i++) cubeVerUniqueColors.Add(0);
    }

    void RecountCubesPerColor()
    {
        // reset
        for (int i = 0; i < cubeVerUniqueColors.Count; i++)
            cubeVerUniqueColors[i] = 0;

        // count theo groupedByY
        for (int i = 0; i < groupedByY.Count; i++)
        {
            var cubes = groupedByY[i].cubes;
            if (cubes == null) continue;

            for (int j = 0; j < cubes.Count; j++)
            {
                var cd = cubes[j];
                uint k = Pack(cd.color);
                if (_paletteIndex.TryGetValue(k, out int idx))
                    cubeVerUniqueColors[idx]++;
            }
        }
    }

    void ClearColorButtons()
    {
        _colorButtons.Clear();
        if (colorButtonPanel == null) return;

        // xoá tất cả con trong panel (trừ template nếu bạn đang để template nằm ngoài panel)
        for (int i = colorButtonPanel.childCount - 1; i >= 0; i--)
            Destroy(colorButtonPanel.GetChild(i).gameObject);
    }

    public void LoadColorButton()
    {
        if (buttonColor == null || colorButtonPanel == null) return;

        for (int i = 0; i < uniqueColors.Count; i++)
        {
            Button b = Instantiate(buttonColor, colorButtonPanel);
            b.transform.localScale = Vector3.one;
            b.gameObject.SetActive(true);

            b.image.color = uniqueColors[i];

            // set text count
            var txt = b.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
                txt.text = cubeVerUniqueColors != null && i < cubeVerUniqueColors.Count
                    ? cubeVerUniqueColors[i].ToString()
                    : "0";

            int index = i;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => ChangeColor(index));

            _colorButtons.Add(b);
            
            var img = b.GetComponent<Image>();
            if (img != null) img.enabled = true;

            var btn = b.GetComponent<Button>();
            if (btn != null) btn.enabled = true;
            
            if (txt != null)
            {
                txt.enabled = true;
                txt.gameObject.SetActive(true);
            }
        }
    }

    public void ChangeColor(int index)
    {
        if (index < 0 || index >= uniqueColors.Count) return;

        currentIndex = index;
        currentColor = uniqueColors[index];

        if (selectedColorImage != null)
            selectedColorImage.color = uniqueColors[index];
    }

    // Update count sau khi paint đổi 1 cube từ old -> new
    void UpdateCountsAfterPaint(Color32 oldColor, Color32 newColor)
    {
        uint ok = Pack(oldColor);
        uint nk = Pack(newColor);

        if (!_paletteIndex.TryGetValue(ok, out int oldIdx)) oldIdx = -1;
        if (!_paletteIndex.TryGetValue(nk, out int newIdx)) newIdx = -1;

        if (oldIdx >= 0 && oldIdx < cubeVerUniqueColors.Count)
        {
            cubeVerUniqueColors[oldIdx] = Mathf.Max(0, cubeVerUniqueColors[oldIdx] - 1);
            UpdateButtonText(oldIdx);
        }

        if (newIdx >= 0 && newIdx < cubeVerUniqueColors.Count)
        {
            cubeVerUniqueColors[newIdx] += 1;
            UpdateButtonText(newIdx);
        }
    }

    void UpdateButtonText(int paletteIndex)
    {
        if (paletteIndex < 0 || paletteIndex >= _colorButtons.Count) return;

        var b = _colorButtons[paletteIndex];
        if (b == null) return;

        var txt = b.GetComponentInChildren<TMP_Text>(true);
        if (txt != null)
            txt.text = cubeVerUniqueColors[paletteIndex].ToString();
    }

    // ===================== Layers dropdown =====================

    void InitWhenDropDown()
    {
        if (dropdown == null) return;

        BuildOptions();

        dropdown.onValueChanged.RemoveListener(OnChanged);
        dropdown.onValueChanged.AddListener(OnChanged);

        dropdown.value = Mathf.Clamp(selectedLayerIndex, 0, dropdown.options.Count - 1);
        dropdown.RefreshShownValue();

        OnChanged(dropdown.value);
    }

    void BuildOptions()
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();

        var opts = new List<string>(groupedByY.Count);
        for (int i = 0; i < groupedByY.Count; i++)
        {
            var g = groupedByY[i];
            int c = (g.cubes != null) ? g.cubes.Count : 0;
            opts.Add($"Layer {i} (y={g.yIndex}) - {c} cubes");
        }
        dropdown.AddOptions(opts);
    }

    void OnChanged(int index)
    {
        selectedLayerIndex = index;

        for (int i = 0; i < groupedByY.Count; i++)
        {
            bool active = (i == index);
            var cubes = groupedByY[i].cubes;
            if (cubes == null) continue;

            foreach (var cd in cubes)
                if (cd != null && cd.cube != null)
                    cd.cube.gameObject.SetActive(active);
        }
    }

    void LoadAll()
    {
        for (int i = 0; i < groupedByY.Count; i++)
        {
            var cubes = groupedByY[i].cubes;
            if (cubes == null) continue;

            foreach (var cd in cubes)
                if (cd != null && cd.cube != null)
                    cd.cube.gameObject.SetActive(true);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Export LevelData (Create New Asset)")]
    public void ExportToNewLevelDataAsset()
    {
        const string folder = "Assets/GameConfig";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameConfig"))
                AssetDatabase.CreateFolder("Assets", "GameConfig");
        }

        LevelData newData = ScriptableObject.CreateInstance<LevelData>();
        ExportToLevelData(newData);

        string fileName = $"{gameObject.name}_LevelData_{System.DateTime.Now:yyyyMMdd_HHmmss}.asset";
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, fileName));

        AssetDatabase.CreateAsset(newData, assetPath);
        EditorUtility.SetDirty(newData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Exported LevelData to: {assetPath}");
        Selection.activeObject = newData;
        EditorGUIUtility.PingObject(newData);
    }

    [ContextMenu("Load LevelData (Use 'data' field)")]
    public void LoadLevelDataAsset()
    {
        LoadDataToRuntime(this.data);
    }
#endif

    public void ExportToLevelData(LevelData dst)
    {
        if (dst == null) return;
        if (voxelizerPro != null) dst.voxelSize = voxelizerPro.voxelSize;

        dst.origin = lastOrigin;

        dst.colors.Clear();
        for (int i = 0; i < uniqueColors.Count; i++)
            dst.colors.Add((Color)uniqueColors[i]);

        dst.voxelData.Clear();
        for (int i = 0; i < groupedByY.Count; i++)
        {
            var src = groupedByY[i];
            var g = new LayerGroup { yIndex = src.yIndex, cubes = new List<CubeData>(src.cubes.Count) };

            for (int j = 0; j < src.cubes.Count; j++)
            {
                var c = src.cubes[j];
                g.cubes.Add(new CubeData
                {
                    xIndex = c.xIndex,
                    zIndex = c.zIndex,
                    color = c.color,
                    cube = null
                });
            }

            dst.voxelData.Add(g);
        }
    }

    public void LoadDataToRuntime(LevelData src)
    {
        if (src == null) return;

        uniqueColors.Clear();
        for (int i = 0; i < src.colors.Count; i++)
            uniqueColors.Add((Color32)src.colors[i]);

        groupedByY.Clear();
        for (int i = 0; i < src.voxelData.Count; i++)
        {
            var layer = src.voxelData[i];
            var g = new LayerGroup { yIndex = layer.yIndex, cubes = new List<CubeData>(layer.cubes.Count) };

            for (int j = 0; j < layer.cubes.Count; j++)
            {
                var c = layer.cubes[j];
                g.cubes.Add(new CubeData
                {
                    xIndex = c.xIndex,
                    zIndex = c.zIndex,
                    color = c.color,
                    cube = null
                });
            }

            groupedByY.Add(g);
        }

        groupedByY.Sort((a, b) => a.yIndex.CompareTo(b.yIndex));

        SpawnVoxelsFromLevelData(src);

        LoadUI(); // <- rebuild buttons + counts + dropdown
    }

    public void SpawnVoxelsFromLevelData(LevelData src)
    {
        if (src == null || voxelizerPro == null || voxelizerPro.cubePrefab == null)
        {
            Debug.LogError("Missing data or cubePrefab.");
            return;
        }

        if (voxelizerPro.outputRoot == null)
        {
            var go = new GameObject(name + "_Voxels");
            go.transform.SetParent(transform, false);
            voxelizerPro.outputRoot = go.transform;
        }

        voxelizerPro.ClearOutput();

        float vs = (src.voxelSize > 0f) ? src.voxelSize : voxelizerPro.voxelSize;
        Vector3 org = (src.origin != Vector3.zero) ? src.origin : lastOrigin;

        var mpb = new MaterialPropertyBlock();
        Vector3 scale = Vector3.one * (vs * voxelizerPro.cubeFill);

        var layerMap = new Dictionary<int, LayerGroup>();
        groupedByY.Clear();

        foreach (var layer in src.voxelData)
        {
            int y = layer.yIndex;

            if (!layerMap.TryGetValue(y, out var g))
            {
                g = new LayerGroup { yIndex = y, cubes = new List<CubeData>() };
                layerMap[y] = g;
            }

            foreach (var cdSrc in layer.cubes)
            {
                Vector3 pos = org + new Vector3((cdSrc.xIndex + 0.5f) * vs, (y + 0.5f) * vs, (cdSrc.zIndex + 0.5f) * vs);

                var go = Instantiate(voxelizerPro.cubePrefab, pos, Quaternion.identity, voxelizerPro.outputRoot);
                go.transform.localScale = scale;

                var cd = new CubeData
                {
                    xIndex = cdSrc.xIndex,
                    zIndex = cdSrc.zIndex,
                    color = cdSrc.color,
                    cube = go.transform
                };
                g.cubes.Add(cd);

                // tag để paint update data
                var tag = go.GetComponent<VoxelCubeTag>();
                if (tag == null) tag = go.AddComponent<VoxelCubeTag>();
                tag.data = cd;
                tag.yIndex = y;

                var r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    if (voxelizerPro.voxelMaterial != null) r.sharedMaterial = voxelizerPro.voxelMaterial;

                    mpb.Clear();
                    mpb.SetColor("_BaseColor", (Color)cd.color);
                    mpb.SetColor("_Color", (Color)cd.color);
                    r.SetPropertyBlock(mpb);
                }
            }
        }

        groupedByY.AddRange(layerMap.Values);
        groupedByY.Sort((a, b) => a.yIndex.CompareTo(b.yIndex));
    }
}
