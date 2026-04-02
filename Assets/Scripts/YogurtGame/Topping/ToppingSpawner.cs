using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using YogurtCulture.GameLoop;

/// <summary>
/// Topping生成器：从操作台鼠标按下后生成Topping实体并跟随鼠标
/// 将此脚本挂载到操作台的GameObject上
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ToppingSpawner : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler,
    IReceiveTopping, IBeginDragHandler, IDragHandler, IPointerUpHandler
{
    private ToppingData Topping;
    private SpriteRenderer sr;

    [Header("Tooltip 设置")]
    [Tooltip("Tooltip 相对于槽位顶部的偏移（屏幕空间）")]
    [SerializeField] private Vector2 tooltipOffset = new(0f, 20f);
    [SerializeField] private float tooltipDelay = 0.2f;

    private Coroutine _tooltipCoroutine;
    private Vector2 _pos;

    void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = null;
    }
    
    #region Tooltip

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Topping == null || 
            GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation) 
            return;

        _tooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelAndHideTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(tooltipDelay);

        var tagDatas = YogurtData.ParseTags(Topping.Tags);
        if (tagDatas == null || tagDatas.Count == 0) yield break;

        string content = string.Join("\n", tagDatas.ConvertAll(t => $"{t.Tag}({t.Value})"));
        TooltipManager.Instance.Show(content, transform, tooltipOffset);
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
    private GameObject Obj;
    public void OnPointerDown(PointerEventData eventData)
    {
        if(Topping == null) return;
        Obj = DesktopManager.Instance.SpawnToppingDragger(Topping);
        _pos = eventData.position;
        if(GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
            ClearContain();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if((_pos - eventData.position).magnitude < 1e-2f)
            Destroy(Obj);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Obj.GetComponent<DraggerTopping>().InitDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        
    }
    #endregion

    #region IReceiveTopping

    /// <summary>
    /// 接收 Dragger 释放时传入的 ToppingData。
    /// </summary>
    public void ReceiveTopping(ToppingData topping)
    {
        // Debug.Log("receiving ID : " + topping.ID);
        if(GameLoopManager.Instance.CurrentPhase != GamePhase.Preparation) return;
        ChangeToTopping(topping);
        Topping = topping;
    }

    /// <summary>
    /// 将 Topping 转化为桌面配料并处理视觉表现。
    /// </summary>
    private const string SPRITEPATH = "Art/Yogurt/Topping/";
    private void ChangeToTopping(ToppingData topping)
    {
        sr.sprite = Resources.Load<Sprite>(SPRITEPATH + topping.GrooveName);
        ColorUtility.TryParseHtmlString(topping.color, out var color);
        sr.color = color;
    }

    #endregion
    void ClearContain()
    {
        sr.sprite = null;
        sr.color = Color.white;
        Topping = null;
    }
}

