using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class BuildingIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Brackets")]
    [Tooltip("四个角使用同一张图旋转获得，拖入左上角素材即可")]
    public Sprite cornerSprite;

    [Tooltip("每个角的世界空间尺寸（pixelsToUnits 影响）")]
    public float cornerSize = 1.5f;

    [Header("Bound Constraints")]
    [Tooltip("约束方框左下角（相对于 transform.localPosition 的偏移）")]
    public Vector2 boundMin = new Vector2(-0.5f, -0.5f);

    [Tooltip("约束方框右上角（相对于 transform.localPosition 的偏移）")]
    public Vector2 boundMax = new Vector2( 0.5f,  0.5f);

    [Header("Float Effect")]
    [Tooltip("开启沿连线方向的内外浮动效果")]
    public bool useFloatEffect = true;

    [Tooltip("浮动频率（次/秒）")]
    public float floatFrequency = 1f;

    [Tooltip("浮动幅度（世界空间单位）")]
    public float floatAmplitude = 0.05f;

    [Header("Scale Effect")]
    [Tooltip("开启以中心为基准的缩放脉冲效果")]
    public bool useScaleEffect = true;

    [Tooltip("缩放脉冲频率（次/秒）")]
    public float scaleFrequency = 1f;

    [Tooltip("缩放幅度比例（0=不缩放，0.2=±20%%）")]
    [Range(0f, 0.5f)]
    public float scaleAmplitude = 0.1f;

    [Header("Label (3D TextMeshPro)")]
    [Tooltip("Hover 时显示的标签文字（调用 SetLabel 设置）")]
    public bool showLabel = true;

    [Tooltip("标签字体资源（留空使用默认字体）")]
    public TMP_FontAsset labelFont;

    [Tooltip("标签字体大小")]
    public float labelFontSize = 0.3f;

    [Tooltip("标签文字颜色")]
    public Color labelColor = Color.white;

    [Tooltip("TMP 材质球（留空则使用默认材质）")]
    public Material labelMaterial;

    [Tooltip("编辑器预览用标签文字（实际使用 SetLabel 动态设置）")]
    [HideInInspector, SerializeField] public string _previewText;
    [Header("Events")]
    [Tooltip("点击时触发")]
    public UnityEvent OnClick;

    private GameObject _cornerTL, _cornerTR, _cornerBL, _cornerBR;
    private GameObject _labelGO;
    private TextMeshPro _labelTMP;
    private bool _visible;

    // 世界空间下的四个角基准位置（不含浮动偏移）
    private Vector2 _pTL, _pTR, _pBL, _pBR;

    void Start()
    {
        CreateCorners();
        HideCorners();
        RefreshPositions();
    }

    void Update()
    {
        if (!_visible) return;

        float floatPhase = Time.time * floatFrequency  * Mathf.PI * 2;
        float scalePhase  = Time.time * scaleFrequency  * Mathf.PI * 2;

        if (useFloatEffect)
        {
            float t = (Mathf.Sin(floatPhase) + 1f) * 0.5f;
            float offset = Mathf.Lerp(-floatAmplitude, floatAmplitude, t);
            ApplyFloat(offset);
        }
        else
        {
            ApplyFloat(0f);
        }

        if (useScaleEffect)
        {
            float s = 1f + Mathf.Sin(scalePhase) * scaleAmplitude;
            ApplyScale(s);
        }
        else
        {
            ApplyScale(1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _visible = true;
        RefreshPositions();
        ShowCorners();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _visible = false;
        HideCorners();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

    // ---------- 生命周期结束后清理 ----------

    void OnDestroy()
    {
        DestroyCorners();
    }

    private void DestroyCorners()
    {
        if (_cornerTL != null) Destroy(_cornerTL);
        if (_cornerTR != null) Destroy(_cornerTR);
        if (_cornerBL != null) Destroy(_cornerBL);
        if (_cornerBR != null) Destroy(_cornerBR);
        if (_labelGO  != null) Destroy(_labelGO);
    }

    // ---------- 角 GameObject 管理 ----------

    private void CreateCorners()
    {
        _cornerTL = NewCorner("Corner_TL", transform);
        _cornerTR = NewCorner("Corner_TR", transform);
        _cornerBL = NewCorner("Corner_BL", transform);
        _cornerBR = NewCorner("Corner_BR", transform);
        CreateLabel();
    }

    /// <summary>
    /// 创建 3D TextMeshPro 标签 GameObject。
    /// </summary>
    private void CreateLabel()
    {
        _labelGO = new GameObject("Corner_Label");
        _labelGO.transform.SetParent(transform);
        _labelGO.transform.localScale = Vector3.one;
        _labelGO.SetActive(false);

        _labelTMP = _labelGO.AddComponent<TextMeshPro>();
        _labelTMP.text = _previewText ?? "";
        try
        {
            if (labelFont != null)
                _labelTMP.font = labelFont;
        }
        catch { }
        _labelTMP.fontSize = labelFontSize;
        _labelTMP.color = labelColor;
        _labelTMP.alignment = TextAlignmentOptions.Center;
        _labelTMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
        _labelTMP.sortingOrder = 1;
        try
        {
            if (labelMaterial != null)
                _labelTMP.fontMaterial = labelMaterial;
        }
        catch { }

        ApplyLabelPosition();
    }

    /// <summary>
    /// 标签锚定在括号上沿横向居中位置。
    /// </summary>
    private void ApplyLabelPosition()
    {
        if (_labelGO == null) return;
        float midX = (boundMin.x + boundMax.x) * 0.5f;
        _labelGO.transform.localPosition = new Vector2(midX, boundMax.y + 0.1f);
    }

    // ---------- 公开 API ----------

    /// <summary>
    /// 设置标签文字并立即显示。
    /// 传入 null 或空字符串可隐藏标签。
    /// forceShow = true 时忽略 _visible 状态（用于编辑器预览）。
    /// </summary>
    public void SetLabel(string text, bool forceShow = false)
    {
        if (_labelTMP == null) return;

        _previewText = text;
        if (string.IsNullOrEmpty(text))
        {
            _labelGO.SetActive(false);
            return;
        }

        _labelTMP.text = text;
        _labelGO.SetActive((_visible || forceShow) && showLabel);
    }

    /// <summary>
    /// 更新标签的 FontSize 和 Color（编辑器和运行时均可调用）。
    /// </summary>
    public void RefreshLabelStyle()
    {
        if (_labelTMP == null) return;
        try { if (labelFont != null) _labelTMP.font = labelFont; } catch { }
        _labelTMP.fontSize = labelFontSize;
        _labelTMP.color = labelColor;
        try { if (labelMaterial != null) _labelTMP.fontMaterial = labelMaterial; } catch { }
    }

    private GameObject NewCorner(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        Vector3 parentWorldScale = parent.lossyScale;
        go.transform.localScale = new Vector3(
            cornerSize / parentWorldScale.x,
            cornerSize / parentWorldScale.y,
            cornerSize / parentWorldScale.z
        );
        go.SetActive(false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = cornerSprite;
        return go;
    }

    /// <summary>
    /// 刷新四个角的世界空间基准位置与外扩方向。
    /// boundMin/Max 是相对于 transform.localPosition 的偏移。
    /// </summary>
    public void RefreshPositions()
    {
        Vector2 c = (boundMin + boundMax) * 0.5f;

        _pTL = boundMin + new Vector2(0,             boundMax.y - boundMin.y);
        _pTR = boundMin + new Vector2(boundMax.x - boundMin.x, boundMax.y - boundMin.y);
        _pBL = boundMin;
        _pBR = boundMin + new Vector2(boundMax.x - boundMin.x, 0);

        ApplyFloat(0f); // 基准位置，无位移
        ApplyScale(1f);  // 基准尺寸，无脉冲
        ApplyLabelPosition();
    }

    private void ShowCorners()
    {
        if (cornerSprite == null) return;
        _cornerTL.SetActive(true);
        _cornerTR.SetActive(true);
        _cornerBL.SetActive(true);
        _cornerBR.SetActive(true);
        if (showLabel && _labelTMP != null && !string.IsNullOrEmpty(_labelTMP.text))
            _labelGO.SetActive(true);
    }

    private void HideCorners()
    {
        _cornerTL?.SetActive(false);
        _cornerTR?.SetActive(false);
        _cornerBL?.SetActive(false);
        _cornerBR?.SetActive(false);
        if (_labelGO != null)
            _labelGO.SetActive(false);
    }

    // ---------- 效果实现 ----------

    /// <summary>
    /// Float：四角沿各自到中心的连线方向做内外偏移（仅改 localPosition）。
    /// offset > 0 向外扩，offset < 0 向内收。
    /// </summary>
    private void ApplyFloat(float offset)
    {
        Vector2 c = (_pTL + _pBR) * 0.5f; // 中心
        SetCorner(_cornerTL, _pTL + (_pTL - c).normalized * offset, Quaternion.Euler(0, 0, 0));
        SetCorner(_cornerTR, _pTR + (_pTR - c).normalized * offset, Quaternion.Euler(0, 0, -90));
        SetCorner(_cornerBL, _pBL + (_pBL - c).normalized * offset, Quaternion.Euler(0, 0, 90));
        SetCorner(_cornerBR, _pBR + (_pBR - c).normalized * offset, Quaternion.Euler(0, 0, 180));
    }

    /// <summary>
    /// Scale：四角 sprite 整体做均匀大小脉冲（仅改 localScale）。
    /// </summary>
    private void ApplyScale(float scale)
    {
        var parent = _cornerBL.transform.parent;
        var trueSize = cornerSize / parent.transform.lossyScale.x;
        float sz = trueSize * scale;
        Vector3 sc = Vector3.one * sz;
        if (_cornerTL != null) _cornerTL.transform.localScale = sc;
        if (_cornerTR != null) _cornerTR.transform.localScale = sc;
        if (_cornerBL != null) _cornerBL.transform.localScale = sc;
        if (_cornerBR != null) _cornerBR.transform.localScale = sc;
    }

    private void SetCorner(GameObject go, Vector2 pos, Quaternion rot)
    {
        if (go == null) return;
        go.transform.localPosition = pos;
        go.transform.localRotation = rot;
    }

    // ---------- Gizmos（Editor 实时预览）----------

    void OnDrawGizmos()
    {
        // boundMin/boundMax 是 localOffset，统一用 localToWorldMatrix 转世界坐标
        Vector3 bl = transform.localToWorldMatrix.MultiplyPoint3x4((Vector3)boundMin);
        Vector3 br = transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(boundMax.x, boundMin.y, 0));
        Vector3 tl = transform.localToWorldMatrix.MultiplyPoint3x4(new Vector3(boundMin.x, boundMax.y, 0));
        Vector3 tr = transform.localToWorldMatrix.MultiplyPoint3x4((Vector3)boundMax);

        // 线框：青色
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.5f);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);

        // 角球：大小与 bracket 一致（橙色），直观看到括号落点
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(tl, cornerSize * 0.2f);
        Gizmos.DrawWireSphere(tr, cornerSize * 0.2f);
        Gizmos.DrawWireSphere(bl, cornerSize * 0.2f);
        Gizmos.DrawWireSphere(br, cornerSize * 0.2f);
    }
}

