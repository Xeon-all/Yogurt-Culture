using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Excel2Unity;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Money")]
    [SerializeField] private TextMeshProUGUI MoneyTxt;
    [Header("ItemContent")]
    [Tooltip("必须包含 ShopGridGenerator 组件的根对象")]
    [SerializeField] private RectTransform itemContent;

    [Header("Item Prefab")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Pagination")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TextMeshProUGUI pageInfoText;
    [Header("Intro")]
    [SerializeField] private Image introIcon;
    [SerializeField] private TextMeshProUGUI introTxt;

    private List<ToppingItem> _shopItemList = new();
    private int _currentPage = 0;
    private int _totalPages = 0;
    private int _itemsPerPage;
    private List<RectTransform> _anchors = new();
    private List<GameObject> _instantiatedItems = new();
    private bool _initialized = false;

    void Awake()
    {
        InitData();
    }

    public void InitData()
    {
        if (_initialized) return;
        _initialized = true;

        LoadShopItemData();
        SetupGrid();
        CalculatePagination();
        InstantiateAllItems();
        SetupButtons();
        ShowPage(0);
        RefreshMoney();
    }

    public void Show()
    {
        InitData();
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 调用 ShopGridGenerator 生成网格布局，然后收集子物体作为锚点。
    /// 内部通过 ShopGridGenerator 是否有子物体判断是否已生成，避免重复。
    /// </summary>
    private void SetupGrid()
    {
        if (itemContent == null)
        {
            Debug.LogError("ItemContent is not assigned!");
            return;
        }

        var gridGenerator = itemContent.GetComponent<ShopGridGenerator>();
        if (gridGenerator == null)
        {
            Debug.LogError("ItemContent does not have ShopGridGenerator component!");
            return;
        }

        if (itemContent.childCount == 0)
            gridGenerator.GenerateGridWithCount(_shopItemList.Count);

        CollectAnchors();
        DeactivateExcessAnchors();
    }

    /// <summary>
    /// 禁用锚点格子中超出数据数量的部分，保证不显示空格子
    /// </summary>
    private void DeactivateExcessAnchors()
    {
        for (int i = 0; i < _anchors.Count; i++)
        {
            bool hasData = i < _shopItemList.Count;
            _anchors[i].gameObject.SetActive(hasData);
        }
    }

    private void CollectAnchors()
    {
        _anchors.Clear();
        foreach (Transform child in itemContent)
        {
            if (child is RectTransform rt)
                _anchors.Add(rt);
        }
    }

    private void LoadShopItemData()
    {
        _shopItemList.Clear();
        var allToppings = YogurtGameBoard.Instance.GetAllActiveToppings();
        foreach (var topping in allToppings)
        {
            if (topping == null || string.IsNullOrWhiteSpace(topping.ID)) continue;
            var toppingItem = new ToppingItem(topping);
            _shopItemList.Add(toppingItem);
        }
        // Debug.Log($"[ShopUI] Loaded {_shopItemList.Count} items from YogurtGameBoard.");
    }

    /// <summary>
    /// 一次性生成所有 item 实例到对应锚点，后续翻页不再实例化
    /// </summary>
    private void InstantiateAllItems()
    {
        _instantiatedItems.Clear();
        for (int i = 0; i < _shopItemList.Count; i++)
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
            SetupItem(item, _shopItemList[i]);
        }
    }

    /// <summary>
    /// 刷新当前页数据引用，不再实例化
    /// </summary>
    public void RefreshMoney()
    {
        MoneyTxt.text = EconomyManager.Instance.Money.ToString();
    }
    public void RefreshDisplay(int pageIndex = -1)
    {
        if (pageIndex == -1) pageIndex = _currentPage;
        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _shopItemList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            SetupItem(_instantiatedItems[i], _shopItemList[i]);
        }
    }

    private void CalculatePagination()
    {
        _itemsPerPage = _anchors.Count;
        _totalPages = Mathf.CeilToInt((float)_shopItemList.Count / _itemsPerPage);
        if (_totalPages == 0) _totalPages = 1;
    }

    private void SetupButtons()
    {
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(OnPrevPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(OnNextPage);
    }

    public void SetupIntro(ToppingItem item)
    {
        var path = "Art/Yogurt/Topping/" + item.Data.ItemIcon;
        var sprite = Resources.Load<Sprite>(path);
        if(sprite)
            introIcon.sprite = sprite;
        if(!string.IsNullOrEmpty(item.Data.Descrip))
            introTxt.text = item.Data.Descrip;
    }

    private void ShowPage(int pageIndex)
    {
        if (itemContent == null || itemPrefab == null)
            return;

        RefreshPageItems(pageIndex);
        RefreshDisplay(pageIndex);

        if (pageInfoText != null)
            pageInfoText.text = $"{pageIndex + 1}";

        if (prevPageButton != null)
            prevPageButton.interactable = pageIndex > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = pageIndex < _totalPages - 1;

        _currentPage = pageIndex;

        int firstIndexOnPage = pageIndex * _itemsPerPage;
        if (firstIndexOnPage < _shopItemList.Count)
            SetupIntro(_shopItemList[firstIndexOnPage]);
    }

    /// <summary>
    /// 根据页索引切换 item 的激活状态，不销毁实例
    /// </summary>
    private void RefreshPageItems(int pageIndex)
    {
        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _shopItemList.Count);

        for (int i = 0; i < _instantiatedItems.Count; i++)
        {
            bool isOnPage = i >= startIndex && i < endIndex;
            _instantiatedItems[i]?.SetActive(isOnPage);
        }
    }

    private void SetupItem(GameObject item, ToppingItem itemData)
    {
        var cell = item.GetComponent<ShopItemCell>();
        cell.SetupItem(itemData, this);
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

    private void ClearItems()
    {
        foreach (GameObject item in _instantiatedItems)
        {
            if (item != null)
                Destroy(item);
        }
        _instantiatedItems.Clear();
    }

    private void OnDestroy()
    {
        ClearItems();
    }
}
