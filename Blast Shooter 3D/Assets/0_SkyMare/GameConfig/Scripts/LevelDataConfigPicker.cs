using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelDataConfigPicker : MonoBehaviour
{
    [Header("UI (Topdown khác)")]
    public TMP_Dropdown configDropdown;
    public Button btnLoadSelected;
    public Button btnRefresh; // optional

    [Header("Target")]
    public VoxelLayerPainter voxelLayerPainter;

    [Header("Auto Scan (Editor only)")]
    [Tooltip("Folder chứa LevelData assets. Ví dụ: Assets/GameConfig")]
    public string configFolder = "Assets/GameConfig";

    [Header("Found Configs")]
    [SerializeField] private List<LevelData> configs = new List<LevelData>();

    int _currentIndex = -1;
    public LevelData select;
    void Awake()
    {
        if (btnLoadSelected != null)
        {
            btnLoadSelected.onClick.RemoveListener(LoadSelected);
            btnLoadSelected.onClick.AddListener(LoadSelected);
        }

        if (btnRefresh != null)
        {
            btnRefresh.onClick.RemoveListener(RefreshList);
            btnRefresh.onClick.AddListener(RefreshList);
        }

        if (configDropdown != null)
        {
            configDropdown.onValueChanged.RemoveListener(OnDropdownChanged);
            configDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }
    }

    void Start()
    {
        RefreshList();
    }

    public void RefreshList()
    {
#if UNITY_EDITOR
        ScanAllLevelDataInFolder_Editor();
#endif
        RebuildDropdownOptions();
    }

#if UNITY_EDITOR
    void ScanAllLevelDataInFolder_Editor()
    {
        configs.Clear();

        if (string.IsNullOrEmpty(configFolder))
            configFolder = "Assets/GameConfig";

        // tìm tất cả asset kiểu LevelData trong folder
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { configFolder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (data != null) configs.Add(data);
        }

        // sort theo tên cho đẹp
        configs.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

        // mark dirty để list này được serialize (để build dùng được)
        EditorUtility.SetDirty(this);
#endif
    }

    void RebuildDropdownOptions()
    {
        if (configDropdown == null) return;

        configDropdown.ClearOptions();

        var opts = new List<string>(configs.Count);
        for (int i = 0; i < configs.Count; i++)
        {
            var d = configs[i];
            if (d == null) continue;

            // hiển thị: Name (layers)
            int layerCount = (d.voxelData != null) ? d.voxelData.Count : 0;
            opts.Add($"{d.name}  (layers: {layerCount})");
        }

        if (opts.Count == 0)
        {
            opts.Add("(No LevelData found)");
            _currentIndex = -1;
            configDropdown.AddOptions(opts);
            configDropdown.value = 0;
            configDropdown.RefreshShownValue();
            return;
        }

        configDropdown.AddOptions(opts);

        // default chọn item 0
        _currentIndex = 0;
        configDropdown.value = 0;
        configDropdown.RefreshShownValue();

        // gọi luôn on change để sync
        OnDropdownChanged(configDropdown.value);
    }

    void OnDropdownChanged(int index)
    {
        if (configs == null || configs.Count == 0)
        {
            _currentIndex = -1;
            return;
        }

        _currentIndex = Mathf.Clamp(index, 0, configs.Count - 1);
    }

    public void LoadSelected()
    {
        if (voxelLayerPainter == null)
        {
            Debug.LogError("[LevelDataConfigPicker] Missing voxelLayerPainter reference.");
            return;
        }

        if (configs == null || configs.Count == 0 || _currentIndex < 0 || _currentIndex >= configs.Count)
        {
            Debug.LogWarning("[LevelDataConfigPicker] No config selected.");
            return;
        }

        var data = configs[_currentIndex];
        if (data == null)
        {
            Debug.LogWarning("[LevelDataConfigPicker] Selected config is null.");
            return;
        }
        select = data;
        voxelLayerPainter.LoadDataToRuntime(data);
    }

    // tiện cho bạn bấm chuột phải test
    [ContextMenu("Editor Scan + Rebuild UI")]
    void CtxScan()
    {
        RefreshList();
    }

    // Cho phép script khác lấy list configs nếu cần
    public IReadOnlyList<LevelData> GetConfigs() => configs;
}
