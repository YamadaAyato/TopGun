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

    [SerializeField, Tooltip("地形・障害物として移動経路を検査するレイヤー")] private LayerMask _obstacleLayers = ~0;
    [SerializeField, Min(0f), Tooltip("壁へ向かう速度がこの値以上なら墜落")]
    private float _fatalNormalSpeed = 12f;
    [SerializeField, Range(0f, 1f), Tooltip("浅い接触を許容する角度の内積閾値")]
    private float _fatalApproachDot = 0.2f;
    [SerializeField, Min(0.01f), Tooltip("許容するめり込み距離")]
    private float _fatalPenetration = 0.4f;
    [SerializeField, Min(0.001f), Tooltip("接触面との間に確保する余白")] private float _skin = 0.03f;

    private Rigidbody _body;
    private BoxCollider _box;
    private PlayerHealth _health;
    private bool _crashed;
    private readonly RaycastHit[] _hits = new RaycastHit[QUERY_CAPACITY];
    private readonly Collider[] _overlaps = new Collider[QUERY_CAPACITY];

    private const int QUERY_CAPACITY = 32;
    private const int MAX_SLIDE_STEPS = 3;
    private const float MIN_MOVEMENT_SQUARED = 0.000001f;

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
        Vector3 half = Vector3.Scale(
            _box.size * 0.5f,
            new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
        Vector3 centerOffset = rotation * Vector3.Scale(_box.center, scale);
        Vector3 remaining = destination - safePosition;
        if (!ResolvePenetration(rotation, half, centerOffset, ref safePosition, ref remaining))
            return false;

        return SweepMovement(rotation, half, centerOffset, remaining, ref safePosition);
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _box = GetComponent<BoxCollider>();
        _health = GetComponent<PlayerHealth>();
    }

    /// <summary>
    ///     移動前のめり込みを解消し、深い侵入は墜落として扱う。
    /// </summary>
    private bool ResolvePenetration(Quaternion rotation, Vector3 half, Vector3 centerOffset,
        ref Vector3 safePosition, ref Vector3 remaining)
    {
        int count = Physics.OverlapBoxNonAlloc(
            safePosition + centerOffset, half, _overlaps, rotation,
            _obstacleLayers, QueryTriggerInteraction.Ignore);

        // バッファが埋まった場合は再取得し、未検査の障害物を残さない。
        Collider[] overlaps = count == _overlaps.Length
            ? Physics.OverlapBox(safePosition + centerOffset, half, rotation,
                _obstacleLayers, QueryTriggerInteraction.Ignore)
            : _overlaps;
        if (overlaps != _overlaps) count = overlaps.Length;

        for (int i = 0; i < count; i++)
        {
            Collider other = overlaps[i];
            if (!IsObstacle(other)) continue;
            if (!Physics.ComputePenetration(
                _box, safePosition, rotation, other,
                other.transform.position, other.transform.rotation,
                out Vector3 direction, out float depth)) continue;

            if (depth >= _fatalPenetration)
            {
                Crash(safePosition);
                return false;
            }

            safePosition += direction * (depth + _skin);
            if (Vector3.Dot(remaining, direction) < 0f) remaining = Vector3.ProjectOnPlane(remaining, direction);
        }

        return true;
    }

    /// <summary>
    ///     移動経路を掃引し、浅い接触では面に沿って残りの移動を続ける。
    /// </summary>
    private bool SweepMovement(Quaternion rotation, Vector3 half, Vector3 centerOffset,
        Vector3 remaining, ref Vector3 safePosition)
    {
        for (int step = 0; step < MAX_SLIDE_STEPS && remaining.sqrMagnitude > MIN_MOVEMENT_SQUARED; step++)
        {
            float length = remaining.magnitude;
            Vector3 direction = remaining / length;
            int count = Physics.BoxCastNonAlloc(
                safePosition + centerOffset, half, direction, _hits, rotation,
                length + _skin, _obstacleLayers, QueryTriggerInteraction.Ignore);

            // 密集した場所でも、配列に入りきらない衝突を見落とさない。
            RaycastHit[] hits = count == _hits.Length
                ? Physics.BoxCastAll(safePosition + centerOffset, half, direction, rotation,
                    length + _skin, _obstacleLayers, QueryTriggerInteraction.Ignore)
                : _hits;
            if (hits != _hits) count = hits.Length;
            float nearest = float.MaxValue;
            RaycastHit hit = default;

            for (int i = 0; i < count; i++)
            {
                if (IsObstacle(hits[i].collider) && hits[i].distance < nearest)
                {
                    nearest = hits[i].distance;
                    hit = hits[i];
                }
            }

            if (nearest == float.MaxValue)
            {
                safePosition += remaining;
                break;
            }

            float travel = Mathf.Clamp(nearest - _skin, 0f, length);
            safePosition += direction * travel;
            float approach = Mathf.Max(0f, -Vector3.Dot(direction, hit.normal));
            float normalSpeed = length / Time.fixedDeltaTime * approach;
            if (approach >= _fatalApproachDot && normalSpeed >= _fatalNormalSpeed)
            {
                Crash(safePosition);
                return false;
            }

            remaining = Vector3.ProjectOnPlane(direction * (length - travel), hit.normal);
        }

        return true;
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
        if (!_body.isKinematic)
        {
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }

        if (TryGetComponent(out PlayerFlightController mover))
        {
            mover.DisableControl = true;
            mover.enabled = false;
        }

        if (TryGetComponent(out PlayerEvadeController evade)) evade.enabled = false;
        OnCrashed?.Invoke();
        _health.Die();
    }
}
