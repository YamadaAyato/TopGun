using UnityEngine;

public class FlareEmitter : MonoBehaviour
{
    [SerializeField] private Transform _flareMuzzle;
    [SerializeField] private GameObject _flarePrefab;

    [SerializeField] private int _count;
    [SerializeField] private float _yawSpreadAngle;
    [SerializeField] private float _pitchSpreadAngle;
    [SerializeField] private float _initSpeed;
    [SerializeField] private float _spawnJitter;

    public void EmitFlare()
    {
        if (_flarePrefab == null || _flareMuzzle == null) return;

        Vector3 origin = _flareMuzzle.position;
        Quaternion baseRot = _flareMuzzle.rotation;

        for (int i = 0; i < _count; i++)
        {
            float u = i / (float)(_count - 1);

            float yaw = Mathf.Lerp(-_yawSpreadAngle / 2f, _yawSpreadAngle / 2f, u);
            float pitch = Random.Range(-_pitchSpreadAngle / 2f, _pitchSpreadAngle / 2f);

            Quaternion rot = baseRot * Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = rot * Vector3.forward;

            Vector3 jitter =
                (_flareMuzzle.right * Random.Range(-_spawnJitter, _spawnJitter)) +
                (_flareMuzzle.up * Random.Range(-_spawnJitter, _spawnJitter)); ;

            GameObject flareObj = Instantiate(_flarePrefab, origin + jitter, rot);

            if (flareObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = dir * _initSpeed;
            }
        }
    }
}
