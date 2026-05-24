using UnityEngine;

public sealed class PlayerCollectibles : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;

    private void Awake()
    {
        if (coinManager == null)
        {
            coinManager = FindFirstObjectByType<CoinManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Collectible"))
        {
            return;
        }

        Collectible collectible = other.GetComponent<Collectible>();

        if (collectible == null)
        {
            return;
        }

        if (coinManager != null)
        {
            coinManager.AddCoins(collectible.Value);
        }

        Destroy(other.gameObject);
    }
}