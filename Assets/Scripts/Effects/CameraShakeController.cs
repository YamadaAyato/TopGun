using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Camera _mainCamera;

    [SerializeField] private float _hitGain;
    [SerializeField] private float _explosionGain;
    [SerializeField] private float _maxDistance;

    [SerializeField] private AnimationCurve _fallOffCurve;
    private CinemachineImpulseSource _impulseSource;

    public void PlayHit(float intensity)
    {
        if (_impulseSource == null) return;

        float gain = _hitGain * Mathf.Clamp01(intensity);

        var cam = _mainCamera != null ? _mainCamera : Camera.main;
        if (cam == null) return;

        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        Vector3 dir = (cam.transform.right * x + cam.transform.up * y).normalized;

        _impulseSource.GenerateImpulse(dir * gain);
    }

    public void PlayExplosion(Vector3 explosionPosition)
    {
        float distance = Vector3.Distance(_mainCamera.transform.position, explosionPosition);
        float t = Mathf.Clamp01(distance / _maxDistance);
        float fallOff = _fallOffCurve.Evaluate(t);
        float gain = _explosionGain * fallOff;
        _impulseSource.GenerateImpulse(gain);
    }

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHit += PlayHit;
        GameEvents.OnExplosion += PlayExplosion;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHit -= PlayHit;
        GameEvents.OnExplosion -= PlayExplosion;
    }
}
