using System;
using UnityEngine;

/// <summary>
/// 配料状态机：管理 IngredientController 的制作流程状态。
/// 状态转换：Idle → Enlarged → ProgressRunning → Shrinking → Done
/// </summary>
public class IngredientStateMachine
{
    /// <summary>
    /// 配料制作流程状态枚举
    /// </summary>
    public enum State
    {
        /// <summary>空闲状态，初始状态</summary>
        Idle,
        /// <summary>放大状态，工具触发放大动画</summary>
        Enlarged,
        /// <summary>进度运行状态，进度条正在执行</summary>
        ProgressRunning,
        /// <summary>缩小状态，制作完成触发放大</summary>
        Shrinking,
        /// <summary>完成状态，整个流程结束</summary>
        Done
    }

    /// <summary>
    /// 状态变更事件
    /// </summary>
    public event Action<State, State> OnStateChanged;

    /// <summary>
    /// 当前状态
    /// </summary>
    public State CurrentState { get; private set; } = State.Idle;

    /// <summary>
    /// 上一状态（用于回退判断）
    /// </summary>
    public State PreviousState { get; private set; } = State.Idle;

    /// <summary>
    /// 状态机是否正在运行
    /// </summary>
    public bool IsRunning => CurrentState != State.Done;

    /// <summary>
    /// 状态机是否已完成
    /// </summary>
    public bool IsFinished => CurrentState == State.Done;

    /// <summary>
    /// 状态机是否处于进度运行状态
    /// </summary>
    public bool IsProgressRunning => CurrentState == State.ProgressRunning;

    /// <summary>
    /// 状态机是否处于放大状态
    /// </summary>
    public bool IsEnlarged => CurrentState == State.Enlarged;

    /// <summary>
    /// 从 Idle 进入 Enlarged（工具碰撞触发）
    /// </summary>
    public void TransitionToEnlarged()
    {
        if (CurrentState == State.Idle)
        {
            ChangeState(State.Enlarged);
        }
    }

    /// <summary>
    /// 从 Enlarged 进入 ProgressRunning（放大动画完成触发）
    /// </summary>
    public void TransitionToProgressRunning()
    {
        if (CurrentState == State.Enlarged)
        {
            ChangeState(State.ProgressRunning);
        }
    }

    /// <summary>
    /// 从 ProgressRunning 进入 Shrinking（进度完成触发）
    /// </summary>
    public void TransitionToShrinking()
    {
        if (CurrentState == State.ProgressRunning)
        {
            ChangeState(State.Shrinking);
        }
    }

    /// <summary>
    /// 从 Shrinking 进入 Done（缩小动画完成触发）
    /// </summary>
    public void TransitionToDone()
    {
        if (CurrentState == State.Shrinking)
        {
            ChangeState(State.Done);
        }
    }

    /// <summary>
    /// 从任意状态直接回到 Idle（取消/重置）
    /// </summary>
    public void TransitionToIdle()
    {
        ChangeState(State.Idle);
    }

    /// <summary>
    /// 重置状态机到初始状态
    /// </summary>
    public void Reset()
    {
        ChangeState(State.Idle);
    }

    /// <summary>
    /// 内部状态变更方法
    /// </summary>
    private void ChangeState(State newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        State oldState = CurrentState;
        CurrentState = newState;
        PreviousState = oldState;

        OnStateChanged?.Invoke(oldState, newState);

        Debug.Log($"[IngredientStateMachine] State: {oldState} → {newState}");
    }

    /// <summary>
    /// 检查当前状态是否允许触发指定状态转换
    /// </summary>
    public bool CanTransitionTo(State targetState)
    {
        return (CurrentState, targetState) switch
        {
            (State.Idle, State.Enlarged) => true,
            (State.Enlarged, State.ProgressRunning) => true,
            (State.ProgressRunning, State.Shrinking) => true,
            (State.Shrinking, State.Done) => true,
            (_, State.Idle) => true, // 任意状态可以重置到 Idle
            _ => false
        };
    }
}
