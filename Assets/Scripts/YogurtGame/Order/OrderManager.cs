using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 订单完成结果数据结构，供各订阅系统使用。
/// </summary>
[Serializable]
public class OrderResult
{
    public OrderManager.Order Order;
    public bool IsSuccess;
    public int DemandFlavor;
    public int ProvidedFlavor;
    /// <summary>成功时声望增量（已含溢出处理后的余量）</summary>
    public int ReputationGain;
    /// <summary>成功时金币奖励</summary>
    public int GoldReward;
    /// <summary>成功时等级变化量（0 或正数）</summary>
    public int LevelChange;
    /// <summary>失败时声望惩罚（不小于0）</summary>
    public int ReputationLoss;
    public Vector2 pos;
}

public class OrderManager : Singleton<OrderManager>
{
    public enum Difficulty { Low, Mid, High }

    /// <summary>
    /// 订单完成时发布。订阅者：ReputationSystem / MoneySystem / GameLoop。
    /// </summary>
    public event Action<OrderResult> OnOrderCompleted;
    public event Action<OrderResult> OnOrderHandover;
    public event Action<OrderResult> OnOrderSuccess;

    [Serializable]
    public class Order
    {
        public string ID;
        public GameObject OrderEntity;
        public Transform parent;
        public List<TagData> DemandTags = new();
        public Difficulty Difficulty = Difficulty.Low;
        public int Price = 10;
        public int FlavorExpec;
        public int SlotIndex = -1;
    }

    [Header("Order Slots")]
    [Tooltip("OrderRoot 下的子物体将自动作为订单生成点位")]
    [SerializeField] private Transform orderRoot;
    [Tooltip("最大同时存在的订单数量，0 表示不限制")]
    [SerializeField] private int maxOrderCount = 0;

    [Header("Timing")]
    [Tooltip("自动生成订单的间隔（秒），<=0 则不自动生成")]
    [SerializeField] private float autoAddInterval = 10f;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> orderPrefabs = new();

    [Header("Difficulty Distribution (relative weight)")]
    [Tooltip("低难度权重")]
    [SerializeField] private float difficultyLowWeight = 5f;
    [Tooltip("中难度权重")]
    [SerializeField] private float difficultyMidWeight = 3.5f;
    [Tooltip("高难度权重")]
    [SerializeField] private float difficultyHighWeight = 1.5f;

    private List<Transform> _orderSlots = new();
    private float _autoAddTimer;

    /// <summary>
    /// 当自动添加间隔触发时调用。在 MorningOp 阶段由 MorningOpHandler 订阅。
    /// </summary>
    public event Action OnAutoAddTick;

    /// <summary>
    /// 槽位清空时触发，参数为被清空的槽位索引。
    /// </summary>
    public event Action<int> OnOrderCleared;
    #region 初始化
    protected override void Awake()
    {
        base.Awake();
        _autoAddTimer = 0f;
        CollectOrderSlots();
        InitSuccessCallback();
    }
    private void InitSuccessCallback()
    {
        OnOrderSuccess += (result) =>
        {
            VFXManager.Instance.PlayVFX("CoinReward", result.pos);
            VFXManager.Instance.PlayVFX("star", result.pos);
        };
    }
    #endregion
    private void Update()
    {
        OnAutoAddTick?.Invoke();
    }

    private void CollectOrderSlots()
    {
        _orderSlots.Clear();
        if (orderRoot == null) return;
        foreach (Transform child in orderRoot)
        {
            _orderSlots.Add(child);
        }
    }

    private int EffectiveMaxOrder => maxOrderCount > 0 ? maxOrderCount : int.MaxValue;

    private Transform GetSlot(int index)
    {
        if (index < 0 || index >= _orderSlots.Count) return null;
        return _orderSlots[index];
    }

