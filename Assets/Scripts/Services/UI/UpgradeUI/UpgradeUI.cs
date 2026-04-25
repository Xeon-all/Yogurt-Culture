using VContainer;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VContainer.Unity;
using System;
using Excel2Unity;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private RectTransform _itemRoot;
    private List<ToppingItem> _toppingItemList = new();
    private List<YogurtItem> _yogurtItemList = new();
    private List<GameObject> _itemInstances = new();
    private object _curList;
    private IEventBus _eventBus;
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    public void OnEnable()
    {
        GetData();
        InstantiateAllItems();
        ShowTopping();
        // _eventBus.Subscribe<OnItemUpgrade>((_) => RefreshDisplay());
    }
    public void ShowUpgradeUI()
    {
        gameObject.SetActive(true);
    }
    public void HideUpgradeUI()
    {
        gameObject.SetActive(false);
    }
    public void ShowTopping()
    {
        _curList = _toppingItemList;
        ShowContent<ToppingData, ToppingItem>(() => _toppingItemList);
    }
    public void ShowYogurt()
    {
        _curList = _yogurtItemList;
        ShowContent<YogurtData, YogurtItem>(() => _yogurtItemList);
    }
    private void GetData()
    {
        var l = YogurtGameBoard.Instance.GetAll<ToppingData>();
        foreach (var item in l)
            _toppingItemList.Add(item as ToppingItem);
        var l2 = YogurtGameBoard.Instance.GetAll<YogurtData>();
        foreach (var item in l2)
            _yogurtItemList.Add(item as YogurtItem);
    }
    private void InstantiateAllItems()
    {
        foreach (Transform child in _itemRoot)
            Destroy(child.gameObject);
        var count = Mathf.Max(_toppingItemList.Count, _yogurtItemList.Count);
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(_itemPrefab, transform);
            obj.transform.SetParent(_itemRoot);
            _itemInstances.Add(obj);
        }
    }
    private void ShowContent<T, T2>(Func<List<T2>> getItems)
        where T : TableDataBase
        where T2 : Itembase<T>
    {
        var items = getItems();
        int idx = 0;
        foreach (var item in items)
        {
            if(idx < items.Count)
            {
                _itemInstances[idx].SetActive(true);
                _itemInstances[idx].GetComponent<UpgradeItem>().SetUp<T, T2>(item);
            }
            else
                _itemInstances[idx].SetActive(false);
            idx++;
        }
    }
    private void RefreshDisplay()
    {
        foreach(var item in _itemInstances)
            item.GetComponent<UpgradeItem>().RefreshDisplay();
    }
}