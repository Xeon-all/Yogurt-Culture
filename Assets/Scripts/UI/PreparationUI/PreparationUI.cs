using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Excel2Unity;
using TMPro;

public class PreparationUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private Transform itemPanel;

    [Header("Item Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Grid Settings")]
    [SerializeField] private int maxRows = 3;
    [SerializeField] private int maxCols = 4;
    [SerializeField] private float itemSpacing = 10f;

    [Header("Pagination")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Text pageInfoText;

    private List<ToppingData> _toppingList = new();
    private int _currentPage = 0;
    private int _totalPages = 0;
    private int _itemsPerPage;

    public void InitData()
    {
        LoadToppingData();
        CalculatePagination();
        SetupButtons();
        ShowPage(0);
    }

    private void LoadToppingData()
    {
        _toppingList.Clear();
        
        // 从 YogurtGameBoard 获取所有 Topping 数据
        _toppingList.AddRange(YogurtGameBoard.Instance.GetAll<ToppingData>("Topping"));
    }

    private void CalculatePagination()
    {
        _itemsPerPage = maxRows * maxCols;
        _totalPages = Mathf.CeilToInt((float)_toppingList.Count / _itemsPerPage);
        if (_totalPages == 0) _totalPages = 1;
    }

    private void SetupButtons()
    {
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(OnPrevPage);
        
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(OnNextPage);
    }

    private void ShowPage(int pageIndex)
    {
        if (itemPanel == null || itemPrefab == null)
        {
            return;
        }

        // 清除现有 items
        foreach (Transform child in itemPanel)
        {
            Destroy(child.gameObject);
        }

        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _toppingList.Count);

        // 计算 Panel 的实际大小
        RectTransform panelRect = itemPanel.GetComponent<RectTransform>();
        float panelWidth = panelRect.rect.width;
        float panelHeight = panelRect.rect.height;

        // 计算每个 item 的尺寸
        float itemWidth = (panelWidth - (maxCols + 1) * itemSpacing) / maxCols;
        float itemHeight = (panelHeight - (maxRows + 1) * itemSpacing) / maxRows;

        for (int i = startIndex; i < endIndex; i++)
        {
            int localIndex = i - startIndex;
            int row = localIndex / maxCols;
            int col = localIndex % maxCols;

            GameObject item = Instantiate(itemPrefab, itemPanel);
            RectTransform itemRect = item.GetComponent<RectTransform>();

            // 从上到下、从左到右排列
            float x = itemSpacing + col * (itemWidth + itemSpacing) + itemWidth / 2;
            float y = -(itemSpacing + row * (itemHeight + itemSpacing) + itemHeight / 2);
            
            itemRect.anchoredPosition = new Vector2(x, y);
            itemRect.sizeDelta = new Vector2(itemWidth, itemHeight);

            // 设置 item 数据
            SetupItem(item, _toppingList[i]);
        }

        // 更新页码显示
        if (pageInfoText != null)
            pageInfoText.text = $"{pageIndex + 1} / {_totalPages}";

        // 更新翻页按钮状态
        if (prevPageButton != null)
            prevPageButton.interactable = pageIndex > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = pageIndex < _totalPages - 1;

        _currentPage = pageIndex;
    }

    private void SetupItem(GameObject item, ToppingData data)
    {
        // 根据需要设置 item 的显示内容
        var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = data.ID;
        }
        else
        {
            // 尝试使用旧版 Text
            var legacyText = item.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = data.ID;
            }
        }
    }

    private void OnPrevPage()
    {
        if (_currentPage > 0)
        {
            ShowPage(_currentPage - 1);
        }
    }

    private void OnNextPage()
    {
        if (_currentPage < _totalPages - 1)
        {
            ShowPage(_currentPage + 1);
        }
    }
}
