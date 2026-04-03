using UnityEngine;

public class UIMovementController : MonoBehaviour {
    [Header("Movement Settings")]
    [Tooltip("从起始位置到目标位置的插值曲线，0=起始，1=目标")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("动画持续时间（秒）")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Vector2 moveBy;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _progress;
    private bool _isAnimating;
    private Coroutine _coroutine;
    public void Move()
    {
        MoveBy(moveBy);
    }

    public void MoveTo(Vector3 target) {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
        }
        _startPos = target;
        _targetPos = transform.position;
        _progress = 0f;
        _isAnimating = true;
        _coroutine = StartCoroutine(Animate());
    }

    public void MoveBy(Vector3 offset) {
        MoveTo(transform.position + offset);
    }

    public void MoveToImmediate(Vector3 target) {
        if (_coroutine != null) {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _isAnimating = false;
        transform.position = target;
    }

    private System.Collections.IEnumerator Animate() {
        while (_isAnimating && _progress < 1f) {
            _progress += Time.deltaTime / duration;
            float t = moveCurve.Evaluate(Mathf.Clamp01(_progress));
            transform.position = Vector3.LerpUnclamped(_startPos, _targetPos, t);
            yield return null;
        }

        if (_isAnimating) {
            transform.position = _targetPos;
        }
        _isAnimating = false;
        _coroutine = null;
    }
}
