using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Excel2Unity;
using TMPro;
using VContainer;
using VContainer.Unity;
using System;
using System.Linq;
using System.Reflection;

public class PreparationUI : MonoBehaviour
{
    [Header("Container")]
    [Tooltip("容器，包含若干子物体作为 item 锚点")]
    [SerializeField] private RectTransform container;

    [Header("Item Prefab")]
    [SerializeField] private List<GameObject> prefabs;

    [Header("Pagination")]
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Text pageInfoText;

    [Header("Category Tabs")]
    [SerializeField] private Button toppingTabButton;
    [SerializeField] private Button yogurtTabButton;

    private int _currentCategoryIndex = 0;

    private List<ToppingItem> _toppingItemList = new();
    private List<YogurtItem> _yogurtItemList = new();
    private Dictionary<Type, object> _allItemList = new();
    private int _currentPage = 0;
    private int _totalPages = 0;
    private int _itemsPerPage;
    private List<RectTransform> _anchors = new();
    private List<List<GameObject>> _itemIntansces = new();
    private bool _initialized = false;
    private IEventBus _eventBus;
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    public void InitData()
    {
        if (_initialized) return;
        _initialized = true;

        LoadData();
        CollectAnchors();
        SetupButtons();
        InstantiateAllItems();
        _currentCategoryIndex = -1;
        SwitchCategory(0);
        _eventBus.Subscribe<OnItemConsume>((_) => RefreshDisplay());
        _eventBus.Subscribe<OnItemRestore>((_) => RefreshDisplay());
    }
    private void LoadData()
    {
        _toppingItemList = YogurtGameBoard.Instance.GetAllActive<ToppingData>().Select(x => new ToppingItem(x.Data)).ToList();
        _yogurtItemList = YogurtGameBoard.Instance.GetAllActive<YogurtDatabase>().Select(x => new YogurtItem(x.Data)).ToList();
        _allItemList.Add(typeof(ToppingData), _toppingItemList);
        _allItemList.Add(typeof(YogurtDatabase), _yogurtItemList);
    }
    /// <summary>
    /// 一次性生成所有 item 实例，保存到对应 anchor，后续翻页不再实例化
    /// </summary>
    private void InstantiateAllItems()
    {
        _itemIntansces.Clear();
        CreateInstancesOfType<ToppingData, ToppingItem>();
        CreateInstancesOfType<YogurtDatabase, YogurtItem>();
    }
    private void CreateInstancesOfType<T, T2>() where T : TableDataBase
    {
        var curInstances = new List<GameObject>();
        var list = _allItemList[typeof(T)] as List<T2>;
        for (int i = 0; i < list.Count; i++)
        {
            var item = GetLocatedInstance<T>(i);
            curInstances.Add(item);
            SetupItem(item, list[i] as Itembase<T>);
        }
        _itemIntansces.Add(curInstances);
    }
    private GameObject GetLocatedInstance<T>(int i) where T : TableDataBase
    {
        _itemsPerPage = _anchors.Count;
        int localIndex = i % _itemsPerPage;
        RectTransform anchor = _anchors[localIndex];
        GameObject prefab = prefabs.Find(x => x.GetComponent<IPreparationItem<T>>() != null);
        GameObject item = Instantiate(prefab, anchor);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = Vector2.one;
        itemRect.sizeDelta = Vector2.zero;
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        return item;
    }

    /// <summary>
    /// 刷新当前页数据，不再实例化
    /// </summary>
    public void RefreshDisplay(int pageIndex = -1)
    {
        if (pageIndex == -1) pageIndex = _currentPage;

        var categoryInstances = _itemIntansces[_currentCategoryIndex];
        var listType = _allItemList.Keys.ElementAt(_currentCategoryIndex);
        var listType2 = _allItemList[listType].GetType().GetGenericArguments()[0];

        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, categoryInstances.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            var method = typeof(PreparationUI)
                .GetMethod(nameof(RefreshItemByIndex), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(listType, listType2);
            method.Invoke(this, new object[] { categoryInstances[i], i });
        }
    }

    private void RefreshItemByIndex<T, T2>(GameObject item, int index) where 
        T : TableDataBase where T2 : Itembase<T>
    {
        SetupItem(item, (_allItemList[typeof(T)] as List<T2>)[index] );
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
        var categoryInstances = _itemIntansces[_currentCategoryIndex];
        _totalPages = Mathf.CeilToInt((float)categoryInstances.Count / _itemsPerPage);
        if (_totalPages == 0) _totalPages = 1;
    }

    private void SetupButtons()
    {
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(OnPrevPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(OnNextPage);

        toppingTabButton?.onClick.AddListener(() => SwitchCategory(0));
        yogurtTabButton?.onClick.AddListener(() => SwitchCategory(1));
    }

    private void SwitchCategory(int index)
    {
        if (_currentCategoryIndex == index) return;

        _currentCategoryIndex = index;
        CalculatePagination();
        HideOtherCategoryInstances(index);
        ShowPage(0);
    }

    private void ShowPage(int pageIndex)
    {
        if (container == null)
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

    private void RefreshPageItems(int pageIndex)
    {
        var categoryInstances = _itemIntansces[_currentCategoryIndex];

        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, categoryInstances.Count);

        for (int i = 0; i < categoryInstances.Count; i++)
        {
            bool isOnPage = i >= startIndex && i < endIndex;
            categoryInstances[i]?.SetActive(isOnPage);
        }
    }

    private void ClearItems()
    {
        foreach (var categoryInstances in _itemIntansces)
        {
            foreach (var item in categoryInstances)
            {
                if (item != null)
                    Destroy(item);
            }
        }
        _itemIntansces.Clear();
    }

    private void HideOtherCategoryInstances(int exceptIndex)
    {
        for (int i = 0; i < _itemIntansces.Count; i++)
        {
            if (i == exceptIndex) continue;
            foreach (var item in _itemIntansces[i])
                item?.SetActive(false);
        }
    }
    private void SetupItem<T>(GameObject item, Itembase<T> itemData) where T : TableDataBase
    {
        var go = item.GetComponent<IPreparationItem<T>>();
        go.SetUpItem(itemData);
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