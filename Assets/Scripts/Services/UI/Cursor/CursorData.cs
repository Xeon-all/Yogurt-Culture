using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CursorData", menuName = "Custom/CursorData")]
public class CursorData : ScriptableObject
{
    public enum CursorType { Default, Grab, Dragging }
    
    [System.Serializable]
    public struct CursorConfig
    {
        public CursorType type;
        public Texture2D texture;
        public Vector2 hotSpot; // 偏移量
    }

    public List<CursorConfig> cursors;
}