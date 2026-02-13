using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class BulletCameraController : MonoBehaviour
{
    [SerializeField] private Camera _bulletCamera;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private GameObject _root;

    [SerializeField] private float _holdSeconds;

    private bool _isShowing;
    private bool _isHolding;
    private Transform _currentTarget;
    private Tween _hideTween;

    public bool TryShow(Transform missile)
    {
        if (_isShowing) return false;

        _currentTarget = missile;
        _cinemachineCamera.Follow = _currentTarget;
        _cinemachineCamera.LookAt = _currentTarget;

        _root.SetActive(true);
        _bulletCamera.enabled = true;

        _isShowing = true;
        return true;
    }

    public void Hide()
    {
        _hideTween?.Kill();
        _hideTween = null;

        _isShowing = false;
        _isHolding = false;
        _currentTarget = null;

        _root.SetActive(false);
        _bulletCamera.enabled = false;
    }

    private void HideAfterHold()
    {
        if (_isHolding) return;
        _isHolding = true;

        _hideTween?.Kill();
        _hideTween = DOVirtual.DelayedCall(_holdSeconds, () =>
        {
            _bulletCamera.enabled = false;

            _cinemachineCamera.Follow = null;
            _cinemachineCamera.LookAt = null;

            _currentTarget = null;
            _isHolding = false;
            _isShowing = false;
            _root.SetActive(false);
        });
    }

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        // 表示してないなら監視しない（ここが超重要）
        if (!_isShowing) return;

        // ターゲットが消えたら「ホールドして閉じる」
        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
        {
            HideAfterHold();
        }
    }
}
