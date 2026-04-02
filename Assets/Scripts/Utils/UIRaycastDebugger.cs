using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // 必须引用

public class UIRaycastDebugger : MonoBehaviour
{
    void Update()
    {
        // 当点击鼠标左键时触发检测
        if (Input.GetMouseButtonDown(0))
        {
            // 确保场景中有 EventSystem
            if (EventSystem.current == null)
            {
                Debug.LogWarning("场景中没有 EventSystem！UI 无法响应点击。");
                return;
            }

            // 构造一个模拟当前鼠标位置的事件数据
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            // 创建一个列表来接收被射线击中的所有结果
            List<RaycastResult> results = new List<RaycastResult>();
            
            // 发射射线！穿透所有 UI
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"<color=cyan>--- 鼠标点击处共穿透了 {results.Count} 个 UI 组件 ---</color>");
            
            // 按拦截顺序打印（索引 0 就是最顶层、最先挡住鼠标的那个）
            for (int i = 0; i < results.Count; i++)
            {
                GameObject hitObj = results[i].gameObject;
                Debug.Log($"[层数 {i}] 拦截者: <color=yellow>{hitObj.name}</color> ");
            }
            
            if (results.Count == 0)
            {
                Debug.Log("未检测到任何可点击的 UI (可能是没点中，或者 Graphic Raycaster 没开)");
            }
        }
    }
}