using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToppingPreparation : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IPointerUpHandler
{
    public ToppingData Topping;
    public string GetID() => Topping.ID;
    private GameObject Obj;
    private Vector2 _pos;
    private const float threshold = 1e-2f;
    public void OnPointerDown(PointerEventData eventData)
    {
        Obj = DesktopManager.Instance.SpawnToppingDragger(Topping);
        _pos = eventData.position;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if((_pos - eventData.position).magnitude < threshold)
            Destroy(Obj);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Obj.GetComponent<DraggerTopping>().InitDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        
    }
}