using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public interface IFeedback
{
   void CallFeedbacks<T>(T evt);
}

public class FeedbackSystem : IFeedback, IInitializable
{
    private readonly IEventBus _eventBus;
    private readonly IEnumerable<IFeedbackHandler> _handlers;
    public FeedbackSystem(IEventBus eventBus, IEnumerable<IFeedbackHandler> handlers)
    {
        _eventBus = eventBus;
        _handlers = handlers;
    }

    public void Initialize()
    {
        _eventBus.Subscribe<OrderResult>(CallFeedbacks);
        _eventBus.Subscribe<OnYogurtSpawn>(CallFeedbacks);
        _eventBus.Subscribe<OnOrderSpawn>(CallFeedbacks);
    }
    public void CallFeedbacks<T>(T evt)
    {
        foreach(var h in _handlers)
            if(h.Check(evt)) h.Execute();
    }
}