#if UNITY_EDITOR
namespace UnityEditor
{
    [CustomEditor(typeof(BuildingIndicator))]
    public class BuildingIndicatorEditor : Editor
    {
        private SerializedProperty spCornerSprite, spCornerSize;
        private SerializedProperty spBoundMin, spBoundMax;
        private SerializedProperty spUseFloatEffect, spFloatFrequency, spFloatAmplitude;
        private SerializedProperty spUseScaleEffect, spScaleFrequency, spScaleAmplitude;
        private SerializedProperty spShowLabel, spLabelFont, spLabelFontSize, spLabelColor, spLabelMaterial;
        private SerializedProperty spOnClick;

        private bool _foldoutBounds = true;
        private bool _foldoutEffect = true;
        private bool _foldoutLabel = true;
        private BuildingIndicator _tgt;

        void OnEnable()
        {
            _tgt = (BuildingIndicator)target;

            spCornerSprite   = serializedObject.FindProperty("cornerSprite");
            spCornerSize     = serializedObject.FindProperty("cornerSize");
            spBoundMin       = serializedObject.FindProperty("boundMin");
            spBoundMax       = serializedObject.FindProperty("boundMax");
            spUseFloatEffect = serializedObject.FindProperty("useFloatEffect");
            spFloatFrequency = serializedObject.FindProperty("floatFrequency");
            spFloatAmplitude = serializedObject.FindProperty("floatAmplitude");
            spUseScaleEffect = serializedObject.FindProperty("useScaleEffect");
            spScaleFrequency = serializedObject.FindProperty("scaleFrequency");
            spScaleAmplitude = serializedObject.FindProperty("scaleAmplitude");
            spShowLabel     = serializedObject.FindProperty("showLabel");
            spLabelFont     = serializedObject.FindProperty("labelFont");
            spLabelFontSize = serializedObject.FindProperty("labelFontSize");
            spLabelColor    = serializedObject.FindProperty("labelColor");
            spLabelMaterial = serializedObject.FindProperty("labelMaterial");
            spOnClick = serializedObject.FindProperty("OnClick");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ── Brackets ──────────────────────────────────────────────
            EditorGUILayout.LabelField("Brackets", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spCornerSprite);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spCornerSize);
            if (GUI.changed)
                _tgt.RefreshPositions();
            EditorGUI.EndChangeCheck();

