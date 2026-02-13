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
    [SerializeField] private BulletCameraController _bulletCameraController;
    [SerializeField] private Transform _muzzle;

    [Header("反撃設定")]
    [SerializeField, Tooltip("反撃に使うコスト")] private int _counterCost;
    [SerializeField, Tooltip("反撃のクールダウン(連続攻撃防止)")] private float _counterColdown;
    [SerializeField, Tooltip("何発撃つか")] private int _burstCount;
    [SerializeField, Tooltip("リング状にずらす半径")] private float _ringRadius;
    [SerializeField, Tooltip("弾のPoolカウント")] private int _poolInitCount;

    private CounterToken _counterToken;
    private PlayerInputHandler _inputHandler;
    private float _counterCooldownTimer;

    private ObjectPool<PlayerCounterBullet> _bulletPool;

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
        // ターゲットを取得
        Transform target = _targetMemory.CurrentTarget;

        if (target == null) return;
        Debug.Log("カウンター攻撃発動!");

        _counterToken.UseToken(_counterCost);

        // リング状に弾を複数生成する
        for (int i = 0; i < _burstCount; i++)
        {
            // 円周上の位置を計算
            float rad = (Mathf.PI * 2f) * (i / (float)_burstCount);

            // 円状の初期位置を計算して、発射位置を決定
            Vector3 offset =
                _muzzle.right * (Mathf.Cos(rad) * _ringRadius) +
                _muzzle.up * (Mathf.Sin(rad) * _ringRadius);
            Vector3 spawnPos = _muzzle.position + offset;

            // 発射方向を計算
            Quaternion rot = Quaternion.LookRotation(_muzzle.forward, Vector3.up);

            // 銃弾を生成して発射
            PlayerCounterBullet bullet = _bulletPool.Get();
            bullet.transform.SetPositionAndRotation(spawnPos, rot);

            // 銃弾にターゲットをセット
            bullet.Spawn(ReturnToPool, transform);
            bullet.SetTarget(target, _muzzle.forward);

            if(i == 0)
            {
                // 最初の弾だけカメラを追従させる
                _bulletCameraController.TryShow(bullet.transform);
            }
        }

        // クールダウンタイマーのリセット
        _counterCooldownTimer = _counterColdown;
        _targetMemory.Clear();
    }

    /// <summary>
    ///     プールに銃弾を返す
    /// </summary>
    /// <param name="bullet"></param>
    private void ReturnToPool(BulletBase bullet)
    {
        _bulletPool.Release((PlayerCounterBullet)bullet);
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _counterToken = GetComponent<CounterToken>();

        _bulletPool = new ObjectPool<PlayerCounterBullet>(_counterBulletPrefab, _muzzle, _poolInitCount);
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
