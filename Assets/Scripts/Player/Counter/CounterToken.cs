using UnityEngine;

public class CounterToken : MonoBehaviour
{
    public int CurrentToken => _currentToken;

    [SerializeField] private int _maxToken;
    private int _currentToken;

    public void AddToken(int amount)
    {
        _currentToken += amount;
        if (_currentToken > _maxToken)
        {
            _currentToken = _maxToken;
        }

        Debug.Log($"Counter Token追加. Current: {_currentToken}/{_maxToken}");
    }

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
