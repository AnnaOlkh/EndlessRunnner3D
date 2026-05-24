using UnityEngine;

public sealed class PlayerCollision : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle"))
        {
            return;
        }

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
}