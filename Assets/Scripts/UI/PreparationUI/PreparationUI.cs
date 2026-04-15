using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Excel2Unity;
using TMPro;

public class PreparationUI : MonoBehaviour
{
    [Header("Container")]
    [Tooltip("容器，包含若干子物体作为 item 锚点")]
    [SerializeField] private RectTransform container;

    [Header("Item Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Pagination")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Text pageInfoText;

    private List<ToppingItem> _toppingList = new();
    private int _currentPage = 0;
    private int _totalPages = 0;
    private int _itemsPerPage;
    private List<RectTransform> _anchors = new();
    private List<GameObject> _instantiatedItems = new();
    private bool _initialized = false;

    public void InitData()
    {
        if (_initialized) return;
        _initialized = true;

        LoadToppingData();
        CollectAnchors();
        CalculatePagination();
        SetupButtons();
        InstantiateAllItems();
        ShowPage(0);
    }

    /// <summary>
    /// 一次性生成所有 item 实例，保存到对应 anchor，后续翻页不再实例化
    /// </summary>
    private void InstantiateAllItems()
    {
        _instantiatedItems.Clear();
        for (int i = 0; i < _toppingList.Count; i++)
        {
            int localIndex = i % _itemsPerPage;
            RectTransform anchor = _anchors[localIndex];

            GameObject item = Instantiate(itemPrefab, anchor);
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.anchorMin = Vector2.zero;
            itemRect.anchorMax = Vector2.one;
            itemRect.sizeDelta = Vector2.zero;
            itemRect.pivot = new Vector2(0.5f, 0.5f);

            _instantiatedItems.Add(item);
            SetupItem(item, _toppingList[i]);
        }
    }

    /// <summary>
    /// 刷新当前页数据，不再实例化
    /// </summary>
    public void RefreshDisplay(int pageIndex = -1)
    {
        if (pageIndex == -1) pageIndex = _currentPage;
        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _toppingList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            SetupItem(_instantiatedItems[i], _toppingList[i]);
        }
    }
    private void LoadToppingData()
    {
        _toppingList.Clear();
        var allToppings = YogurtGameBoard.Instance.GetAllActiveToppings();
        foreach (var topping in allToppings)
        {
            if (topping == null || string.IsNullOrWhiteSpace(topping.ID)) continue;
            _toppingList.Add(YogurtGameBoard.Instance.GetToppingItem(topping.ID));
        }
    }

    private void CollectAnchors()
    {
        _anchors.Clear();
        if (container == null) return;
        foreach (Transform child in container)
        {
            if (child is RectTransform rt)
                _anchors.Add(rt);
        }
    }

    private void CalculatePagination()
    {
        _itemsPerPage = _anchors.Count;
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
        if (container == null || itemPrefab == null)
            return;

        RefreshPageItems(pageIndex);
        RefreshDisplay(pageIndex);

        if (pageInfoText != null)
            pageInfoText.text = $"{pageIndex + 1} / {_totalPages}";

        if (prevPageButton != null)
            prevPageButton.interactable = pageIndex > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = pageIndex < _totalPages - 1;

        _currentPage = pageIndex;
    }

    /// <summary>
    /// 根据页索引切换 item 的激活状态，不销毁实例
    /// </summary>
    private void RefreshPageItems(int pageIndex)
    {
        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _toppingList.Count);

        for (int i = 0; i < _instantiatedItems.Count; i++)
        {
            bool isOnPage = i >= startIndex && i < endIndex;
            _instantiatedItems[i]?.SetActive(isOnPage);
        }
    }

    private void ClearItems()
    {
        foreach (GameObject item in _instantiatedItems)
        {
            if (item != null)
                Destroy(item);
        }
        _instantiatedItems.Clear();
    }
    private void SetupItem(GameObject item, ToppingItem itemData)
    {
        var dataCache = item.GetComponent<ToppingPreparation>();
        dataCache.Item = itemData;
        dataCache.Refresh();
    }

    private void OnPrevPage()
    {
        if (_currentPage > 0)
            ShowPage(_currentPage - 1);
    }

    private void OnNextPage()
    {
        if (_currentPage < _totalPages - 1)
            ShowPage(_currentPage + 1);
    }

    private void OnDestroy()
    {
        ClearItems();
    }
}