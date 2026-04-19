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

    [Header("接手检测")]
    [Tooltip("拖拽结束后判定的 Layer 名称（需在 Project Settings > Tags and Layers 中配置）。")]
    [SerializeField] private string orderLayerName = "order";

    [Tooltip("垃圾桶的 Tag 名称")]
    [SerializeField] private string trashCanTag = "TrashCan";

    [Header("回弹动画")]
    [Tooltip("Ease-out 动画曲线")]
    [SerializeField] private AnimationCurve returnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("回弹时长（秒）")]
    [SerializeField] private float returnDuration = 0.25f;

    private Camera mainCamera;
    private bool isDragging;
    private Vector3 dragOffset;
    private float objectZ;
    private int orderLayerMask;

    /// <summary>
    /// 出生位置（用于 EndDrag 未命中任何目标时的回弹终点）
    /// </summary>
    private Vector3 spawnPosition;

    /// <summary>
    /// 返回动画进行中锁定交互
    /// </summary>
    private bool isReturning;

    private YogurtData yogurtData;

    private void Awake()
    {
        mainCamera = Camera.main;
        objectZ = transform.position.z;
        orderLayerMask = LayerMask.GetMask(orderLayerName);
        spawnPosition = transform.position;

        EnsureEventSystem();
        EnsurePhysics2DRaycaster();

        yogurtData = GetComponent<YogurtData>();

        if (orderLayerMask == 0)
        {
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
        spawnPosition = transform.position;

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
        if (!isDragging || isReturning || mainCamera == null)
        {
            return;
        }

        Vector3 mouseWorld = GetWorldPosition(eventData.position);
        Vector3 targetPos = mouseWorld + dragOffset;

        if (lockZAxis)
        {
            targetPos.z = objectZ;
        }

        transform.position = targetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isReturning)
        {
            return;
        }

        isDragging = false;

        if (!HandOver())
        {
            ReturnToSpawn();
        }
    }

    private void ReturnToSpawn()
    {
        StartCoroutine(ReturnToSpawnCoroutine());
    }

    private System.Collections.IEnumerator ReturnToSpawnCoroutine()
    {
        isReturning = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = returnCurve.Evaluate(Mathf.Clamp01(elapsed / returnDuration));
            transform.position = Vector3.Lerp(startPos, spawnPosition, t);
            yield return null;
        }

        transform.position = spawnPosition;
        isReturning = false;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 mouseScreen = screenPosition;
        float z = lockZAxis ? Mathf.Abs(mainCamera.transform.position.z - objectZ) : Mathf.Abs(mainCamera.transform.position.z);
        mouseScreen.z = z;
        return mainCamera.ScreenToWorldPoint(mouseScreen);
    }

    /// <summary>
    /// 执行垃圾桶检测和订单提交。
    /// </summary>
    /// <returns>若成功处理（垃圾桶或订单）返回 true；否则返回 false。</returns>
    private bool HandOver()
    {
        Collider2D selfCollider = GetComponent<Collider2D>();
        if (selfCollider == null)
        {
            return false;
        }

        // 先检测垃圾桶
        if (CheckAndDestroyAtTrashCan())
        {
            return true;
        }

        // 再检测订单
        if (orderLayerMask == 0)
        {
            return false;
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
            Destroy(gameObject);
            return true;
        }

        return false;
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
