using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 酸奶产品的画面表现部分（拖拽相关）。
/// 挂载在任意带 Collider2D 的物体上实现鼠标拖拽。
/// 适用于 2D 场景（正交摄像机）。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class YogurtInstance : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("拖拽设置")]
    [Tooltip("拖拽时物体与鼠标之间是否保持初始偏移（true：更自然；false：中心跟随鼠标）。")]
    [SerializeField] private bool keepOffsetFromMouse = true;

    [Tooltip("是否在拖拽过程中锁定 Z 轴位置。")]
    [SerializeField] private bool lockZAxis = true;

    [Header("拖动范围限制")]
    [Tooltip("限制拖动范围的 GameObject（留空则自动查找 tag 为 Gameboard 的物体）")]
    [SerializeField] private GameObject dragBoundsCollider;

    /// <summary>
    /// 垃圾桶的 Layer 名称（需在 Project Settings > Tags and Layers 中配置）。
    /// </summary>
    [Header("接手检测")]
    [Tooltip("拖拽结束后判定的 Layer 名称（需在 Project Settings > Tags and Layers 中配置）。")]
    [SerializeField] private string orderLayerName = "order";

    [Tooltip("垃圾桶的 Tag 名称")]
    [SerializeField] private string trashCanTag = "TrashCan";

    private Camera mainCamera;
    private bool isDragging;
    private Vector3 dragOffset;
    private float objectZ;
    private int orderLayerMask;

    private Bounds bounds;
    private bool hasBounds = false;

    private YogurtData yogurtData;

    private void Awake()
    {
        mainCamera = Camera.main;
        objectZ = transform.position.z;
        orderLayerMask = LayerMask.GetMask(orderLayerName);

        EnsureEventSystem();
        EnsurePhysics2DRaycaster();
        InitializeDragBounds();

        yogurtData = GetComponent<YogurtData>();

        if (orderLayerMask == 0)
        {
            // Debug.LogWarning($"ShopItem: Layer '{orderLayerName}' 未找到，将无法触发订单检测。");
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    private void EnsurePhysics2DRaycaster()
    {
        if (mainCamera != null && mainCamera.GetComponent<Physics2DRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }

    private void InitializeDragBounds()
    {
        GameObject boundsObject = null;

        if (dragBoundsCollider != null)
        {
            boundsObject = dragBoundsCollider;
        }
        else
        {
            GameObject gameboard = GameObject.FindGameObjectWithTag("Gameboard");
            if (gameboard != null)
            {
                boundsObject = gameboard;
            }
        }

        if (boundsObject != null)
        {
            BoxCollider2D boxCollider = boundsObject.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                bounds = boxCollider.bounds;
                hasBounds = true;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            // Debug.LogWarning("DraggableObject: 未找到主摄像机，无法进行拖拽。");
            return;
        }

        isDragging = true;

        Vector3 mouseWorld = GetWorldPosition(eventData.position);
        if (keepOffsetFromMouse)
        {
            dragOffset = transform.position - mouseWorld;
        }
        else
        {
            dragOffset = Vector3.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || mainCamera == null)
        {
            return;
        }

        Vector3 mouseWorld = GetWorldPosition(eventData.position);
        Vector3 targetPos = mouseWorld + dragOffset;

        if (lockZAxis)
        {
            targetPos.z = objectZ;
        }

        if (hasBounds)
        {
            targetPos = ClampPositionToBounds(targetPos);
        }

        transform.position = targetPos;
    }

    private Vector3 ClampPositionToBounds(Vector3 position)
    {
        Collider2D selfCollider = GetComponent<Collider2D>();

        if (selfCollider != null)
        {
            Bounds selfBounds = selfCollider.bounds;
            float halfWidth = selfBounds.extents.x;
            float halfHeight = selfBounds.extents.y;

            float clampedX = Mathf.Clamp(position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
            float clampedY = Mathf.Clamp(position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

            return new Vector3(clampedX, clampedY, position.z);
        }
        else
        {
            float clampedX = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            float clampedY = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);

            return new Vector3(clampedX, clampedY, position.z);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        HandOver();
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 mouseScreen = screenPosition;
        float z = lockZAxis ? Mathf.Abs(mainCamera.transform.position.z - objectZ) : Mathf.Abs(mainCamera.transform.position.z);
        mouseScreen.z = z;
        return mainCamera.ScreenToWorldPoint(mouseScreen);
    }

    private void HandOver()
    {
        Collider2D selfCollider = GetComponent<Collider2D>();
        if (selfCollider == null)
        {
            return;
        }

        // 先检测垃圾桶
        if (CheckAndDestroyAtTrashCan())
        {
            return;
        }

        // 再检测订单
        if (orderLayerMask == 0)
        {
            return;
        }

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = orderLayerMask,
            useTriggers = true
        };
        Collider2D[] results = new Collider2D[4];
        int hitCount = selfCollider.OverlapCollider(filter, results);
        if (hitCount > 0)
        {
            if (yogurtData != null)
            {
                results[0].GetComponent<OrderEntity>()?.TrySubmit(yogurtData);
            }
        }
    }

    /// <summary>
    /// 检测是否与垃圾桶重合，如果是则销毁实例。
    /// </summary>
    /// <returns>如果成功销毁返回 true，否则返回 false</returns>
    private bool CheckAndDestroyAtTrashCan()
    {
        GameObject[] trashCans = GameObject.FindGameObjectsWithTag(trashCanTag);
        if (trashCans == null || trashCans.Length == 0)
        {
            return false;
        }

        Collider2D selfCollider = GetComponent<Collider2D>();
        if (selfCollider == null)
        {
            return false;
        }

        foreach (GameObject trashCanObj in trashCans)
        {
            TrashCan trashCan = trashCanObj.GetComponent<TrashCan>();
            if (trashCan != null && trashCan.CheckOverlapWithTrashCan(gameObject))
            {
                trashCan.TryDestroyYogurt(gameObject);
                return true;
            }
        }

        return false;
    }
}
