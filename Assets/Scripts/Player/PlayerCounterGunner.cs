using UnityEngine;

/// <summary>
///     反撃用のガンナーを管理するクラス
/// </summary>
public class PlayerCounterGunner : MonoBehaviour
{
    [SerializeField] private TimeDilationController _timeDlicon;
    [SerializeField] private Transform _muzzle;
    [SerializeField,Tooltip("反撃に使うコスト")] private int _counterCost;
    [SerializeField,Tooltip("反撃のクールダウン(連続攻撃防止)")] private float _counterColdown;

    private CounterToken _counterToken;
    private PlayerInputHandler _inputHandler;
    private float _counterCooldownTimer;

    /// <summary>
    ///     反撃入力があったときの処理をする
    /// </summary>
    private void OnFirePerformed()
    {
        if (_timeDlicon == null) return;
        if(!_timeDlicon.IsPlaying) return;

        if (_counterCooldownTimer > 0f) return;
        TryCounterAttack();
    }

    /// <summary>
    ///     反撃攻撃を試みる
    /// </summary>
    private void TryCounterAttack()
    {
        if (_counterToken.CurrentToken < _counterCost) return;

        // カウンター攻撃処理
        Debug.Log("カウンター攻撃発動!");
        _counterToken.UseToken(_counterCost);

        // クールダウンタイマーのリセット
        _counterCooldownTimer = _counterColdown;
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
            _counterCooldownTimer -= Time.deltaTime;
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
