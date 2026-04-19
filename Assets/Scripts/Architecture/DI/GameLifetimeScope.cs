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

        builder.RegisterComponent(GameLoopManager.Instance);
        builder.RegisterComponent(VFXManager.Instance);
        builder.RegisterComponent(AudioManager.Instance);
    }
}
