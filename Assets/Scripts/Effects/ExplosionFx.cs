using System;
using UnityEngine;

public class ExplosionFx : MonoBehaviour
{
    [SerializeField] private ParticleSystem _ps;
    [SerializeField] private float _duration;

    private Action<ExplosionFx> _onRelease;
    private float _timer;

    public void Play(Action<ExplosionFx> onRelease)
    {
        _onRelease = onRelease;
        _timer = 0f;

        if(_ps != null)
        {
            _ps.Clear();
            _ps.Play();
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= _duration)
        {
            _onRelease?.Invoke(this);
            gameObject.SetActive(false);

            _onRelease = null;
        }
    }
}
