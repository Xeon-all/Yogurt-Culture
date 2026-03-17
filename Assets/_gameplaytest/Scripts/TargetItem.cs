using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TargetItem : MonoBehaviour, IPointerEnterHandler
{
    [Header("Timing")]
    [SerializeField] private float initialDuration = 3f;
    [SerializeField] private float fadeDelay = 2f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Colors")]
    [SerializeField] private Color initialColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Scoring")]
    [SerializeField] private int initialPenalty = -2;
    [SerializeField] private int activeScore = 1;

    private SpriteRenderer _spriteRenderer;
    private Image _image;
    private bool _isInitialPhase = true;
    private bool _isDestroyed = false;
    private float _timer = 0f;
    private Color _currentColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _image = GetComponent<Image>();

        _currentColor = initialColor;
        SetColor(_currentColor);
    }

    private void Start()
    {
        _timer = 0f;
        _isInitialPhase = true;
    }

    private void Update()
    {
        if (_isDestroyed) return;

        _timer += Time.deltaTime;

        // 阶段转换
        if (_isInitialPhase && _timer >= initialDuration)
        {
            _isInitialPhase = false;
            _timer = 0f;
            _currentColor = activeColor;
            SetColor(_currentColor);
        }

        // Fade 逻辑：从 initialDuration + fadeDelay 后开始 fade
        if (!_isInitialPhase && _timer >= fadeDelay)
        {
            float fadeProgress = (_timer - fadeDelay) / fadeDuration;
            float alpha = 1f - fadeProgress;
            alpha = Mathf.Clamp01(alpha);
            Color fadeColor = _currentColor;
            fadeColor.a = alpha;
            SetColor(fadeColor);
        }

        // 彻底消失后销毁
        if (!_isInitialPhase && _timer >= fadeDelay + fadeDuration)
        {
            Destroy(gameObject);
        }
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
        else if (_image != null)
        {
            _image.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDestroyed) return;

        int scoreChange = _isInitialPhase ? initialPenalty : activeScore;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreChange);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _isDestroyed = true;
    }

    // // 编辑器中显示状态
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = _isInitialPhase ? initialColor : activeColor;
    //     Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
    // }
}
