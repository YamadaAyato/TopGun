using UnityEngine;

/// <summary>
///     機体速度に応じて画面周辺へ風の粒子を流すクラス
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class FlightWindParticles : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerAirCraftController _aircraft;
    [SerializeField] private Camera _camera;

    [Header("演出設定")]
    [SerializeField, Tooltip("風が出始める速度")] private float _startSpeed = 20f;
    [SerializeField, Tooltip("演出が最大になる速度")] private float _fullSpeed = 160f;
    [SerializeField, Min(0f), Tooltip("最大速度で1秒間に出す粒子数")] private float _maxEmission = 180f;
    [SerializeField, Min(1f), Tooltip("粒子を生成する距離")] private float _spawnDistance = 12f;
    [SerializeField, Range(0.5f, 0.95f), Tooltip("中央を空ける範囲")] private float _innerRadius = 0.58f;
    [SerializeField, Tooltip("風の色")] private Color _color = new Color(0.85f, 0.94f, 1f, 0.65f);

    [SerializeField, Tooltip("風の筋の太さの最小値・最大値")]
    private Vector2 _widthRange = new Vector2(0.08f, 0.14f);

    [SerializeField, Min(1), Tooltip("同時に表示できる風の粒子数。ParticleSystemの最大数を上書きする")]
    private int _maxParticles = 160;
    [SerializeField, Min(1), Tooltip("処理落ち後に粒子が集中しないようにする1フレームの発生上限")]
    private int _maxEmissionPerFrame = 16;
    [SerializeField, Tooltip("演出が最小・最大のときの粒子速度")]
    private Vector2 _particleSpeedRange = new Vector2(22f, 75f);
    [SerializeField, Range(0f, 1f), Tooltip("画面周辺に粒子を生成する外側の範囲")]
    private float _outerRadius = 0.9f;
    [SerializeField, Min(0f), Tooltip("カメラの手前で粒子を消す距離。生成距離より小さく設定する")]
    private float _despawnDistance = 1f;

    private ParticleSystem _particles;
    private float _emissionRemainder;

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = _particles.main;

        // カメラを基準に手動発生させるため、自動発生とShapeによる配置は使わない。
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = _maxParticles;
        main.startSpeed = 0f;
        main.playOnAwake = false;
        main.useUnscaledTime = false;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        ParticleSystem.EmissionModule emission = _particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = _particles.shape;
        shape.enabled = false;

        // 現状は再現用の固定フェード。ここはParticleSystem側へ移せる見た目の設定。
        ParticleSystem.ColorOverLifetimeModule fade = _particles.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.04f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;
    }

    private void OnEnable()
    {
        _emissionRemainder = 0f;
        _particles.Play();
    }

    private void OnDisable()
    {
        if (_particles != null) _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void LateUpdate()
    {
        if (_aircraft == null || _camera == null || !_camera.isActiveAndEnabled) return;

        transform.SetPositionAndRotation(_camera.transform.position, _camera.transform.rotation);
        float intensity = Mathf.InverseLerp(_startSpeed, _fullSpeed, _aircraft.CurrentSpeed);
        EmitWind(intensity, Time.deltaTime);
    }

    /// <summary>
    ///     画面中央を避けた楕円状の範囲に粒子を生成する
    /// </summary>
    /// <param name="intensity"> 速度に対応する演出の強さ </param>
    /// <param name="deltaTime"> 経過時間 </param>
    private void EmitWind(float intensity, float deltaTime)
    {
        _emissionRemainder += _maxEmission * intensity * deltaTime;
        int count = Mathf.Min(Mathf.FloorToInt(_emissionRemainder), _maxEmissionPerFrame);
        _emissionRemainder -= Mathf.Floor(_emissionRemainder);

        float height = Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * _spawnDistance;
        float speed = Mathf.Lerp(_particleSpeedRange.x, _particleSpeedRange.y, intensity);

        for (int i = 0; i < count; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float radius = Random.Range(_innerRadius, Mathf.Max(_innerRadius, _outerRadius));
            var particle = new ParticleSystem.EmitParams
            {
                position = new Vector3(Mathf.Cos(angle) * height * _camera.aspect * radius, Mathf.Sin(angle) * height * radius, _spawnDistance),
                velocity = Vector3.back * speed,
                startLifetime = (_spawnDistance - _despawnDistance) / speed,
                startSize = Random.Range(_widthRange.x, _widthRange.y),
                startColor = _color
            };
            _particles.Emit(particle, 1);
        }
    }
}
