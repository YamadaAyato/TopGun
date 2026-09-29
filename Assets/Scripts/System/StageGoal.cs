using System;
using UnityEngine;

/// <summary>
///     プレイヤーのゴール通過を一度だけ通知するクラス
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class StageGoal : MonoBehaviour
{
    /// <summary> プレイヤーがゴールを通過した時のイベント </summary>
    public event Action<PlayerHealth> OnReached;

    private bool _isReached;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isReached || other.isTrigger) return;

        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.IsDead) return;

        _isReached = true;
        OnReached?.Invoke(player);
    }
}
