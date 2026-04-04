// VFXDefinition.cs
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewVFXSet", menuName = "VFX/VFX Definition")]
public class VFXDefinition : ScriptableObject
{
    [Serializable]
    public class VFXClip
    {
        public string key;                              // 标识，如 "CoinReward"
        public ParticleSystem prefab;                  // 粒子预制体
        [Range(0.1f, 3f)] public float scale = 1f;    // 整体缩放
        public bool autoReturnToPool = true;           // 是否自动回池
    }

    public VFXClip[] clips;
}