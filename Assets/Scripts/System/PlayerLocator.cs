using UnityEngine;

/// <summary>
///     プレイヤーの参照情報を保持するクラス
/// </summary>
public class PlayerLocator : MonoBehaviour
{
    public static PlayerLocator Instance { get; private set; }

    /// <summary> プレイヤーの体力情報 </summary>
    public PlayerHealth PlayerHealth { get; private set; }

    /// <summary> プレイヤーのTransform情報 </summary>
    public Transform PlayerTransform { get; private set; }

    /// <summary>
    ///     プレイヤーを登録する
    /// </summary>
    /// <param name="playerHealth"></param>
    public void Register(PlayerHealth playerHealth)
    {
        PlayerHealth = playerHealth;
        PlayerTransform = playerHealth.transform;
    }

    /// <summary>
    ///     プレイヤーの登録を解除する
    /// </summary>
    public void Unregister()
    {
        PlayerHealth = null;
        PlayerTransform = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
}
