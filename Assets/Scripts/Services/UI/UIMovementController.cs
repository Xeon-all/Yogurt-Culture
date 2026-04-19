using UnityEngine;

public class UIMovementController : MonoBehaviour {
    [Header("Movement Settings")]
    [Tooltip("从起始位置到目标位置的插值曲线，0=起始，1=目标")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("动画持续时间（秒）")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Vector2 moveBy;
    private Vector2 _hidePos;
    [SerializeField] private Vector2 _showPos = Vector2.zero;
    private float _progress;
    private bool _isAnimating;
    private Coroutine _coroutine;
    void Awake()
    {
        // _showPos = transform.position;
        // Debug.Log("stored pos : " + _showPos);
        _hidePos = _showPos + moveBy;
    }

    public void MoveShow() {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
        }
        _progress = 0f;
        _isAnimating = true;
        _coroutine = StartCoroutine(Animate(_hidePos, _showPos));
    }

    public void MoveHide()
    {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
        }
        _progress = 0f;
        _isAnimating = true;
        _coroutine = StartCoroutine(Animate(_showPos, _hidePos));
    }

    public void MoveToImmediate(Vector3 target) {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _isAnimating = false;
        transform.position = target;
    }

    private System.Collections.IEnumerator Animate(Vector2 start, Vector2 end) {
        // Debug.Log("Actual dst : " + end);
        while (_isAnimating && _progress < 1f) {
            _progress += Time.deltaTime / duration;
            float t = moveCurve.Evaluate(Mathf.Clamp01(_progress));
            transform.position = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        if (_isAnimating) {
            transform.position = end;
        }
        _isAnimating = false;
        _coroutine = null;
    }
}
