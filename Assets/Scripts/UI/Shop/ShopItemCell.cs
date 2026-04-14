using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 商店格子 UI 组件，响应鼠标事件并显示商品信息。
/// 当前仅打印 Debug Log，后续接入购买逻辑。
/// </summary>
public class ShopItemCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI stockText;
    private ShopUI rootUI;

    public ToppingItem Item { get; private set; }

    public void SetupItem(ToppingItem item, ShopUI root)
    {
        Item = item;
        rootUI = root;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (Item?.Data == null) return;

        if (nameText != null)
            nameText.text = Item.Data.Name;

        if (priceText != null)
            priceText.text = $"{Item.Data.Price}";

        if (stockText != null)
        {
            int count = YogurtGameBoard.Instance.GetToppingCount(Item.Data.ID);
            stockText.text = $"{count}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        Debug.Log($"[ShopItemCell] Clicked: {Item.Data.Name} (ID: {Item.Data.ID})");
        RestockTopping();
    }

    /// <summary>
    /// 通过 Cell 中的 Item 反查 YogurtGameBoard 仓库，对应储量 +1
    /// </summary>
    private void RestockTopping(int amount = 1)
    {
        if (Item?.Data == null) return;

        string id = Item.Data.ID;
        var toppingItem = YogurtGameBoard.Instance.GetToppingItem(id);
        if (toppingItem == null)
        {
            Debug.LogWarning($"[ShopItemCell] ToppingItem not found in repository: {id}");
            return;
        }

        int previousCount = toppingItem.Count;
        EconomyManager.Instance.AddMoney(-Item.Data.Price * amount);
        YogurtGameBoard.Instance.RestoreTopping(id, amount);
        int newCount = toppingItem.Count;

        rootUI.RefreshMoney();
        RefreshDisplay();

        Debug.Log($"[ShopItemCell] Restocked {Item.Data.Name} (ID: {id}): {previousCount} → {newCount}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        Debug.Log($"[ShopItemCell] Enter: {Item.Data.Name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        Debug.Log($"[ShopItemCell] Exit: {Item.Data.Name}");
    }
}
