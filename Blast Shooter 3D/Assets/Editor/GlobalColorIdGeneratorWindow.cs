using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalColorIdGeneratorWindow : EditorWindow
{
    private const string LevelRootPath = "Assets/GameConfig";
    private const string OutputFolder = "Assets/0_SkyMare/Data";
    private const string OutputAssetPath = OutputFolder + "/ColorID.asset";

    private bool clearLevelColorsAfter = false; // nếu muốn xoá LevelData.colors sau khi chuyển sang global
    private bool overwriteExisting = true;      // ghi đè ColorID.asset nếu đã tồn tại

    [MenuItem("Tools/SkyMare/Generate Global ColorID")]
    public static void Open()
    {
        GetWindow<GlobalColorIdGeneratorWindow>("Generate Global ColorID");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Scan LevelData and build a global ColorID palette", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("Input:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("LevelData root", LevelRootPath);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Output:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("ColorID asset", OutputAssetPath);

        EditorGUILayout.Space(10);
        overwriteExisting = EditorGUILayout.ToggleLeft("Overwrite existing ColorID.asset", overwriteExisting);
        clearLevelColorsAfter = EditorGUILayout.ToggleLeft("Clear LevelData.colors after remap (optional)", clearLevelColorsAfter);

        EditorGUILayout.Space(14);
        if (GUILayout.Button("Generate + Remap", GUILayout.Height(32)))
        {
            GenerateAndRemap();
        }
    }

    private static Color32 ToKey(Color c) => (Color32)c; // convert float color to byte-based key

    private void GenerateAndRemap()
    {
        // 1) Find all LevelData assets under Assets/GameConfig
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { LevelRootPath });
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning($"[GlobalColorID] No LevelData found under: {LevelRootPath}");
            return;
        }

        // 2) Build global unique palette
        var colorKeyToNewIndex = new Dictionary<Color32, int>();
        var palette = new List<Color32>(); // store as Color32 for stable uniqueness

        int totalLevels = 0;
        int totalColorsSeen = 0;

        // Load levels first (so we can do a second pass remap)
        var levels = new List<LevelData>(guids.Length);
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (level == null) continue;

            levels.Add(level);
            totalLevels++;

            if (level.colors == null) continue;

            for (int i = 0; i < level.colors.Count; i++)
            {
                totalColorsSeen++;
                var key = ToKey(level.colors[i]);

                if (!colorKeyToNewIndex.ContainsKey(key))
                {
                    colorKeyToNewIndex[key] = palette.Count;
                    palette.Add(key);
                }
            }
        }

        if (palette.Count == 0)
        {
            Debug.LogWarning("[GlobalColorID] No colors found in LevelData.colors.");
            return;
        }

        // 3) Ensure output folder exists
        EnsureFolder(OutputFolder);

        // 4) Create or overwrite ColorID asset
        ColorID colorIdAsset = AssetDatabase.LoadAssetAtPath<ColorID>(OutputAssetPath);

        if (colorIdAsset != null)
        {
            if (!overwriteExisting)
            {
                Debug.LogWarning($"[GlobalColorID] ColorID already exists and overwrite is OFF: {OutputAssetPath}");
                return;
            }

            // overwrite existing instance content
            Undo.RecordObject(colorIdAsset, "Overwrite ColorID");
        }
        else
        {
            colorIdAsset = ScriptableObject.CreateInstance<ColorID>();
            AssetDatabase.CreateAsset(colorIdAsset, OutputAssetPath);
        }

        // Fill ColorID.colorWithIDs
        if (colorIdAsset.colorWithIDs == null)
            colorIdAsset.colorWithIDs = new List<ColorWithID>();
        else
            colorIdAsset.colorWithIDs.Clear();

        for (int i = 0; i < palette.Count; i++)
        {
            colorIdAsset.colorWithIDs.Add(new ColorWithID
            {
                ID = i,                       // ID = index (bạn có thể đổi logic nếu cần)
                color = (Color)palette[i]
            });
        }

        EditorUtility.SetDirty(colorIdAsset);

        // 5) Remap each LevelData.colorIndex to point into global palette indices
        int remappedLevels = 0;
        int remappedIndices = 0;
        int invalidIndices = 0;

        foreach (var level in levels)
        {
            if (level == null) continue;

            bool changed = false;

            // Map old local color index -> new global palette index
            var oldToNew = new Dictionary<int, int>();

            if (level.colors != null)
            {
                for (int oldIdx = 0; oldIdx < level.colors.Count; oldIdx++)
                {
                    var key = ToKey(level.colors[oldIdx]);
                    if (colorKeyToNewIndex.TryGetValue(key, out int newIdx))
                        oldToNew[oldIdx] = newIdx;
                }
            }

            if (level.colorIndex != null)
            {
                for (int i = 0; i < level.colorIndex.Count; i++)
                {
                    int oldIdx = level.colorIndex[i];

                    if (!oldToNew.TryGetValue(oldIdx, out int newIdx))
                    {
                        invalidIndices++;
                        continue; // giữ nguyên nếu index lỗi / không map được
                    }

                    if (newIdx != oldIdx)
                    {
                        level.colorIndex[i] = newIdx;
                        changed = true;
                        remappedIndices++;
                    }
                }
            }

            if (clearLevelColorsAfter && level.colors != null && level.colors.Count > 0)
            {
                level.colors.Clear();
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(level);
                remappedLevels++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[GlobalColorID] Done.\n" +
            $"- Levels scanned: {totalLevels}\n" +
            $"- Total colors seen: {totalColorsSeen}\n" +
            $"- Unique colors (global palette): {palette.Count}\n" +
            $"- Levels changed: {remappedLevels}\n" +
            $"- Indices remapped: {remappedIndices}\n" +
            $"- Invalid/unmapped indices encountered: {invalidIndices}\n" +
            $"- Output: {OutputAssetPath}"
        );
    }

    private static void EnsureFolder(string folderPath)
    {
        // folderPath like "Assets/0_SkyMare/Data"
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        // Create nested folders progressively
        string[] parts = folderPath.Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }
}
