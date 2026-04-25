using VContainer;
using VContainer.Unity;
using YogurtCulture.GameLoop;

public class GameLifetimeScope : LifetimeScope
{

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IEventBus, GlobalEventBus>(Lifetime.Singleton);
        
        builder.Register<ITutorialHandler, StartGameTor>(Lifetime.Scoped);
        builder.RegisterEntryPoint<TutorialSystem>();

        builder.Register<IFeedbackHandler, OrderCompleteFeedback>(Lifetime.Scoped);
        builder.Register<IFeedbackHandler, YogurtSpawnFeedback>(Lifetime.Scoped);
        builder.Register<IFeedbackHandler, OrderSpawnFeedback>(Lifetime.Scoped);
        builder.RegisterEntryPoint<FeedbackSystem>();

        builder.RegisterComponent(GameLoopManager.Instance);
        builder.RegisterComponent(OrderManager.Instance);
        builder.RegisterComponent(YogurtFactory.Instance);
        builder.RegisterComponent(VFXManager.Instance);
        builder.RegisterComponent(AudioManager.Instance);
        builder.RegisterComponent(YogurtGameBoard.Instance);
        builder.RegisterComponent(FindObjectOfType<PreparationUI>(true));
        builder.RegisterComponent(FindObjectOfType<UpgradeUI>(true));
    }
}
