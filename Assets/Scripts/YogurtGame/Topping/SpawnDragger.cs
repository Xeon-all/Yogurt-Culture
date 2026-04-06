using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SpawnDragger : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public ToppingItem Item;

    protected GameObject Obj;
    protected Vector2 _pos;

    void Awake()
    {
        Item = null;
        Obj = null;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (Item?.Data == null || Item.Count <= 0) return;

        var item = ConstructItem();
        
        EventSystem.current.pixelDragThreshold = 0;
        Obj = DesktopManager.Instance.SpawnToppingDragger(item);
        Obj.GetComponent<DraggerTopping>()?.SetSource(this);
        CursorManager.Instance.SetCursor(CursorData.CursorType.Dragging, true);
        _pos = eventData.position;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (_pos == eventData.position)
        {
            Destroy(Obj);
            CursorManager.Instance.SetCursor(CursorData.CursorType.Grab, unlock: true);
        }
        EventSystem.current.pixelDragThreshold = 8;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (Item?.Data == null) return;
        CursorManager.Instance.SetCursor(CursorData.CursorType.Grab);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorData.CursorType.Default);
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (Obj == null) return;
        Obj.GetComponent<DraggerTopping>().InitDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
    }
    public abstract void RestoreTopping(ToppingItem item);
    protected abstract ToppingItem ConstructItem();
}