using UnityEngine;

/// <summary>
///     反撃用のガンナーを管理するクラス
/// </summary>
public class PlayerCounterGunner : MonoBehaviour
{
    [SerializeField] private TimeDilationController _timeDlicon;
    [SerializeField] private CounterTargetMemory _targetMemory;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private PlayerCounterBullet _counterBulletPrefab;

    [SerializeField, Tooltip("反撃に使うコスト")] private int _counterCost;
    [SerializeField, Tooltip("反撃のクールダウン(連続攻撃防止)")] private float _counterColdown;

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

        // カウンター攻撃処理
        Debug.Log("カウンター攻撃発動!");
        _counterToken.UseToken(_counterCost);

        Vector3 dir = (target.position - _muzzle.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        PlayerCounterBullet bullet = Instantiate(_counterBulletPrefab, _muzzle.position, rot);
        bullet.SetTarget(target);

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
