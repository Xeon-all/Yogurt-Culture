using System.Collections.Generic;
using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    public CursorData data;
    private bool _locked;
    protected override void Awake()
    {
        base.Awake();
        var config = data.cursors.Find(c => c.type == CursorData.CursorType.Default);
        Cursor.SetCursor(config.texture, config.hotSpot, CursorMode.Auto);
    }

    public void SetCursor(CursorData.CursorType type, bool locking = false, bool unlock = false)
    {
        if(unlock) _locked = false;
        if(locking && unlock) Debug.LogWarning("同时给cursor上锁解锁是想干什么？？？");
        if(_locked && !unlock) return;
        if(locking) _locked = true;
        var config = data.cursors.Find(c => c.type == type);
        // 使用 Auto 模式，Unity 会根据平台自动处理
        Cursor.SetCursor(config.texture, config.hotSpot, CursorMode.Auto);
    }

}