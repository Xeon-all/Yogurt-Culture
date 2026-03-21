using System;
using System.Collections.Generic;
using UnityEngine;

namespace YogurtCulture.GameLoop
{
    /// <summary>
    /// 游戏循环管理器 - 核心状态机
    /// </summary>
    public class GameLoopManager : Singleton<GameLoopManager>
    {
        /// <summary>
        /// 阶段执行顺序列表（Editor 中配置）
        /// </summary>
        [Header("阶段顺序")]
        [SerializeField] private GamePhase[] phaseOrder = new GamePhase[]
        {
            GamePhase.Preparation,
            GamePhase.MorningOp,
            GamePhase.NoonBreak,
            GamePhase.AfternoonOp,
            GamePhase.Settlement,
            GamePhase.NightAction
        };
        private Dictionary<GamePhase, Type> builtInPhases = new Dictionary<GamePhase, Type>
        {
            { GamePhase.Init, typeof(InitHandler) },
            { GamePhase.Preparation, typeof(PreparationHandler) },
            { GamePhase.MorningOp, typeof(MorningOpHandler) },
            { GamePhase.NoonBreak, typeof(NoonBreakHandler) },
            { GamePhase.AfternoonOp, typeof(AfternoonOpHandler) },
            { GamePhase.Settlement, typeof(SettlementHandler) },
            { GamePhase.NightAction, typeof(NightActionHandler) }
        };
        
        [Header("数据")]
        [SerializeField] private GameLoopData gameData = new();
        
        [Header("准备阶段")]
        [SerializeField] private List<GameObject> preparationUI;
        [Header("经营阶段")]
        [SerializeField] public GameObject npcManager;
        private int _currentPhaseIndex;
        private GamePhase _currentPhase;
        private bool _isPaused;
        private IPhaseHandler _currentHandler;
        
        public GamePhase CurrentPhase => _currentPhase;
        public GameLoopData Data => gameData;
        public int CurrentPhaseIndex => _currentPhaseIndex;
        public GamePhase[] PhaseOrder => phaseOrder;
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        private void Start()
        {
            TransitionTo(GamePhase.Init);
        }
        
        private void Update()
        {
            if (_isPaused) return;
            
            // 调用当前 Handler 的 Update
            _currentHandler?.OnPhaseUpdate(gameData, Time.deltaTime);
        }
        
        /// <summary>
        /// 获取准备阶段 UI
        /// </summary>
        public List<GameObject> GetPreparationUI() => preparationUI;
        
        /// <summary>
        /// Debug: 手动进入下一阶段
        /// </summary>
        public void DebugNextPhase()
        {
            TransitToNext();
        }
        
        /// <summary>
        /// 手动跳转到指定阶段
        /// </summary>
        public void TransitionTo(GamePhase targetPhase)
        {
            // 退出旧阶段（Init 为初始阶段，不执行退出）
            if (_currentPhase != GamePhase.Init || targetPhase != GamePhase.Preparation)
            {
                ExitCurrentPhase();
            }

            // 切换阶段
            if (targetPhase == GamePhase.Init)
            {
                _currentPhaseIndex = -1; // Init 不在 phaseOrder 中
            }
            else
            {
                _currentPhaseIndex = System.Array.IndexOf(phaseOrder, targetPhase);
                if (_currentPhaseIndex < 0) return;
            }
            _currentPhase = targetPhase;
            gameData.currentPhase = targetPhase;

            // 进入新阶段
            EnterPhase(targetPhase);
        }
        
        /// <summary>
        /// 进入指定阶段 - 实例化并调用 Handler
        /// </summary>
        private void EnterPhase(GamePhase phase)
        {
            if (!builtInPhases.TryGetValue(phase, out var handlerType)) return;
            
            _currentHandler = Activator.CreateInstance(handlerType) as IPhaseHandler;
            _currentHandler?.OnPhaseEnter(gameData);
        }
        
        /// <summary>
        /// 退出当前阶段 - 实例化并调用 Handler
        /// </summary>
        private void ExitCurrentPhase()
        {
            if (_currentHandler == null) return;
            
            _currentHandler.OnPhaseExit(gameData);
            Debug.Log($"[GameLoop] Exit: {_currentPhase}");
        }
        
        public void TransitToNext()
        {
            GetNextPhase(out var nextPhase, out var _);
            TransitionTo(nextPhase);
        }
        
        /// <summary>
        /// 获取下一阶段
        /// </summary>
        private void GetNextPhase(out GamePhase nextPhase, out int nextIndex)
        {
            if (_currentPhase == GamePhase.Init)
            {
                // Init 是特殊入口阶段，下一阶段固定为 phaseOrder[0]
                nextIndex = 0;
                nextPhase = phaseOrder[0];
                return;
            }
            nextIndex = (_currentPhaseIndex + 1) % phaseOrder.Length;
            nextPhase = phaseOrder[nextIndex];
        }
        
        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
    }
}
