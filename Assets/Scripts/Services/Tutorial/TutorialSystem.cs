using VContainer;
using VContainer.Unity;
using System;
using YogurtCulture.GameLoop;
using System.Collections.Generic;

public class TutorialSystem : IInitializable, IDisposable
{
    private readonly IEventBus _eventBus;
    private readonly IEnumerable<ITutorialHandler> _handlers;

    public TutorialSystem(IEventBus eventBus, IEnumerable<ITutorialHandler> handlers)
    {
        _eventBus = eventBus;
        _handlers = handlers;
    }

    // DI 容器构建完成并启动时自动调用
    public void Initialize()
    {
        _eventBus.Subscribe<OnTransitionPhase>(CheckTutorial);
    }

    // DI 容器销毁（如切场景）时自动调用
    public void Dispose()
    {
        // _eventBus.Unsubscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }
    private void CheckTutorial<T>(T evt)
    {
        foreach(var h in _handlers)
            if(h.CheckCondition()) h.Execute();
        
    }
}