using System;
using UnityEngine;

/// <summary>
///     機体の移動経路を検査し、軽い接触を滑らせ、強い地形衝突を通知するクラス
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(PlayerHealth))]
public class AircraftCollisionGuard : MonoBehaviour
{
    /// <summary> 地形への墜落が確定した時のイベント </summary>
    public event Action OnCrashed;

    [SerializeField] private LayerMask _obstacleLayers = ~0;
    [SerializeField, Min(0f), Tooltip("壁へ向かう速度がこの値以上なら墜落")]
    private float _fatalNormalSpeed = 12f;
    [SerializeField, Range(0f, 1f), Tooltip("浅い接触を許容する角度の内積閾値")]
    private float _fatalApproachDot = 0.2f;
    [SerializeField, Min(0.01f), Tooltip("許容するめり込み距離")]
    private float _fatalPenetration = 0.4f;
    [SerializeField, Min(0.001f)] private float _skin = 0.03f;

    private Rigidbody _body;
    private BoxCollider _box;
    private PlayerHealth _health;
    private bool _crashed;
    private readonly RaycastHit[] _hits = new RaycastHit[32];
    private readonly Collider[] _overlaps = new Collider[32];

    /// <summary>
    ///     移動経路で衝突を検査し、接触面に沿った移動先へ補正する
    /// </summary>
    /// <param name="destination"> 移動予定の位置 </param>
    /// <param name="safePosition"> 衝突を考慮した移動先 </param>
    /// <returns> 墜落していなければtrue </returns>
    public bool ConstrainMove(Vector3 destination, out Vector3 safePosition)
    {
        safePosition = _body.position;
        if (_crashed || _health.IsDead) return false;
        Quaternion rotation = _body.rotation;
        Vector3 scale = transform.lossyScale;
        Vector3 half = Vector3.Scale(_box.size * 0.5f, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        Vector3 centerOffset = rotation * Vector3.Scale(_box.center, scale);
        Vector3 remaining = destination - safePosition;
        int count = Physics.OverlapBoxNonAlloc(safePosition + centerOffset, half, _overlaps, rotation, _obstacleLayers, QueryTriggerInteraction.Ignore);
        Collider[] overlaps = count == _overlaps.Length ? Physics.OverlapBox(safePosition + centerOffset, half, rotation, _obstacleLayers, QueryTriggerInteraction.Ignore) : _overlaps;
        if (overlaps != _overlaps) count = overlaps.Length;
        for (int i = 0; i < count; i++)
        {
            var other = overlaps[i];
            if (!IsObstacle(other)) continue;
            if (!Physics.ComputePenetration(_box, safePosition, rotation, other, other.transform.position, other.transform.rotation, out Vector3 direction, out float depth)) continue;
            if (depth >= _fatalPenetration) { Crash(safePosition); return false; }
            safePosition += direction * (depth + _skin);
            if (Vector3.Dot(remaining, direction) < 0f) remaining = Vector3.ProjectOnPlane(remaining, direction);
        }
        for (int step = 0; step < 3 && remaining.sqrMagnitude > 0.000001f; step++)
        {
            float length = remaining.magnitude;
            Vector3 direction = remaining / length;
            count = Physics.BoxCastNonAlloc(safePosition + centerOffset, half, direction, _hits, rotation, length + _skin, _obstacleLayers, QueryTriggerInteraction.Ignore);
            RaycastHit[] hits = count == _hits.Length ? Physics.BoxCastAll(safePosition + centerOffset, half, direction, rotation, length + _skin, _obstacleLayers, QueryTriggerInteraction.Ignore) : _hits;
            if (hits != _hits) count = hits.Length;
            float nearest = float.MaxValue;
            RaycastHit hit = default;
            for (int i = 0; i < count; i++)
            {
                if (IsObstacle(hits[i].collider) && hits[i].distance < nearest)
                { nearest = hits[i].distance; hit = hits[i]; }
            }
            if (nearest == float.MaxValue) { safePosition += remaining; break; }
            float travel = Mathf.Clamp(nearest - _skin, 0f, length);
            safePosition += direction * travel;
            float approach = Mathf.Max(0f, -Vector3.Dot(direction, hit.normal));
            float normalSpeed = length / Time.fixedDeltaTime * approach;
            if (approach >= _fatalApproachDot && normalSpeed >= _fatalNormalSpeed)
            { Crash(safePosition); return false; }
            remaining = Vector3.ProjectOnPlane(direction * (length - travel), hit.normal);
        }
        return true;
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _box = GetComponent<BoxCollider>();
        _health = GetComponent<PlayerHealth>();
    }

    /// <summary>
    ///     自機・敵・弾・フレアを除き、実体のある環境物を対象にする
    /// </summary>
    private bool IsObstacle(Collider other)
    {
        return other != null && !other.isTrigger && other.attachedRigidbody != _body &&
            !other.transform.IsChildOf(transform) && other.GetComponentInParent<EnemyBase>() == null &&
            other.GetComponentInParent<BulletBase>() == null && other.GetComponentInParent<FlareDecoyRoot>() == null;
    }

    /// <summary>
    ///     移動を停止し、墜落と死亡を一度だけ通知する
    /// </summary>
    private void Crash(Vector3 position)
    {
        if (_crashed) return;
        _crashed = true;
        _body.position = position;
        if (!_body.isKinematic) { _body.linearVelocity = Vector3.zero; _body.angularVelocity = Vector3.zero; }
        var mover = GetComponent<PlayerAirCraftController>();
        if (mover != null) { mover.DisableControl = true; mover.enabled = false; }
        var evade = GetComponent<PlayerEvadeController>();
        if (evade != null) evade.enabled = false;
        OnCrashed?.Invoke();
        _health.Die();
    }
}