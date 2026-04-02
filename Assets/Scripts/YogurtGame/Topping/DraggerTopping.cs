using UnityEngine;
using UnityEngine.EventSystems;

public class DraggerTopping : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] public ToppingData toppingData;

    public void SetData(ToppingData data)
    {
        toppingData = data;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        eventData.pointerDrag = gameObject;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetWorldPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Collider2D selfCollider = GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        Collider2D[] results = new Collider2D[4];
        selfCollider.OverlapCollider(filter, results);
        foreach (var col in results)
        {
            col?.GetComponent<IReceiveTopping>()?.ReceiveTopping(toppingData);
        }
        Destroy(gameObject);
    }

    protected Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 mouseScreen = screenPosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mouseScreen);
    }

    public void InitDrag(PointerEventData eventData)
    {
        ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.initializePotentialDrag);
        ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.beginDragHandler);
        ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.dragHandler);
    }
}
