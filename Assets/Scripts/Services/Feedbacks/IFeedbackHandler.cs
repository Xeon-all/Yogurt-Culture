public interface IFeedbackHandler
{
    bool Check<T>(T evt);
    void Execute();
}
