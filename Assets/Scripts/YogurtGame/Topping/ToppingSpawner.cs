using UnityEngine;
using UnityEngine.EventSystems;
using YogurtCulture.GameLoop;

/// <summary>
/// Topping生成器：从操作台鼠标按下后生成Topping实体并跟随鼠标
/// 将此脚本挂载到操作台的GameObject上
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ToppingSpawner : MonoBehaviour, IPointerDownHandler, IReceiveTopping, IBeginDragHandler, IDragHandler
{
    private ToppingData Topping;
    private SpriteRenderer sr;
    void Awake()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = null;
    }
    
    #region 生成拖拽
    private GameObject Obj;
    public void OnPointerDown(PointerEventData eventData)
    {
        Obj = DesktopManager.Instance.SpawnToppingDragger(Topping);
        if(GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
            ClearContain();
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

