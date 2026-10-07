using System.Collections.Generic;

using UnityEngine;

/// <summary>
///     敵弾の方向と近さを機体周囲の矢印で表示するクラス
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class ProjectileDirectionView : UnityEngine.UI.MaskableGraphic
{
    [Header("参照")]
    [SerializeField] private Transform _player;
    [SerializeField] private Camera _camera;
    [Header("矢印設定")]
    [SerializeField] private float _range = 600f;
    [SerializeField] private float _nearDistance = 40f;
    [SerializeField] private float _farArrowScale = 0.7f;
    [SerializeField] private float _nearArrowScale = 2.5f;
    [SerializeField] private bool _showAircraftIndicators = false;
    [SerializeField] private float _indicatorRadius = 100f;
    [SerializeField] private int _maxIndicators = 8;
    [SerializeField] private Color _missileColor = new Color(1f, 0.35f, 0.12f);
    [SerializeField] private Color _bulletColor = new Color(1f, 0.85f, 0.25f);
    [Header("近距離の点滅")]
    [SerializeField, Min(0f), Tooltip("ミサイルの矢印が点滅し始める距離")]
    private float _blinkDistance = 40f;
    [SerializeField, Min(0.1f), Tooltip("1秒あたりの点滅回数")]
    private float _blinkFrequency = 3f;
    [SerializeField, Range(0f, 1f), Tooltip("点滅で暗くなる瞬間の不透明度倍率")]
    private float _blinkMinAlpha = 0.2f;
    private readonly List<EnemyProjectileSignal> _signals = new List<EnemyProjectileSignal>();
    private readonly List<Vector2> _directions = new List<Vector2>();
    private Vector2 _aircraftPoint;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    private void LateUpdate()
    {
        _signals.Clear();
        _directions.Clear();
        if (_player == null || _camera == null)
        {
            SetVerticesDirty();
            return;
        }
        foreach (var signal in EnemyProjectileSignal.Active)
        {
            if (signal != null && signal.isActiveAndEnabled &&
                (signal.transform.position - _player.position).sqrMagnitude <= _range * _range)
                _signals.Add(signal);
        }
        _signals.Sort((a, b) => (a.transform.position - _player.position).sqrMagnitude.CompareTo(
            (b.transform.position - _player.position).sqrMagnitude));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
            _camera.WorldToScreenPoint(_player.position), null, out _aircraftPoint);
        var rect = rectTransform.rect;
        _aircraftPoint.x = Mathf.Clamp(_aircraftPoint.x, rect.xMin + 170, rect.xMax - 170);
        _aircraftPoint.y = Mathf.Clamp(_aircraftPoint.y, rect.yMin + 170, rect.yMax - 170);
        int count = _showAircraftIndicators ? Mathf.Min(_signals.Count, _maxIndicators) : 0;
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = _signals[i].transform.position - _player.position;
            Vector3 local = _camera.transform.InverseTransformDirection(offset);
            Vector2 direction = new Vector2(local.x, local.y);
            if (direction.sqrMagnitude < 1f) direction = local.z >= 0 ? Vector2.up : Vector2.down;
            _directions.Add(direction.normalized);
        }
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear();
        for (int i = 0; i < _directions.Count; i++)
        {
            Vector2 d = _directions[i], side = new Vector2(-d.y, d.x);
            Vector2 tip = _aircraftPoint + d * (_indicatorRadius + (i % 3) * 25);
            float distance = Vector3.Distance(_player.position, _signals[i].transform.position);
            float proximity = 1f - Mathf.InverseLerp(_nearDistance, _range, distance);
            float scale = Mathf.Lerp(_farArrowScale, _nearArrowScale, proximity * proximity * (3f - 2f * proximity));
            Triangle(vh, tip + d * (9 * scale), tip - d * (5 * scale) + side * (7 * scale), tip - d * (5 * scale) - side * (7 * scale),
                GetIndicatorColor(_signals[i].IsMissile, distance, Time.unscaledTime));
        }
    }

    /// <summary>
    ///     近距離のミサイル矢印だけを実時間で点滅させる
    /// </summary>
    /// <param name="isMissile"> ミサイルかどうか </param>
    /// <param name="distance"> プレイヤーからの距離 </param>
    /// <param name="time"> 点滅に使う経過時間 </param>
    private Color GetIndicatorColor(bool isMissile, float distance, float time)
    {
        Color tint = isMissile ? _missileColor : _bulletColor;
        if (isMissile && _blinkDistance > 0f && distance <= _blinkDistance)
        {
            bool dimmed = Mathf.Repeat(time * _blinkFrequency, 1f) >= 0.5f;
            if (dimmed) tint.a *= _blinkMinAlpha;
        }
        return tint;
    }
    /// <summary>
    ///     三角形を描画する
    /// </summary>
    private void Triangle(UnityEngine.UI.VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color tint)
    {
        int start = vh.currentVertCount;
        vh.AddVert(a, tint, Vector2.zero);
        vh.AddVert(b, tint, Vector2.zero);
        vh.AddVert(c, tint, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
    }
}



