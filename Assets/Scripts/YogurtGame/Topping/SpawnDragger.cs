using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SpawnDragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IPointerUpHandler
{
    public ToppingData Topping;
    protected GameObject Obj;
    protected Vector2 _pos;
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        EventSystem.current.pixelDragThreshold = 0;
        Obj = DesktopManager.Instance.SpawnToppingDragger(Topping);
        _pos = eventData.position;
    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        EventSystem.current.pixelDragThreshold = 8;
        // if((_pos - eventData.position).magnitude <= 8)
        //     Destroy(Obj);
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Obj.GetComponent<DraggerTopping>().InitDrag(eventData);
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        
    }
}