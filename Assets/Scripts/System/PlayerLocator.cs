using UnityEngine;

public class PlayerLocator : MonoBehaviour
{
    public static PlayerLocator Instance { get; private set; }

    public PlayerHealth PlayerHealth { get; private set; }

    public Transform PlayerTransform { get; private set; }

    public void Register(PlayerHealth playerHealth)
    {
        PlayerHealth = playerHealth;
        PlayerTransform = playerHealth.transform;
    }

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
