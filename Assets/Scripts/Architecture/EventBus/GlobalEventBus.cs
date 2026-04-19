using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventBus 
{ 
    void Publish<T>(T evt);
    void Subscribe<T>(Action<T> handler);
    void Unsubscribe<T>(Action<T> handler);
}

// 具体的实现类，它不是单例，没有静态的 Instance
public class GlobalEventBus : IEventBus 
{ 
    private readonly Dictionary<Type, Delegate> _subscribers = new Dictionary<Type, Delegate>();
    public void Publish<T>(T evt)
    {
        Type eventType = typeof(T);
        // 如果有人监听了这个事件
        if (_subscribers.TryGetValue(eventType, out Delegate existingDelegate))
        {
            Action<T> handlers = existingDelegate as Action<T>;
            handlers?.Invoke(evt);
        }
    }
    public void Subscribe<T>(Action<T> handler)
    {
        if (handler == null) return;

        Type eventType = typeof(T);

        if (_subscribers.TryGetValue(eventType, out Delegate existingDelegate))
            _subscribers[eventType] = Delegate.Combine(existingDelegate, handler);
        else
            _subscribers[eventType] = handler;
        
    }
    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler == null) return;

        Type eventType = typeof(T);

        // 尝试找到对应的委托链
        if (_subscribers.TryGetValue(eventType, out Delegate existingDelegate))
        {
            // 从委托链中移除该方法
            Delegate newDelegate = Delegate.Remove(existingDelegate, handler);

            if (newDelegate == null)
                _subscribers.Remove(eventType);
            
            else
                _subscribers[eventType] = newDelegate;
        }
    }
}