            // ── Bound Constraints ─────────────────────────────────────
            EditorGUILayout.Space(4);
            _foldoutBounds = EditorGUILayout.Foldout(_foldoutBounds, "Bound Constraints", true);
            if (_foldoutBounds)
            {
                EditorGUI.indentLevel++;

                // Min (BL)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Min (BL)");
                spBoundMin.vector2Value = EditorGUILayout.Vector2Field(GUIContent.none, spBoundMin.vector2Value);
                EditorGUILayout.EndHorizontal();

                // Max (TR)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Max (TR)");
                spBoundMax.vector2Value = EditorGUILayout.Vector2Field(GUIContent.none, spBoundMax.vector2Value);
                EditorGUILayout.EndHorizontal();

                // 辅助按钮，垂直一行紧接其后
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Auto from SpriteRenderer", GUILayout.Height(20)))
                    AutoFromSpriteRenderer();
                if (GUILayout.Button("Fit to CornerSize", GUILayout.Height(20)))
                    FitCornersToBounds();
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            // ── Effect ────────────────────────────────────────────────
            EditorGUILayout.Space(4);
            _foldoutEffect = EditorGUILayout.Foldout(_foldoutEffect, "Float / Scale Effect", true);
            if (_foldoutEffect)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spUseFloatEffect);
                if (spUseFloatEffect.boolValue)
                {
                    EditorGUILayout.PropertyField(spFloatFrequency);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(spFloatAmplitude);
                    if (GUI.changed) _tgt.RefreshPositions();
                    EditorGUI.EndChangeCheck();
                }
                EditorGUILayout.PropertyField(spUseScaleEffect);
                if (spUseScaleEffect.boolValue)
                {
                    EditorGUILayout.PropertyField(spScaleFrequency);
                    EditorGUILayout.PropertyField(spScaleAmplitude);
                }
                EditorGUI.indentLevel--;
            }

