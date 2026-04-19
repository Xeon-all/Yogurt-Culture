public class OrderCompleteFeedback : IFeedbackHandler
{
    private OrderResult result;
    public bool Check<T>(T evt)
    {
        if(evt is OrderResult r)
        {
            result = r;
            return true;
        }
        return false;
    }
    public void Execute()
    {
        if(result.IsSuccess)
        {
            AudioManager.Instance.PlaySFX("orderSuccess");
            VFXManager.Instance.PlayVFX("CoinReward", result.pos);
            VFXManager.Instance.PlayVFX("star", result.pos);
        }
        else
        {

        }
    }
}