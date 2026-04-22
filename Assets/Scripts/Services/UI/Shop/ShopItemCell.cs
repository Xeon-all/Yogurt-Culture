using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 商店格子 UI 组件，响应鼠标事件并显示商品信息。
/// 当前仅打印 Debug Log，后续接入购买逻辑。
/// </summary>
public class ShopItemCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
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
        if(!string.IsNullOrEmpty(Item.Data.ItemIcon))
        {
            nameText.text = "";
            var sprite = Resources.Load<Sprite>(YogurtGameBoard.TOPPING_SPRITE + Item.Data.ItemIcon);
            if(sprite != null)
                GetComponent<Image>().sprite = sprite;
        }
        if (priceText != null)
            priceText.text = $"{Item.Data.Price}";

        if (stockText != null)
        {
            int count = YogurtGameBoard.Instance.Get<ToppingData>(Item.Data.ID).Count;
            stockText.text = $"{count}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        // Debug.Log($"[ShopItemCell] Clicked: {Item.Data.Name} (ID: {Item.Data.ID})");
        RestockTopping();
    }

    /// <summary>
    /// 通过 Cell 中的 Item 反查 YogurtGameBoard 仓库，对应储量 +1
    /// </summary>
    private void RestockTopping(int amount = 1)
    {
        if (Item?.Data == null) return;

        string id = Item.Data.ID;
        var toppingItem = YogurtGameBoard.Instance.Get<ToppingData>(id);
        if (toppingItem == null)
        {
            Debug.LogWarning($"[ShopItemCell] ToppingItem not found in repository: {id}");
            return;
        }

        EconomyManager.Instance.AddMoney(-Item.Data.Price * amount);
        YogurtGameBoard.Instance.Restore<ToppingData>(id, amount);

        rootUI.RefreshMoney();
        RefreshDisplay();

        // Debug.Log($"[ShopItemCell] Restocked {Item.Data.Name} (ID: {id}): {previousCount} → {newCount}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        rootUI.SetupIntro(Item);
        // Debug.Log($"[ShopItemCell] Enter: {Item.Data.Name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        // Debug.Log($"[ShopItemCell] Exit: {Item.Data.Name}");
    }
}
