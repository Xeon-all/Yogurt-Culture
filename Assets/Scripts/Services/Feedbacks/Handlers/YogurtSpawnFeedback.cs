using UnityEngine;

public class YogurtSpawnFeedback : IFeedbackHandler
{
    private OnYogurtSpawn result;
    public bool Check<T>(T evt)
    {
        if(evt is OnYogurtSpawn r)
        {
            result = r;
            return true;
        }
        return false;
    }
    public void Execute()
    {
        AudioManager.Instance.PlaySFX("yogurtSpawn");
        var ps = VFXManager.Instance.AppendVFX("sparkle", result.yogurt);
        var s = ps.shape;
        s.scale = new Vector3(s.scale.x/2, s.scale.y * 0.7f, s.scale.z);
    }
}