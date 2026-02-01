using UnityEngine;

/// <summary>
///     反撃用のガンナーを管理するクラス
/// </summary>
public class PlayerCounterGunner : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private TimeDilationController _timeDlicon;
    [SerializeField] private CounterTargetMemory _targetMemory;
    [SerializeField] private PlayerCounterBullet _counterBulletPrefab;
    [SerializeField] private Transform _muzzle;


    [Header("反撃設定")]
    [SerializeField, Tooltip("反撃に使うコスト")] private int _counterCost;
    [SerializeField, Tooltip("反撃のクールダウン(連続攻撃防止)")] private float _counterColdown;
    [SerializeField, Tooltip("何発撃つか")] private int _burstCount;
    [SerializeField, Tooltip("リング状にずらす半径")] private float _ringRadius;
    [SerializeField, Tooltip("撃つ向きを散らす角度")] private float _angleJitter;

    private CounterToken _counterToken;
    private PlayerInputHandler _inputHandler;
    private float _counterCooldownTimer;

    /// <summary>
    ///     反撃入力があったときの処理をする
    /// </summary>
    private void OnFirePerformed()
    {
        if (_timeDlicon == null) return;
        if (!_timeDlicon.IsPlaying) return;

        if (_counterCooldownTimer > 0f) return;
        TryCounterAttack();
    }

    /// <summary>
    ///     反撃攻撃を試みる
    /// </summary>
    private void TryCounterAttack()
    {
        if (_counterToken.CurrentToken < _counterCost) return;
        Transform target = _targetMemory.CurrentTarget;

        if (target == null) return;

        Debug.Log("カウンター攻撃発動!");
        _counterToken.UseToken(_counterCost);

        for (int i = 0; i < _burstCount; i++)
        {
            float rad = (Mathf.PI * 2f) * (i / (float)_burstCount);

            Vector3 offset =
                _muzzle.right * (Mathf.Cos(rad) * _ringRadius) +
                _muzzle.up * (Mathf.Sin(rad) * _ringRadius);

            Vector3 spawnPos = _muzzle.position + offset;

            Vector3 dir = (target.position - spawnPos);
            if (dir.sqrMagnitude < 0.0001f) dir = _muzzle.forward;
            dir.Normalize();

            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            rot *= Quaternion.Euler(
                Random.Range(-_angleJitter, _angleJitter),
                Random.Range(-_angleJitter, _angleJitter),
                0f
            );

            PlayerCounterBullet bullet = Instantiate(_counterBulletPrefab, spawnPos, rot);


            bullet.Spawn(null, transform);
            bullet.SetTarget(target, _muzzle.forward);
        }

        // クールダウンタイマーのリセット
        _counterCooldownTimer = _counterColdown;
        _targetMemory.Clear();
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _counterToken = GetComponent<CounterToken>();
    }

    private void Update()
    {
        if (_counterCooldownTimer > 0f)
        {
            _counterCooldownTimer -= Time.unscaledDeltaTime;
        }
    }

    private void OnEnable()
    {
        _inputHandler.FirePerformed += OnFirePerformed;
    }

    private void OnDisable()
    {
        _inputHandler.FirePerformed -= OnFirePerformed;
    }
}