            // ── Label ────────────────────────────────────────────────
            EditorGUILayout.Space(4);
            _foldoutLabel = EditorGUILayout.Foldout(_foldoutLabel, "Label", true);
            if (_foldoutLabel)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(spShowLabel);
                if (spShowLabel.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(spLabelFont);
                    EditorGUILayout.PropertyField(spLabelFontSize);
                    EditorGUILayout.PropertyField(spLabelColor);
                    EditorGUILayout.PropertyField(spLabelMaterial);
                    if (GUI.changed)
                    {
                        _tgt.RefreshLabelStyle();
                        _tgt.RefreshPositions();
                    }
                    EditorGUI.EndChangeCheck();

                    EditorGUI.BeginChangeCheck();
                    string newText = EditorGUILayout.TextField("Preview Text", _tgt._previewText ?? "");
                    if (GUI.changed)
                    {
                        Undo.RecordObject(_tgt, "Preview Label Text");
                        _tgt._previewText = newText;
                        _tgt.SetLabel(newText, true);
                    }
                    EditorGUI.EndChangeCheck();
                }
                EditorGUI.indentLevel--;
            }

            // ── Events ────────────────────────────────────────────────
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(spOnClick);

            EditorGUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();
        }

        private void AutoFromSpriteRenderer()
        {
            var sr = _tgt.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null)
            {
                Debug.LogWarning("[BuildingIndicator] 物体上未找到 SpriteRenderer 或 Sprite。");
                return;
            }
            Undo.RecordObject(_tgt, "Auto Bounds from SpriteRenderer");
            var b = sr.sprite.bounds;
            _tgt.boundMin = new Vector2(b.min.x, b.min.y);
            _tgt.boundMax = new Vector2(b.max.x, b.max.y);
            _tgt.RefreshPositions();
            serializedObject.ApplyModifiedProperties();
        }

        private void FitCornersToBounds()
        {
            Undo.RecordObject(_tgt, "Fit Corners to Bounds");
            Vector2 c = (_tgt.boundMin + _tgt.boundMax) * 0.5f;
            float hs = _tgt.cornerSize * 0.5f;
            _tgt.boundMin = c + new Vector2(-hs, -hs);
            _tgt.boundMax = c + new Vector2( hs,  hs);
            _tgt.RefreshPositions();
            serializedObject.ApplyModifiedProperties();
        }

        // ── Scene Handles（四角独立拖拽）────────────────────────────────
        void OnSceneGUI()
        {
            if (_tgt.cornerSprite == null) return;

            Event e = Event.current;
            bool allowEdit = !Application.isPlaying
                             && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag);

            Matrix4x4 mtx = _tgt.transform.localToWorldMatrix;

            // 世界坐标下的四角（统一用矩阵变换，旋转/缩放时一致）
            Vector3 blW = mtx.MultiplyPoint3x4(new Vector3(_tgt.boundMin.x, _tgt.boundMin.y, 0));
            Vector3 brW = mtx.MultiplyPoint3x4(new Vector3(_tgt.boundMax.x, _tgt.boundMin.y, 0));
            Vector3 tlW = mtx.MultiplyPoint3x4(new Vector3(_tgt.boundMin.x, _tgt.boundMax.y, 0));
            Vector3 trW = mtx.MultiplyPoint3x4(new Vector3(_tgt.boundMax.x, _tgt.boundMax.y, 0));

            Handles.color = new Color(0.3f, 0.9f, 1f, allowEdit ? 0.9f : 0.5f);
            Handles.DrawLine(blW, brW);
            Handles.DrawLine(brW, trW);
            Handles.DrawLine(trW, tlW);
            Handles.DrawLine(tlW, blW);

            // 角球（选中高亮 + 半径 = bracket 尺寸）
            float r = _tgt.cornerSize * 0.2f;
            DrawCap(blW, r, allowEdit);
            DrawCap(brW, r, allowEdit);
            DrawCap(tlW, r, allowEdit);
            DrawCap(trW, r, allowEdit);

            if (!allowEdit) return;

            EditorGUI.BeginChangeCheck();

            // 每角两个正交滑块（X轴 + Y轴），互不干扰
            var blX = Handles.Slider(blW + Vector3.right  * r, Vector3.right,  r * 0.4f, Handles.ConeHandleCap, 0f);
            var blY = Handles.Slider(blX  + Vector3.up     * r, Vector3.up,     r * 0.4f, Handles.ConeHandleCap, 0f);

            var brX = Handles.Slider(brW + Vector3.right  * r, Vector3.right,  r * 0.4f, Handles.ConeHandleCap, 0f);
            var brY = Handles.Slider(brX  + Vector3.up     * r, Vector3.up,     r * 0.4f, Handles.ConeHandleCap, 0f);

            var tlX = Handles.Slider(tlW + Vector3.right  * r, Vector3.right,  r * 0.4f, Handles.ConeHandleCap, 0f);
            var tlY = Handles.Slider(tlX  + Vector3.up     * r, Vector3.up,     r * 0.4f, Handles.ConeHandleCap, 0f);

            var trX = Handles.Slider(trW + Vector3.right  * r, Vector3.right,  r * 0.4f, Handles.ConeHandleCap, 0f);
            var trY = Handles.Slider(trX  + Vector3.up     * r, Vector3.up,     r * 0.4f, Handles.ConeHandleCap, 0f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_tgt, "Drag Corner Handles");

                // 收集四个角各自的 world X 和 Y 极值
                float minX = Mathf.Min(blX.x, brX.x, tlX.x, trX.x);
                float minY = Mathf.Min(blY.y, brY.y, tlY.y, trY.y);
                float maxX = Mathf.Max(blX.x, brX.x, tlX.x, trX.x);
                float maxY = Mathf.Max(blY.y, brY.y, tlY.y, trY.y);

                // 反算回 local space
                Vector3 bMin = mtx.inverse.MultiplyPoint3x4(new Vector3(minX, minY, 0));
                Vector3 bMax = mtx.inverse.MultiplyPoint3x4(new Vector3(maxX, maxY, 0));

                _tgt.boundMin = new Vector2(bMin.x, bMin.y);
                _tgt.boundMax = new Vector2(bMax.x, bMax.y);
                _tgt.RefreshPositions();
                EditorUtility.SetDirty(_tgt);
            }
        }

        private void DrawCap(Vector3 pos, float radius, bool selected)
        {
            Handles.color = selected
                ? new Color(1f, 0.4f, 0.2f, 1f)
                : new Color(1f, 0.6f, 0.2f, 0.7f);
            Handles.DrawWireDisc(pos, Camera.current != null ? Camera.current.transform.forward : Vector3.back, radius);
            if (selected)
                Handles.DrawSolidDisc(pos, Camera.current != null ? Camera.current.transform.forward : Vector3.back, radius * 0.5f);
        }
    }
}
#endif
