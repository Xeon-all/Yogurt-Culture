using UnityEngine;
using UnityEngine.Pool; // Unity 2021+ 内置的对象池

public class VFXManager : Singleton<VFXManager>
{
    [Header("金币粒子预制体")]
    public ParticleSystem coinParticlePrefab;

    // 使用 Unity 内置的对象池
    private ObjectPool<ParticleSystem> pool;

    protected override void Awake()
    {
        base.Awake();
        // 初始化对象池
        pool = new ObjectPool<ParticleSystem>(
            createFunc: () => Instantiate(coinParticlePrefab, transform),
            actionOnGet: (ps) => ps.gameObject.SetActive(true),
            actionOnRelease: (ps) => ps.gameObject.SetActive(false),
            actionOnDestroy: (ps) => Destroy(ps.gameObject),
            defaultCapacity: 5, // 预热 5 个
            maxSize: 20
        );
    }

    private void OnEnable()
    {
        OrderManager.Instance.OnOrderSuccess += PlayCoinEffect;
    }

    private void OnDisable()
    {
        if(OrderManager.Instance)
            OrderManager.Instance.OnOrderSuccess -= PlayCoinEffect;
    }

    private void PlayCoinEffect(Vector2 pos)
    {
        // if (result.Order?.OrderEntity == null) return;

        ParticleSystem ps = pool.Get();
        ps.transform.position = pos;
        ps.Play();

        StartCoroutine(ReturnToPoolAfterDelay(ps, ps.main.duration));
    }

    private System.Collections.IEnumerator ReturnToPoolAfterDelay(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(ps); // 放回池子
    }
}