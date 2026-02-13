using Unity.Cinemachine;
using UnityEngine;

public class BulletCameraController : MonoBehaviour
{
    [SerializeField] private Camera _bulletCamera;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private GameObject _root;

    private bool _isShowing;
    private Transform _currentTarget;

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
        _isShowing = false;
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
        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy)
            Hide();
    }
}
