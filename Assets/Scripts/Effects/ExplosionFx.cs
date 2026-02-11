using System;
using UnityEngine;

/// <summary>
///     爆発エフェクトの種類
/// </summary>
public enum ExplosionType
{
    Small,
    Big
}

/// <summary>
///     爆発エフェクトの管理を行うクラス
/// </summary>
public class ExplosionFx : MonoBehaviour
{
    [SerializeField] private float _duration;

    private Action<ExplosionFx> _onRelease;
    private float _timer;
    private ParticleSystem _ps;

    /// <summary>
    ///     エフェクトを再生する
    /// </summary>
    /// <param name="onRelease"></param>
    public void Play(Action<ExplosionFx> onRelease)
    {
        _onRelease = onRelease;
        _timer = 0f;

        if (_ps != null)
        {
            _ps.Clear();
            _ps.Play();
        }

        GameEvents.RaiseExplosion(this.transform.position);
        gameObject.SetActive(true);
    }

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // 再生時間を超えたらプールに返す
        if (_timer >= _duration)
        {
            _onRelease?.Invoke(this);
            gameObject.SetActive(false);

            _onRelease = null;
        }
    }
}
