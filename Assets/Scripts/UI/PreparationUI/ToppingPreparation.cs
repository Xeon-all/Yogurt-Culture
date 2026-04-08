using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ToppingPreparation : SpawnDragger
{
    [SerializeField] private TextMeshProUGUI countText;
    private TextMeshProUGUI _tmp;

    public override void RestoreTopping(ToppingItem item)
    {
        YogurtGameBoard.Instance.RestoreTopping(item);
    }

    protected override ToppingItem ConstructItem()
    {
        return Item;
    }

    public void Refresh()
    {
        if (Item?.Data == null) return;
        Item.Count = YogurtGameBoard.Instance.GetToppingCount(Item.Data.ID);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (Item == null) return;
        if(_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
        _tmp.text = Item.Data.Name;
        if (countText != null)
            countText.text = $"{Item.Count}";
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        YogurtGameBoard.Instance.ConsumeTopping(Item.Data.ID, Item.Count);
    }

    public string GetID() => Item?.Data?.ID;
}