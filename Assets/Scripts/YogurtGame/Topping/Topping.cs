using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Topping 基类：所有配料装饰的基类
/// 实现拖拽功能，鼠标松开后检测是否添加到YogurtBase
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class Topping : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region 字段和变量
    
    [Header("拖拽设置")]
    [Tooltip("拖拽时物体与鼠标之间是否保持初始偏移")]
    [SerializeField] protected bool keepOffsetFromMouse = true;
    
    [Tooltip("是否在拖拽过程中锁定Z轴位置")]
    [SerializeField] protected bool lockZAxis = true;

    [Header("检测设置")]
    [Tooltip("YogurtBase 的 Layer 名称（用于检测是否添加到酸奶碗）")]
    [SerializeField] protected string ingredientLayerName = "ingredient";

    [SerializeField] private float sizeInBowl = 0.1f;
    
    protected Camera mainCamera;
    protected bool isDragging;
    protected Vector3 dragOffset;
    protected float objectZ;
    protected int ingredientLayerMask;
    
    // 状态记录（无论是否隐藏都会持续记录）
    protected Dictionary<string, object> stateData = new Dictionary<string, object>();
    protected bool isAddedToYogurtBase = false;
    protected YogurtBase parentYogurtBase;

    // 保存相对于 YogurtBase 中心的极坐标（半径和角度），以便多次打开保持不变
    protected float savedPolarRadius = 0f;
    protected float savedPolarAngle = 0f; // radians
    protected bool hasSavedPolar = false;
    
    // 是否已经设置过scale（用于首次Show时设置）
    protected bool hasSetScale = false;
    
    // 是否已经执行过第一次Show的效果
    protected bool hasPerformedFirstShow = false;
    
    #endregion
    
    #region Unity生命周期和初始化
    
    protected virtual void Awake()
    {
        mainCamera = Camera.main;
        objectZ = transform.position.z;
        ingredientLayerMask = LayerMask.GetMask(ingredientLayerName);
        
        // 确保 EventSystem 存在
        EnsureEventSystem();
        
        // 确保 Camera 有 Physics2DRaycaster
        EnsurePhysics2DRaycaster();
        
        // 初始化状态
        InitializeState();
    }
    
    /// <summary>
    /// 更新Topping状态（持续调用，无论是否隐藏）
    /// </summary>
    protected virtual void Update()
    {
        // 持续更新状态，即使隐藏也会记录
        UpdateState();
    }
    
    /// <summary>
    /// 初始化状态（子类可重写）
    /// </summary>
    protected virtual void InitializeState()
    {
        stateData["isActive"] = gameObject.activeSelf;
        stateData["position"] = transform.position;
        stateData["rotation"] = transform.rotation;
        stateData["scale"] = transform.localScale;
    }
    
    /// <summary>
    /// 确保场景中有 EventSystem
    /// </summary>
    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }
    
    /// <summary>
    /// 确保 Camera 有 Physics2DRaycaster（用于2D拖拽检测）
    /// </summary>
    private void EnsurePhysics2DRaycaster()
    {
        if (mainCamera != null && mainCamera.GetComponent<Physics2DRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }
    
    #endregion
    
    #region 功能方法
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
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
        
        // 更新状态
        UpdateState();
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

        transform.position = targetPos;
        
        // 更新状态
        UpdateState();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        
        // 检测是否与YogurtBase重合
        CheckAndAddToYogurtBase();
        
        // 更新状态
        UpdateState();
    }
    
    /// <summary>
    /// 将屏幕坐标转换为世界坐标
    /// </summary>
    protected Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Vector3 mouseScreen = screenPosition;
        float z = lockZAxis ? Mathf.Abs(mainCamera.transform.position.z - objectZ) : Mathf.Abs(mainCamera.transform.position.z);
        mouseScreen.z = z;
        return mainCamera.ScreenToWorldPoint(mouseScreen);
    }
    
    /// <summary>
    /// 检测是否与YogurtBase重合，如果重合则添加到YogurtBase（公共方法，供外部调用）
    /// </summary>
    public void CheckAndAddToYogurtBase()
    {
        Collider2D selfCollider = GetComponent<Collider2D>();
        if (selfCollider == null)
        {
            // 如果没有重合，销毁实体
            Destroy(gameObject);
            return;
        }

        if (ingredientLayerMask == 0)
        {
            // 如果没有配置Layer，直接销毁
            Destroy(gameObject);
            return;
        }

        // 检测与YogurtBase图层的碰撞
        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = ingredientLayerMask,
            useTriggers = true
        };
        
        Collider2D[] results = new Collider2D[4];
        int hitCount = selfCollider.OverlapCollider(filter, results);
        
        if (hitCount > 0)
        {
            // 找到第一个有效的YogurtBase
            foreach (var hit in results)
            {
                if (hit == null) continue;

                // 直接获取YogurtBase组件
                YogurtBase yogurtBase = hit.GetComponent<YogurtBase>();
                if (yogurtBase != null)
                {
                    parentYogurtBase = yogurtBase;
                    SetParentIngredient(yogurtBase);
                    isAddedToYogurtBase = true;

                    // 隐藏实体（由YogurtFactory统一管理生命周期）
                    gameObject.SetActive(false);

                    UpdateState();
                    return;
                }
            }
        }
        
        // 如果没有重合，销毁实体
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 更新状态（持续记录，无论是否隐藏）
    /// </summary>
    protected virtual void UpdateState()
    {
        stateData["isActive"] = gameObject.activeSelf;
        stateData["position"] = transform.position;
        stateData["rotation"] = transform.rotation;
        stateData["scale"] = transform.localScale;
        stateData["isDragging"] = isDragging;
        stateData["isAddedToYogurtBase"] = isAddedToYogurtBase;
    }
    
    /// <summary>
    /// 获取状态数据（供外部查询）
    /// </summary>
    public Dictionary<string, object> GetState()
    {
        UpdateState(); // 确保状态是最新的
        return new Dictionary<string, object>(stateData);
    }
     
    /// <summary>
    /// 设置状态数据（供外部设置）
    /// </summary>
    public void SetState(Dictionary<string, object> newState)
    {
        if (newState != null)
        {
            stateData = new Dictionary<string, object>(newState);
        }
    }
    
    /// <summary>
    /// 检查是否已添加到YogurtBase
    /// </summary>
    public bool IsAddedToYogurtBase()
    {
        return isAddedToYogurtBase;
    }
    
    /// <summary>
    /// 获取父YogurtBase
    /// </summary>
    public YogurtBase GetParentYogurtBase()
    {
        return parentYogurtBase;
    }

    /// <summary>
    /// 设置父YogurtBase（由YogurtFactory调用）
    /// </summary>
    public void SetParentIngredient(YogurtBase yogurtBase)
    {
        parentYogurtBase = yogurtBase;
        UpdateState();
    }

    /// <summary>
    /// 显示Topping（由YogurtFactory调用）
    /// </summary>
    public virtual void Show(YogurtBase yogurtBase)
    {
        gameObject.SetActive(true);

        if (!hasPerformedFirstShow)
        {
            OnFirstShow(yogurtBase);
            hasPerformedFirstShow = true;
        }
        else
        {
            if (yogurtBase != null)
            {
                Vector3 center = yogurtBase.transform.position;
                if (hasSavedPolar)
                {
                    Vector3 worldPos = center + new Vector3(Mathf.Cos(savedPolarAngle) * savedPolarRadius, Mathf.Sin(savedPolarAngle) * savedPolarRadius, transform.position.z - center.z);
                    transform.position = worldPos;
                }
                else
                {
                    Vector3 offset = transform.position - center;
                    savedPolarRadius = new Vector2(offset.x, offset.y).magnitude;
                    savedPolarAngle = Mathf.Atan2(offset.y, offset.x);
                    hasSavedPolar = true;
                }
            }
        }

        UpdateState();
    }

    /// <summary>
    /// 第一次调用Show时的效果回调函数（子类可重写）
    /// </summary>
    protected virtual void OnFirstShow(YogurtBase yogurtBase)
    {
        transform.localScale = Vector3.one * sizeInBowl;
        hasSetScale = true;
    }

    /// <summary>
    /// 隐藏Topping（由YogurtFactory调用）
    /// </summary>
    public virtual void Hide()
    {
        if (transform.parent != null && parentYogurtBase != null)
        {
            Vector3 center = parentYogurtBase.transform.position;
            Vector3 offset = transform.position - center;
            savedPolarRadius = new Vector2(offset.x, offset.y).magnitude;
            savedPolarAngle = Mathf.Atan2(offset.y, offset.x);
            hasSavedPolar = true;
        }

        gameObject.SetActive(false);
        UpdateState();
    }
    
    /// <summary>
    /// 加载Topping的具体实现（虚函数，子类实现）
    /// 在Ingredient首次放大时调用
    /// </summary>
    public abstract void LoadTopping();
    
    #endregion
}

