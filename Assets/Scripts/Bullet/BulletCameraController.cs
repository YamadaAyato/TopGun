using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
///     弾に追従するカメラを制御するクラス
/// </summary>
public class BulletCameraController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Camera _bulletCamera;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private GameObject _root;
    [Header("ホールド時間")]
    [SerializeField] private float _holdSeconds;

    private bool _isShowing;
    private bool _isHolding;
    private Transform _currentTarget;
    private Tween _hideTween;

    /// <summary>
    ///     カメラの表示を試みる
    /// </summary>
    /// <param name="missile"> 追従する対象のTransform </param>
    /// <returns> カメラの表示に成功したかどうか </returns>
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

    /// <summary>
    ///     カメラを消す
    /// </summary>
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

    private void Awake()
    {
        Hide();
    }

    private void LateUpdate()
    {
        // カメラが表示されていない場合は早期リターン
        if (!_isShowing) return;

        // ターゲットが消えたら「ホールドして閉じる」
        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
        {
            HideAfterHold();
        }
    }

    /// <summary>
    ///     カメラをホールドして消す
    /// </summary>
    private void HideAfterHold()
    {
        if (_isHolding) return;
        _isHolding = true;

        _hideTween?.Kill();
        _hideTween = DOVirtual.DelayedCall(_holdSeconds, () =>
        {
            // ホールド時間経過後にターゲットが消えてたら消す
            _bulletCamera.enabled = false;

            _cinemachineCamera.Follow = null;
            _cinemachineCamera.LookAt = null;

            _currentTarget = null;
            _isHolding = false;
            _isShowing = false;
            _root.SetActive(false);
        });
    }
}
