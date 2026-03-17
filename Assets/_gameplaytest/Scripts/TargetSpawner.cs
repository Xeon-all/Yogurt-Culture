using UnityEngine;
using System.Collections.Generic;

public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private bool randomDirection = true;
    [SerializeField] private float rotationSpeed = 0.2f;

    private List<GameObject> _spawnedTargets = new();
    private bool _isSpawning = false;
    private float _currentAngle = 0f;
    private float _timeSinceLastSpawn = 0f;
    private bool _isClockwise = true;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        // 自动获取自身 scale 作为半径乘数
        float scaleMultiplier = _transform.localScale.x;
        radius *= scaleMultiplier;
    }

    private void Update()
    {
        if (!_isSpawning) return;

        _timeSinceLastSpawn += Time.deltaTime;

        // 旋转逻辑
        float currentRotationSpeed = _isClockwise ? 1 : -1;
        _currentAngle += currentRotationSpeed * Time.deltaTime * rotationSpeed;

        // 随机切换方向
        if (randomDirection && Random.value < 0.01f)
        {
            _isClockwise = !_isClockwise;
        }

        // 按固定间隔实例化
        if (_timeSinceLastSpawn >= spawnInterval)
        {
            SpawnTarget();
            _timeSinceLastSpawn = 0f;
        }
    }

    public void StartSpawning()
    {
        _isSpawning = true;
        _currentAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        _timeSinceLastSpawn = 0f;
        _isClockwise = true;
    }

    public void StopSpawning()
    {
        _isSpawning = false;
    }

    public void EndAndClear()
    {
        StopSpawning();
        ClearAllTargets();
    }

    private void SpawnTarget()
    {
        if (prefabToSpawn == null) return;

        // 随机角度位置
        var actualRad = radius * (1 + Random.Range(-0.2f, 0.2f));
        float x = Mathf.Cos(_currentAngle) * actualRad;
        float y = Mathf.Sin(_currentAngle) * actualRad;
        Vector3 spawnPos = _transform.position + new Vector3(x, y, 0);

        GameObject target = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        target.transform.SetParent(gameObject.transform);
        _spawnedTargets.Add(target);
    }

    private void ClearAllTargets()
    {
        foreach (var target in _spawnedTargets)
        {
            if (target != null)
            {
                Destroy(target);
            }
        }
        _spawnedTargets.Clear();
    }

    // 编辑器中可视化半径
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float scaleMultiplier = transform.localScale.x;
        Gizmos.DrawWireSphere(transform.position, radius * scaleMultiplier);
    }
}
