using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class VFXManager : Singleton<VFXManager>
{
    [Header("粒子特效配置")]
    [SerializeField] private VFXDefinition vfxDefinition;

    private Dictionary<string, VFXDefinition.VFXClip> vfxClipMap;
    private Dictionary<string, ObjectPool<ParticleSystem>> pools;

    protected override void Awake()
    {
        base.Awake();
        BuildVFXClipMap();
    }

    private void OnEnable()
    {
        OrderManager.Instance.OnOrderSuccess += (pos) =>
        {
            PlayCoinEffect(pos);
            PlayVFX("spark", pos);
        };
    }

    private void OnDisable()
    {
        // if (OrderManager.Instance != null)
        //     OrderManager.Instance.OnOrderSuccess = null;
    }

    private void BuildVFXClipMap()
    {
        vfxClipMap = new Dictionary<string, VFXDefinition.VFXClip>();
        pools = new Dictionary<string, ObjectPool<ParticleSystem>>();

        if (vfxDefinition == null || vfxDefinition.clips == null)
            return;

        foreach (var clip in vfxDefinition.clips)
        {
            if (string.IsNullOrEmpty(clip.key) || clip.prefab == null)
                continue;

            if (!vfxClipMap.ContainsKey(clip.key))
            {
                vfxClipMap.Add(clip.key, clip);
                pools.Add(clip.key, CreatePoolForClip(clip));
            }
        }
    }

    private ObjectPool<ParticleSystem> CreatePoolForClip(VFXDefinition.VFXClip clip)
    {
        return new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(clip.prefab, transform),
            actionOnGet: (ps) => ps.gameObject.SetActive(true),
            actionOnRelease: (ps) => ps.gameObject.SetActive(false),
            actionOnDestroy: (ps) => Destroy(ps.gameObject),
            defaultCapacity: 3,
            maxSize: 10
        );
    }

    public void PlayVFX(string key, Vector3 position)
    {
        if (!vfxClipMap.TryGetValue(key, out var clip))
        {
            Debug.LogWarning($"VFX键 '{key}' 不存在!");
            return;
        }

        if (!pools.TryGetValue(key, out var pool))
        {
            Debug.LogWarning($"VFX池 '{key}' 未初始化!");
            return;
        }

        ParticleSystem ps = pool.Get();
        ps.transform.position = position;
        ps.transform.localScale = Vector3.one * clip.scale;
        ps.Play();

        if (clip.autoReturnToPool)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPoolDelayed(pool, ps, duration));
        }
    }

    public void PlayVFX(string key, Vector2 position)
    {
        PlayVFX(key, new Vector3(position.x, position.y, 0));
    }

    private System.Collections.IEnumerator ReturnToPoolDelayed(ObjectPool<ParticleSystem> pool, ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(ps);
    }

    private void PlayCoinEffect(Vector2 pos)
    {
        // 兼容旧接口，金币特效暂用 "CoinReward" key
        PlayVFX("CoinReward", pos);
    }
}