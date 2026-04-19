using UnityEngine;
using UnityEngine.EventSystems;

public class DraggerTopping : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] public ToppingItem Item;

    private bool _received;
    private SpawnDragger source;
    public void SetSource(SpawnDragger spawner) => source = spawner;

    public void SetData(ToppingItem item)
    {
        Item = new ToppingItem(item.Data, item.Count);
        _received = false;
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
        CursorManager.Instance.SetCursor(CursorData.CursorType.Default, unlock: true);
        _received = false;

        Collider2D selfCollider = GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        Collider2D[] results = new Collider2D[4];
        selfCollider.OverlapCollider(filter, results);

        foreach (var col in results)
        {
            if (col == null) continue;
            var receiver = col.GetComponent<IReceiveTopping>();
            if (receiver != null)
            {
                receiver.ReceiveTopping(Item);
                _received = true;
                break;
            }
        }

        if (!_received)
        {
            Debug.Log($"[DraggerTopping] 未被接收，归还 {Item.Count} 个 {Item.Data.ID}");
            source.RestoreTopping(Item);
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
