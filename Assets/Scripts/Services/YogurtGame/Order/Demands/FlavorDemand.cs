using UnityEngine;
public class FlavorDemand : IOrderDemand
{
    public int GetScore(YogurtData yogurt)
    {
        return yogurt.Exflavor;
    }
}