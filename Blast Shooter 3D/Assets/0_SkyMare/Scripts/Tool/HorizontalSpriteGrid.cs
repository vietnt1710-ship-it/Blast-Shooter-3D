using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class HorizontalSpriteGrid : MonoBehaviour
{
    [Header("Grid settings")]
    public int columns = 5;
    public Vector2 cellSize = new Vector2(1f, 1f);
    public Vector2 spacing = Vector2.zero;
    public bool autoUpdate = true;
    public bool is2DScene = false;

    void OnValidate()
    {
        if (autoUpdate && !Application.isPlaying)
            Fit();
    }

    [ContextMenu("Fit & Arrange Sprites (Centered)")]
    public void Fit()
    {
        // Lấy danh sách các child đang active
        List<Transform> activeChildren = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                activeChildren.Add(child);
            }
        }

        int count = activeChildren.Count;
        int cols = Mathf.Clamp(columns, 0, count);
        if (count == 0 || cols <= 0) return;

        // Số hàng cần thiết (ceil)
        int rows = (count + cols - 1) / cols;

        // Kích thước toàn bộ vùng grid (tính theo world units)
        float fullWidth = cols * cellSize.x + Mathf.Max(0, cols - 1) * spacing.x;
        float fullHeight = rows * cellSize.y + Mathf.Max(0, rows - 1) * spacing.y;

        // Offset để căn giữa (center)
        Vector2 centerOffset = new Vector2(fullWidth * 0.5f, fullHeight * 0.5f);

        for (int i = 0; i < count; i++)
        {
            Transform t = activeChildren[i];
            var sr = t.GetComponentInChildren<SpriteRenderer>();

            if (sr == null || sr.sprite == null)
            {
                // Vẫn đặt vị trí ô trống nếu không có sprite
            }
            else
            {
                // --- Scale sprite để vừa cellSize ---
                Vector2 spriteSize = sr.sprite.bounds.size;
                if (spriteSize.x > 0f && spriteSize.y > 0f)
                {
                    float scaleX = cellSize.x / spriteSize.x;
                    float scaleY = cellSize.y / spriteSize.y;

                    // Scale transform chứa SpriteRenderer (không scale root nếu sprite ở child)
                    if (sr.transform == t)
                        t.localScale = new Vector3(cellSize.x, cellSize.y, cellSize.y);
                    else
                        sr.transform.localScale = new Vector3(cellSize.x, cellSize.y, 1f);
                }
            }

            // --- Tính vị trí theo hàng/cột (tính từ góc trên-left) ---
            int col = i % cols;
            int row = i / cols;
            Vector2 posFromTopLeft = new Vector2(
                cellSize.x * 0.5f + col * (cellSize.x + spacing.x),
                -(cellSize.y * 0.5f + row * (cellSize.y + spacing.y))
            );

            // Trừ centerOffset để đưa toàn bộ vùng về tâm (0,0) = transform.position
            Vector2 localPos = posFromTopLeft - new Vector2(centerOffset.x, -centerOffset.y);

            if (is2DScene)
            {
                t.localPosition = new Vector3(localPos.x, localPos.y, 0);
            }
            else
            {
                t.localPosition = new Vector3(localPos.x, 0, localPos.y);
            }
        }
    }
}
