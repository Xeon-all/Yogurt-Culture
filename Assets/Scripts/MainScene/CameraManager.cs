using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Cinemachine;
using YogurtCulture.GameLoop;

public class CameraManager : MonoBehaviour {
    [Header("相机配置")]
    private Camera mainCamera;
    public CinemachineVirtualCamera cinemachineVirtualCamera;

    [Header("Cinemachine 虚拟相机")]
    public CinemachineVirtualCamera vcamFull;
    public CinemachineVirtualCamera vcamFocus;

    [Header("视差图层列表")]
    // 这会在 Inspector 中呈现类似 Button OnClick 的列表
    public List<ParallaxLayerData> layers = new List<ParallaxLayerData>();
    [System.Serializable]
    public class ParallaxLayerData {
        public string layerName;        // 仅用于识别
        public Transform layerRoot;     // 该层级所有物件的父物体
        [Range(0, 1)]
        public float FactorX;   // X轴跟随因子：0=不动，1=完全跟随
        [Range(0, 1)]
        public float FactorY;   // Y轴跟随因子：0=不动，1=完全跟随
    }

    private Vector3 _cameraStartPos;
    private List<Vector3> _layerStartPositions = new List<Vector3>();
    private List<Vector3> _layerStartScales = new List<Vector3>();

    void Awake() {
        if (vcamFull != null) vcamFull.gameObject.SetActive(true);
        if (vcamFocus != null) vcamFocus.gameObject.SetActive(false);
    }

    /// <summary>
    /// 从 vcamFull 切换到 vcamFocus，使用 CinemachineBrain 的 defaultBlend，切换完成后回调
    /// </summary>
    /// <param name="onComplete">切换完成后的回调</param>
    public void TransitionToFocus() {
        var brain = gameObject.GetComponent<CinemachineBrain>();
        if(GameLoopManager.Instance.CurrentPhase != GamePhase.Init || brain.IsBlending) return;
        if (vcamFull == null || vcamFocus == null) return;
        vcamFull.gameObject.SetActive(false);
        vcamFocus.gameObject.SetActive(true);
        StartCoroutine(WaitForBlendComplete(() => GameLoopManager.Instance.TransitToNext()));
    }

    private IEnumerator WaitForBlendComplete(Action onComplete) {
        var brain = gameObject.GetComponent<CinemachineBrain>();
        if (brain != null) {
            do {
                yield return new WaitForSecondsRealtime(0.01f);
            } while (brain.IsBlending);
        }
        onComplete?.Invoke();
    }

    /// <summary>
    /// 设置相机跟随目标，传入null则停止跟随
    /// </summary>
    /// <param name="target">跟随的目标GameObject，null表示停止跟随</param>
    public void SetFollowTarget(GameObject target) {
        if (cinemachineVirtualCamera != null) {
            cinemachineVirtualCamera.Follow = target != null ? target.transform : null;
        }
    }

    /// <summary>
    /// 调试控制器：使用WASD移动相机，ZX控制镜头缩放
    /// </summary>
    public void DebugController() {
        if (mainCamera == null) return;

        // WASD控制相机移动
        float moveSpeed = 10f; // 移动速度，可以根据需要调整
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

        // 应用移动
        if (moveDirection != Vector3.zero) {
            mainCamera.transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
        }

        // ZX控制镜头缩放
        float zoomSpeed = 5f; // 缩放速度
        float minZoom = 1f;   // 最小正交尺寸
        float maxZoom = 20f;  // 最大正交尺寸

        if (Input.GetKey(KeyCode.Z)) {
            // Z键放大（缩小视野）
            mainCamera.orthographicSize = Mathf.Max(minZoom, mainCamera.orthographicSize - zoomSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.X)) {
            // X键缩小（放大视野）
            mainCamera.orthographicSize = Mathf.Min(maxZoom, mainCamera.orthographicSize + zoomSpeed * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.Space)){
            TransitionToFocus();
        }

        // 检测缩放结束
        // if (!isZooming && _isZooming) {
        //     _isZooming = false;
        //     _initialOrthoSize = mainCamera.orthographicSize;
        // }
    }

    void Start() {
        mainCamera = GetComponent<Camera>();

        // 如果没有手动设置cinemachineVirtualCamera，尝试自动获取
        if (cinemachineVirtualCamera == null) {
            cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        _cameraStartPos = mainCamera.transform.position;

        // 记录所有图层的初始位置和缩放
        foreach (var layer in layers) {
            _layerStartPositions.Add(layer.layerRoot.position);
            _layerStartScales.Add(layer.layerRoot.localScale);
        }
    }

    void Update() {
        // 调试控制器：键盘控制相机移动和缩放
        DebugController();
    }

    void LateUpdate() { // 使用 LateUpdate 确保在相机移动后更新图层
        Vector3 cameraDelta = mainCamera.transform.position - _cameraStartPos;

        for (int i = 0; i < layers.Count; i++) {
            if (layers[i].layerRoot == null) continue;

            // 基准位置 = 初始位置 + 相机移动偏移（XY分别应用不同的跟随因子）
            Vector3 basePos = _layerStartPositions[i];
            basePos.x += cameraDelta.x * layers[i].FactorX;
            basePos.y += cameraDelta.y * layers[i].FactorY;

            // 保持 Z 轴不变
            basePos.z = _layerStartPositions[i].z;
            layers[i].layerRoot.position = basePos;
        }
    }

}