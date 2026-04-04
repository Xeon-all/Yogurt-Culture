using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SpawnDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    private ToppingData _topping;
    [HideInInspector] public ToppingData Topping
    {        
        get => _topping; 
        set => _topping = value;
    }
    protected GameObject Obj;
    protected Vector2 _pos;
    void Awake()
    {
        Topping = null;
        Obj = null;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if(Topping == null) return;
        EventSystem.current.pixelDragThreshold = 0;
        Obj = DesktopManager.Instance.SpawnToppingDragger(Topping);
        CursorManager.Instance.SetCursor(CursorData.CursorType.Dragging, true);
        _pos = eventData.position;
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if(_pos == eventData.position)
        {
            Destroy(Obj);
            CursorManager.Instance.SetCursor(CursorData.CursorType.Grab, unlock:true);
        }
        EventSystem.current.pixelDragThreshold = 8;
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(Topping == null) return;
        CursorManager.Instance.SetCursor(CursorData.CursorType.Grab);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorData.CursorType.Default);
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if(Obj == null) return;
        Obj.GetComponent<DraggerTopping>().InitDrag(eventData);
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        
    }
}