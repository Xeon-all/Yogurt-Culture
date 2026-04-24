using UnityEngine;

public class TagDemand : IOrderDemand
{
    public int minVal;
    public int maxVal;
    public int score;
    public int panelty;
    public YogurtTag demandTag;
    public int GetScore(ProductData yogurt)
    {
        var point = yogurt.GetTagValue(demandTag);
        if(Mathf.Clamp(point, minVal, maxVal) == point)
            return score;
        else
        {
            return -panelty * (int)(Mathf.Abs((float)(minVal + maxVal)/2-point) - (maxVal - minVal)/2);
        }
    }
}