    private bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= _orderSlots.Count) return false;
        foreach (Transform child in _orderSlots[index])
        {
            if (child.GetComponent<OrderEntity>() != null) return true;
        }
        return false;
    }

    private int GetNextFreeSlot()
    {
        for (int i = 0; i < _orderSlots.Count; i++)
        {
            if (!IsSlotOccupied(i)) return i;
        }
        return -1;
    }

    private int ActiveOrderCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < _orderSlots.Count; i++)
            {
                if (IsSlotOccupied(i)) count++;
            }
            return count;
        }
    }

    #region 自动添加订单（计时器）

    /// <summary>
    /// 计时器触发时调用。MorningOpHandler 会在进入阶段时订阅，退出时取消订阅。
    /// </summary>
    public void TempAddOrder()
    {
        if (autoAddInterval <= 0f) return;

        _autoAddTimer += Time.deltaTime;
        if (_autoAddTimer >= autoAddInterval)
        {
            _autoAddTimer = 0f;
            AddOrder();
        }
    }

    /// <summary>
    /// 重置计时器（进入阶段时调用）。
    /// </summary>
    public void ResetAutoAddTimer()
    {
        _autoAddTimer = 0f;
    }

    #endregion

    #region 后台数据

    /// <summary>
    /// 生成订单数据并分配槽位，不处理游戏表现。
    /// 按低:中:高 = 5:3.5:1.5 加权随机选取难度，
    /// 低分配 2 点，中分配 3 点，高分配 4 点。
    /// 每点从 YogurtTag（除 None）中随机选一个 Tag 累加数值。
    /// </summary>
    private Order AppendOrderData()
    {
        if (orderPrefabs == null || orderPrefabs.Count == 0)
        {
            Debug.LogWarning("OrderManager: 没有可用的订单 prefab。");
            return null;
        }

        Order newOrder = new Order();
        int randomIndex = UnityEngine.Random.Range(0, orderPrefabs.Count);
        newOrder.OrderEntity = orderPrefabs[randomIndex];
        newOrder.ID = newOrder.OrderEntity.name;

        newOrder.Difficulty = RollDifficulty();
        newOrder.Price = GetPrice(newOrder.Difficulty);
        newOrder.FlavorExpec = GetTagPoints(newOrder.Difficulty);
        newOrder.DemandTags = GenerateDemandTags(newOrder.FlavorExpec);

        int slotIndex = GetNextFreeSlot();
        if (slotIndex < 0)
        {
            Debug.Log("OrderManager: 没有可用的订单槽位。");
            return null;
        }
        newOrder.SlotIndex = slotIndex;

        return newOrder;
    }

    /// <summary>
    /// 按权重随机抽取难度。
    /// </summary>
    private Difficulty RollDifficulty()
    {
        float totalWeight = difficultyLowWeight + difficultyMidWeight + difficultyHighWeight;
        float roll = UnityEngine.Random.Range(0f, totalWeight);

        float lowMax = difficultyLowWeight;
        float midMax = lowMax + difficultyMidWeight;

        if (roll < lowMax) return Difficulty.Low;
        if (roll < midMax) return Difficulty.Mid;
        return Difficulty.High;
    }

    /// <summary>
    /// 根据难度获取奖励金额。
    /// </summary>
    private int GetPrice(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Low => 10,
            Difficulty.Mid => 15,
            Difficulty.High => 20,
            _ => 10
        };
    }

    /// <summary>
    /// 根据难度获取 Tag 点数。
    /// </summary>
    private int GetTagPoints(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Low => 2,
            Difficulty.Mid => 3,
            Difficulty.High => 4,
            _ => 2
        };
    }

    /// <summary>
    /// 生成指定点数的 TagData 需求。
    /// 优先从当前已激活的 Topping 支持的 Tag 中抽取，
    /// 若池为空则回退到非 None 的 YogurtTag 枚举池。
    /// 每点随机选一个 Tag 累加数值后合并同 Tag。
    /// </summary>
    private List<TagData> GenerateDemandTags(int totalPoints)
    {
        var result = new List<TagData>();

        // Step 1: 从已激活 Topping 的 Tag 集合构建候选池
        var candidatePool = new List<YogurtTag>();
        var activeToppings = YogurtGameBoard.Instance != null
            ? YogurtGameBoard.Instance.GetAllActiveToppings()
            : null;

        if (activeToppings != null && activeToppings.Count > 0)
        {
            foreach (var topping in activeToppings)
            {
                var tags = YogurtGameBoard.Instance.GetToppingTags(topping.ID);
                if (tags == null) continue;
                foreach (var tagData in tags)
                {
                    if (tagData.Tag != YogurtTag.None && !candidatePool.Contains(tagData.Tag))
                    {
                        candidatePool.Add(tagData.Tag);
                    }
                }
            }
        }

        // Step 2: 若候选池为空，回退到枚举池
        if (candidatePool.Count == 0)
        {
            candidatePool.AddRange(
                Enum.GetValues(typeof(YogurtTag))
                    .Cast<YogurtTag>()
                    .Where(t => t != YogurtTag.None)
            );
        }

        if (candidatePool.Count == 0)
        {
            Debug.LogWarning("OrderManager: 没有可用的 Tag 来生成订单需求。");
            return result;
        }

        // Step 3: 按点数随机分配
        for (int i = 0; i < totalPoints; i++)
        {
            YogurtTag randomTag = candidatePool[UnityEngine.Random.Range(0, candidatePool.Count)];
            int existingIdx = result.FindIndex(t => t.Tag == randomTag);
            if (existingIdx >= 0)
            {
                var existing = result[existingIdx];
                result[existingIdx] = new TagData(existing.Tag, existing.Value + 1);
            }
            else
            {
                result.Add(new TagData(randomTag, 1));
            }
        }

        return result;
    }

    #endregion

    #region 游戏表现

    /// <summary>
    /// 在指定槽位生成订单实体并注入数据。
    /// </summary>
    private void SpawnOrderEntity(Order order)
    {
        if (order == null) return;

        Transform slot = GetSlot(order.SlotIndex);
        if (order.OrderEntity == null || slot == null) return;

        GameObject entity = Instantiate(order.OrderEntity, slot);
        order.parent = slot;
        entity.transform.localPosition = Vector3.zero;
        entity.transform.localRotation = Quaternion.identity;

        entity.GetComponent<OrderEntity>()?.Setup(order);
    }
    
    #endregion

    /// <summary>
    /// 公开接口：创建一条订单，依次调用后台数据生成与游戏表现。
    /// </summary>
    public Order AddOrder()
    {
        if (ActiveOrderCount >= EffectiveMaxOrder) return null;

        Order newOrder = AppendOrderData();
        if (newOrder == null) return null;

        AudioManager.Instance.PlaySFX("orderSpawn");

        SpawnOrderEntity(newOrder);
        return newOrder;
    }

    /// <summary>
    /// 0 时刻触发一次自动添加。
    /// </summary>
    [ContextMenu("Trigger Initial Order")]
    public void TriggerInitialOrder()
    {
        AddOrder();
    }

    /// <summary>
    /// 清理所有已存在的订单实体（MorningOp 退出时调用）。
    /// </summary>
    public void ClearAllOrders()
    {
        foreach (Transform slot in _orderSlots)
        {
            foreach (Transform child in slot)
            {
                var entity = child.GetComponent<OrderEntity>();
                if (entity != null)
                    Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 手动生成订单需求（保留以防外部调用）。
    /// </summary>
    public List<TagData> OrderDemands()
    {
        return new List<TagData>();
    }

    #region 订单完成发布

    /// <summary>
    /// 获取指定等级所需的声望值。
    /// </summary>
    
    public void OrderHandOver(OrderResult result)
    {
        OnOrderHandover?.Invoke(result);
    }
    public void OrderSuccess(OrderResult result)
    {
        OnOrderSuccess?.Invoke(result);
    }
    public void OrderComplete(OrderResult result)
    {
        OnOrderCompleted?.Invoke(result);
    }
    /// <summary>
    /// 由 OrderEntity 在提交时调用，计算奖励/惩罚后发布结果。
    /// 奖励计算只在此处发生一次，各系统只消费，不重复计算。
    /// </summary>
    public OrderResult GetOrderResult(
        Order order,
        bool success,
        int providedFlavor)
    {
        var result = new OrderResult
        {
            Order = order,
            IsSuccess = success,
            DemandFlavor = order.FlavorExpec,
            ProvidedFlavor = providedFlavor
        };
        result.pos = order.parent.position;
        if (success)
        {
            result.GoldReward = order.Price;
            result.ReputationGain = CalculateSuccessReputation(order.Difficulty, order.FlavorExpec, providedFlavor);
        }
        else
        {
            result.ReputationLoss = Mathf.Max(0, (order.FlavorExpec - providedFlavor) * 2);
        }

        // OnOrderCompleted?.Invoke(result);
        return result;
    }

    /// <summary>
    /// 计算成功时的声望增量及升级次数。
    /// </summary>
    private int CalculateSuccessReputation(
        Difficulty difficulty,
        int demandFlavor,
        int providedFlavor)
    {
        int basePoints = difficulty switch
        {
            Difficulty.Low => 2,
            Difficulty.Mid => 3,
            Difficulty.High => 4,
            _ => 2
        };

        int baseRep = basePoints * 3;
        int extraFlavor = Mathf.Max(0, providedFlavor - demandFlavor);
        int bonusRep = extraFlavor * 2;
        int totalRep = baseRep + bonusRep;

        // // 计算升级次数：溢出部分保留为余量
        // levelUps = 0;
        // int currentLevel = ReputationSystem.Instance != null
        //     ? ReputationSystem.Instance.CurrentLevel
        //     : 1;
        // float expNeeded = GetExpForLevel(currentLevel + 1);

        // while (totalRep >= expNeeded)
        // {
        //     totalRep -= expNeeded;
        //     levelUps++;
        //     expNeeded = GetExpForLevel(currentLevel + levelUps + 1);
        //     Debug.Log("Level UP!!");
        // }

        return totalRep;
    }

    #endregion

    /// <summary>
    /// OrderEntity 提交成功后调用，清空指定槽位并通知外部。
    /// </summary>
    public void ClearSlot(int slotIndex)
    {
        OnOrderCleared?.Invoke(slotIndex);
    }

    #region 废弃方法

    /// <summary>
    /// [废弃] 使用 AddOrder 代替。
    /// </summary>
    [Obsolete("Use AddOrder instead.")]
    protected Order TempAddOrder_obsolete()
    {
        return new Order();
    }

    /// <summary>
    /// [废弃] 不再需要调用此方法，订单槽位自动从 OrderRoot 收集。
    /// </summary>
    [Obsolete("Order slots are auto-collected from OrderRoot children.")]
    public void StartTempOrder() { }

    #endregion
}
