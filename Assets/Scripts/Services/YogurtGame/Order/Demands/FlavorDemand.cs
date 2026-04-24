using UnityEngine;
public class FlavorDemand : IOrderDemand
{
    public int GetScore(ProductData yogurt)
    {
        return yogurt.Flavor;
    }
}