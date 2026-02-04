using System.Collections.Generic;
using UnityEngine;

public class ShooterColor : MonoBehaviour
{
    // Map: material gốc -> material clone runtime
    private readonly Dictionary<Material, Material> cloneMap = new();
    private readonly List<Material> createdClones = new();

    public void ChangeColor(Color color)
    {
        var renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        foreach (var r in renderers)
        {
            if (r == null) continue;

            var shared = r.sharedMaterials;
            if (shared == null || shared.Length == 0) continue;

            bool changed = false;
            var newMats = new Material[shared.Length];

            for (int i = 0; i < shared.Length; i++)
            {
                var original = shared[i];
                if (original == null)
                {
                    newMats[i] = null;
                    continue;
                }

                // Lấy clone nếu đã có, chưa có thì tạo 1 lần
                if (!cloneMap.TryGetValue(original, out var clone) || clone == null)
                {
                    clone = new Material(original);
                    clone.name = original.name + " (RuntimeClone)";
                    cloneMap[original] = clone;
                    cloneMap[original].color = color;
                    createdClones.Add(clone);
                }

                newMats[i] = clone;

                // Nếu khác original thì đánh dấu changed (để khỏi set lại không cần thiết)
                if (newMats[i] != original) changed = true;
            }

            if (changed)
                r.sharedMaterials = newMats; // gán lại cho từng con, đúng slot
        }
    }

    // Ví dụ: chỉnh màu của tất cả material clone thuộc cùng "material gốc"
    public void SetColorForOriginal(Material original, Color color)
    {
        if (original == null) return;
        if (!cloneMap.TryGetValue(original, out var clone) || clone == null) return;

        if (clone.HasProperty("_BaseColor")) clone.SetColor("_BaseColor", color);
        else if (clone.HasProperty("_Color")) clone.SetColor("_Color", color);
    }

    void OnDestroy()
    {
        // Dọn clone để tránh leak
        for (int i = 0; i < createdClones.Count; i++)
        {
            if (createdClones[i] != null)
                Destroy(createdClones[i]);
        }
        createdClones.Clear();
        cloneMap.Clear();
    }
}
