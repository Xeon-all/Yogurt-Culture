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

    private List<ToppingData> _toppingList = new();
    private int _currentPage = 0;
    private int _totalPages = 0;
    private int _itemsPerPage;
    private List<RectTransform> _anchors = new();
    private List<GameObject> _instantiatedItems = new();

    public void InitData()
    {
        LoadToppingData();
        CollectAnchors();
        CalculatePagination();
        SetupButtons();
        ShowPage(0);
    }

    private void LoadToppingData()
    {
        _toppingList.Clear();
        _toppingList.AddRange(YogurtGameBoard.Instance.GetAllActiveToppings());
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

        ClearItems();

        int startIndex = pageIndex * _itemsPerPage;
        int endIndex = Mathf.Min(startIndex + _itemsPerPage, _toppingList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            int localIndex = i - startIndex;
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

        if (pageInfoText != null)
            pageInfoText.text = $"{pageIndex + 1} / {_totalPages}";

        if (prevPageButton != null)
            prevPageButton.interactable = pageIndex > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = pageIndex < _totalPages - 1;

        _currentPage = pageIndex;
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

    private void SetupItem(GameObject item, ToppingData data)
    {
        var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
        var dataCache = item.GetComponent<ToppingPreparation>();
        dataCache.Topping = data;
        if (textComponent != null)
        {
            textComponent.text = data.Name;
        }
        else
        {
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
