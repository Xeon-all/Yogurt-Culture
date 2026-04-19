using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using YogurtCulture.GameLoop;

[RequireComponent(typeof(Collider2D))]
public class ToppingSpawner : SpawnDragger, IPointerEnterHandler, IPointerExitHandler,
    IReceiveTopping
{
    private SpriteRenderer sr;

    [Header("Tooltip 设置")]
    [Tooltip("Tooltip 相对于槽位顶部的偏移（屏幕空间）")]
    [SerializeField] private Vector2 tooltipOffset = new(0f, 20f);
    [SerializeField] private float tooltipDelay = 0.2f;

    private Coroutine _tooltipCoroutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = null;
    }

    #region Tooltip

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if(Item == null) return;
        if(Item.Count == 0) Item = null;
        base.OnPointerEnter(eventData);
        if (Item?.Data == null ||
            GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
            return;
        _tooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        CancelAndHideTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(tooltipDelay);

        if (Item?.Data == null) yield break;
        var tagDatas = YogurtData.ParseTags(Item.Data.Tags);
        if (tagDatas.Count == 0) yield break;

        string content = string.Join("\n", tagDatas.ConvertAll(t => $"{t.Tag}({t.Value})"));
        TooltipManager.Instance.Show(content, transform, tooltipOffset);
        Debug.Log("Content Item count : " + Item.Count);
    }

    private void CancelAndHideTooltip()
    {
        if (_tooltipCoroutine != null)
        {
            StopCoroutine(_tooltipCoroutine);
            _tooltipCoroutine = null;
        }
        TooltipManager.Instance.Hide();
    }

    #endregion

    #region 生成拖拽
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
            ClearContain();
    }
    public override void RestoreTopping(ToppingItem item)
    {
        switch(GameLoopManager.Instance.CurrentPhase)
        {
            case GamePhase.Preparation:
                YogurtGameBoard.Instance.RestoreTopping(item);
                break;
            case GamePhase.MorningOp:
                Item.Count += item.Count;
                break;
            default: break;
        }
            
    }
    protected override ToppingItem ConstructItem()
    {
        switch(GameLoopManager.Instance.CurrentPhase)
        {
            case GamePhase.Preparation: return Item;
            case GamePhase.MorningOp:
                Item.Count -= 1;
                return new ToppingItem(Item.Data, 1);
            default: break;
        }
        return null;
    }
    #endregion

    #region IReceiveTopping

    public void ReceiveTopping(ToppingItem item)
    {
        if (item?.Data == null) return;

        if(GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation) return;
        ApplyVisual(item.Data);
        Item = item;

        Debug.Log($"[ReceiveTopping] id={item.Data.ID}, rawCount={item.Count}");

        // if (GameLoopManager.Instance.CurrentPhase == GamePhase.MorningOp && item.Count > 1)
        // {
        //     // int restore = item.Count - 1;
        //     item.Count = 1;
        //     YogurtGameBoard.Instance.RestoreTopping(item);
        //     Debug.Log($"[ReceiveTopping] MorningOp 归还 {item.Count} 个");
        // }

        // if (GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
        // {
        //     ApplyVisual(item.Data);
        //     Item = item;
        // }
    }

    private const string SPRITEPATH = "Art/Yogurt/Topping/";
    private void ApplyVisual(ToppingData topping)
    {
        sr.sprite = Resources.Load<Sprite>(SPRITEPATH + topping.GrooveName);
        ColorUtility.TryParseHtmlString(topping.color, out var color);
        sr.color = color;
    }

    #endregion
    public void ClearContain()
    {
        sr.sprite = null;
        sr.color = Color.white;
        Item = null;
    }
}
