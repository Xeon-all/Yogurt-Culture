using UnityEngine;
public class OrderSpawnFeedback : IFeedbackHandler
{
    public bool Check<T>(T evt)
    {
        if(evt is OnOrderSpawn)
            return true;
        return false;
    }
    public void Execute()
    {
        AudioManager.Instance.PlaySFX("orderSpawn");
    }
}