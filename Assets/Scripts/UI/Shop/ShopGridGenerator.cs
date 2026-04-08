using UnityEngine;
using UnityEditor;

public class ShopGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rowCount = 3;
    [SerializeField] private int columnCount = 4;

    [Header("Cell Size")]
    [SerializeField] private Vector2 cellSize = new(100, 100);

    [Header("Padding")]
    [SerializeField] private float paddingLeft = 10f;
    [SerializeField] private float paddingRight = 10f;
    [SerializeField] private float paddingTop = 10f;
    [SerializeField] private float paddingBottom = 10f;

    [Header("Spacing")]
    [SerializeField] private float spacingX = 10f;
    [SerializeField] private float spacingY = 10f;

    [Header("Prefab & Container")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform container;

    public void GenerateGrid()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned!");
            return;
        }

        if (container == null)
        {
            Debug.LogError("Container is not assigned!");
            return;
        }

        ClearContainer();

        float totalWidth = columnCount * cellSize.x + (columnCount - 1) * spacingX + paddingLeft + paddingRight;
        float totalHeight = rowCount * cellSize.y + (rowCount - 1) * spacingY + paddingTop + paddingBottom;

        Vector2 startPos = new Vector2(-totalWidth / 2f + paddingLeft + cellSize.x / 2f, totalHeight / 2f - paddingTop - cellSize.y / 2f);

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                GameObject item =PrefabUtility.InstantiatePrefab(prefab, container) as GameObject;

                float posX = startPos.x + c * (cellSize.x + spacingX);
                float posY = startPos.y - r * (cellSize.y + spacingY);

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(posX, posY);
                    rect.sizeDelta = cellSize;
                }

                item.name = $"Cell_{r}_{c}";
                Undo.RegisterCreatedObjectUndo(item, "Generate Shop Grid");
            }
        }

        Debug.Log($"Generated {rowCount}x{columnCount} grid with {rowCount * columnCount} items.");
    }

    public void ClearContainer()
    {
        if (container == null) return;

        int childCount = container.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            Undo.DestroyObjectImmediate(child.gameObject);
        }
    }

    [ContextMenu("Generate Grid")]
    private void GenerateGridContext()
    {
        GenerateGrid();
    }

    /// <summary>
    /// 根据指定数量动态生成格子。自动计算行数，确保格子总数 >= count。
    /// </summary>
    public void GenerateGridWithCount(int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[ShopGridGenerator] Item count is {count}, skipping grid generation.");
            return;
        }

        int rows = Mathf.CeilToInt((float)count / columnCount);

        ClearContainer();

        float totalWidth = columnCount * cellSize.x + (columnCount - 1) * spacingX + paddingLeft + paddingRight;
        float totalHeight = rows * cellSize.y + (rows - 1) * spacingY + paddingTop + paddingBottom;

        Vector2 startPos = new Vector2(-totalWidth / 2f + paddingLeft + cellSize.x / 2f, totalHeight / 2f - paddingTop - cellSize.y / 2f);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                int index = r * columnCount + c;
                if (index >= count) return;

                GameObject item = PrefabUtility.InstantiatePrefab(prefab, container) as GameObject;

                float posX = startPos.x + c * (cellSize.x + spacingX);
                float posY = startPos.y - r * (cellSize.y + spacingY);

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(posX, posY);
                    rect.sizeDelta = cellSize;
                }

                item.name = $"Cell_{r}_{c}";
                Undo.RegisterCreatedObjectUndo(item, "Generate Shop Grid");
            }
        }

        Debug.Log($"[ShopGridGenerator] Generated {count} cells ({rows} rows x {columnCount} cols).");
    }
}
