using UnityEngine;

public class PlayerCounterGunner : MonoBehaviour
{
    [SerializeField] private TimeDilationController _timeDlicon;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private int _counterCost;
    [SerializeField] private float _counterColdown;

    private CounterToken _counterToken;
    private PlayerInputHandler _inputHandler;
    private float _counterCooldownTimer;

    private void OnFirePerformed()
    {
        if (_timeDlicon == null) return;
        if(!_timeDlicon.IsPlaying) return;

        if (_counterCooldownTimer > 0f) return;
        TryCounterAttack();
    }

    private void TryCounterAttack()
    {
        if (_counterToken.CurrentToken < _counterCost) return;
        Debug.Log("カウンター攻撃発動!");
        // カウンター攻撃処理
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
