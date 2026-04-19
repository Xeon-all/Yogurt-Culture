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

    public ParticleSystem PlayVFX(string key, Vector3 position)
    {
        ParticleSystem ps = InitParticle(key, out var clip, out var pool);
        if(!ps) return ps;
        ps.transform.position = position;
        ps.Play();

        if (clip.autoReturnToPool)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPoolDelayed(pool, ps, duration));
        }
        return ps;
    }

    public void PlayVFX(string key, Vector2 position)
    {
        PlayVFX(key, new Vector3(position.x, position.y, 0));
    }

    public ParticleSystem AppendVFX(string key, Transform parent, Vector3 offset = default)
    {
        var ps = PlayVFX(key, offset);
        ps.transform.parent = parent;
        ps.transform.localPosition = offset;
        ps.transform.localScale = Vector3.one;
        return ps;
    }

    private ParticleSystem InitParticle(string key, out VFXDefinition.VFXClip clip, out ObjectPool<ParticleSystem> pool)
    {
        if (!vfxClipMap.TryGetValue(key, out var c))
        {
            Debug.LogWarning($"VFX键 '{key}' 不存在!");
            clip = null;
            pool = null;
            return null;
        }

        if (!pools.TryGetValue(key, out var p))
        {
            Debug.LogWarning($"VFX池 '{key}' 未初始化!");
            clip = null;
            pool = null;
            return null;
        }
        clip = c;
        pool = p;
        var ps = p.Get();
        ps.transform.localScale = Vector3.one * clip.scale;
        return ps;
    }

    private System.Collections.IEnumerator ReturnToPoolDelayed(ObjectPool<ParticleSystem> pool, ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(ps);
    }
}