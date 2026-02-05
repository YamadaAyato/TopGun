using UnityEngine;

/// <summary>
///     フレアを発射するクラス
/// </summary>
public class FlareEmitter : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Transform _flareMuzzle;
    [SerializeField] private GameObject _flarePrefab;

    [Header("発射設定")]
    [SerializeField, Tooltip("1回の発射で生成するフレア数")] private int _count;
    [SerializeField, Tooltip("左右方向の散布角")] private float _yawSpreadAngle;
    [SerializeField, Tooltip("上下方向の散布角")] private float _pitchSpreadAngle;
    [SerializeField, Tooltip("生成直後に与える初速")] private float _initSpeed;
    [SerializeField, Tooltip("発生位置の微ブレ")] private float _spawnJitter;

    /// <summary>
    ///     フレアを発射する
    /// </summary>
    public void EmitFlare()
    {
        if (_flarePrefab == null || _flareMuzzle == null) return;

        // 発射位置と基準回転を取得
        Vector3 origin = _flareMuzzle.position;
        Quaternion baseRot = _flareMuzzle.rotation;

        // フレアを複数生成
        for (int i = 0; i < _count; i++)
        {
            // 散布の割合を計算
            float u = i / (float)(_count - 1);

            float yaw = Mathf.Lerp(-_yawSpreadAngle / 2f, _yawSpreadAngle / 2f, u);
            float pitch = Random.Range(-_pitchSpreadAngle / 2f, _pitchSpreadAngle / 2f);

            // 発射方向を計算
            Quaternion rot = baseRot * Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = rot * Vector3.forward;

            // 位置の微ブレを計算
            Vector3 jitter =
                (_flareMuzzle.right * Random.Range(-_spawnJitter, _spawnJitter)) +
                (_flareMuzzle.up * Random.Range(-_spawnJitter, _spawnJitter)); ;

            GameObject flareObj = Instantiate(_flarePrefab, origin + jitter, rot);

            // 初速を与える
            if (flareObj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = dir * _initSpeed;
            }
        }
    }
}
