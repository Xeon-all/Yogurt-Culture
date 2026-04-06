using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ToppingPreparation : SpawnDragger
{
    private TextMeshProUGUI _tmp;

    void Awake()
    {
        _tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void RestoreTopping(ToppingItem item)
    {
        YogurtGameBoard.Instance.RestoreTopping(item);
        // RefreshCount();
    }

    protected override ToppingItem ConstructItem()
    {
        return Item;
    }

    public void RefreshCount()
    {
        if (Item?.Data == null) return;
        Item.Count = YogurtGameBoard.Instance.GetToppingCount(Item.Data.ID);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (Item?.Data == null) return;
        if (_tmp != null)
            _tmp.text = $"{Item.Data.Name} x{Item.Count}";
        Debug.Log($"update display of {Item.Data.Name} x{Item.Count}");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        YogurtGameBoard.Instance.ConsumeTopping(Item.Data.ID, Item.Count);
    }

    public string GetID() => Item?.Data?.ID;
}