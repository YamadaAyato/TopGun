using UnityEngine;

/// <summary>
///     反撃トークンを管理するクラス
///     ジャスト回避成功で増える / 反撃時に消費する
/// </summary>
public class CounterToken : MonoBehaviour
{
    public int CurrentToken => _currentToken;

    [SerializeField] private int _maxToken;
    private int _currentToken;

    /// <summary>
    ///     ジャスト回避成功時などに呼ばれる
    ///     反撃トークンを増やす
    /// </summary>
    /// <param name="amount"> トークンを増やす数 </param>
    public void AddToken(int amount)
    {
        _currentToken += amount;
        if (_currentToken > _maxToken)
        {
            _currentToken = _maxToken;
        }

        Debug.Log($"Counter Token追加. Current: {_currentToken}/{_maxToken}");
    }

    /// <summary>
    ///     反撃トークンを減らす
    /// </summary>
    /// <param name="amount"> 消費数 </param>
    /// <returns></returns>
    public bool UseToken(int amount)
    {
        if (_currentToken >= amount)
        {
            _currentToken -= amount;
            Debug.Log($"Counter Token使用. Current: {_currentToken}/{_maxToken}");
            return true;
        }
        Debug.Log("Counter Token不足で使用不可.");
        return false;
    }
}
