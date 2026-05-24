using System.Collections.Generic;
using UnityEngine;

public sealed class CollectibleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject bonusCoinPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float segmentLength = 30f;
    [SerializeField] private float spawnAheadDistance = 120f;
    [SerializeField] private float despawnDistance = 30f;
    [SerializeField] private int safeSegmentCount = 2;

    [Header("Lanes")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float collectibleHeight = 1f;

    [Header("Coin Groups")]
    [SerializeField, Range(0f, 1f)] private float coinGroupChance = 0.40f;
    [SerializeField] private int minCoinsInGroup = 1;
    [SerializeField] private int maxCoinsInGroup = 6;
    [SerializeField] private float coinSpacing = 1.4f;

    [Header("Bonus")]
    [SerializeField, Range(0f, 1f)] private float bonusChance = 0.12f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleAvoidanceRadius = 1.25f;

    private readonly Queue<GameObject> activeCollectibles = new Queue<GameObject>();

    private float nextSegmentStartZ;
    private int spawnedSegmentCount;

    private void Update()
    {
        if (player == null || coinPrefab == null)
        {
            return;
        }

        float requiredEndZ = player.position.z + spawnAheadDistance;

        while (nextSegmentStartZ < requiredEndZ)
        {
            TrySpawnCollectiblesForSegment(nextSegmentStartZ);

            nextSegmentStartZ += segmentLength;
            spawnedSegmentCount++;
        }

        DespawnOldCollectibles();
    }

    private void TrySpawnCollectiblesForSegment(float segmentStartZ)
    {
        if (spawnedSegmentCount < safeSegmentCount)
        {
            return;
        }

        if (Random.value > coinGroupChance)
        {
            return;
        }

        int lane = Random.Range(-1, 2);
        float x = lane * laneDistance;

        float startZ = segmentStartZ + Random.Range(
            segmentLength * 0.25f,
            segmentLength * 0.75f
        );

        bool spawnBonus = bonusCoinPrefab != null && Random.value < bonusChance;

        if (spawnBonus)
        {
            Vector3 bonusPosition = new Vector3(x, collectibleHeight, startZ);

            if (IsNearObstacle(bonusPosition))
            {
                return;
            }

            SpawnCollectible(
                bonusCoinPrefab,
                bonusPosition,
                "BonusCoin"
            );

            return;
        }

        int coinCount = Random.Range(minCoinsInGroup, maxCoinsInGroup + 1);

        if (!CanSpawnCoinGroup(x, startZ, coinCount))
        {
            return;
        }

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 coinPosition = new Vector3(
                x,
                collectibleHeight,
                startZ + i * coinSpacing
            );

            SpawnCollectible(
                coinPrefab,
                coinPosition,
                "Coin"
            );
        }
    }

    private bool CanSpawnCoinGroup(float x, float startZ, int coinCount)
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 coinPosition = new Vector3(
                x,
                collectibleHeight,
                startZ + i * coinSpacing
            );

            if (IsNearObstacle(coinPosition))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsNearObstacle(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position,
            obstacleAvoidanceRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Obstacle"))
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnCollectible(
        GameObject prefab,
        Vector3 position,
        string objectName
    )
    {
        GameObject collectible = Instantiate(
            prefab,
            position,
            Quaternion.identity
        );

        collectible.name = $"{objectName}_{position.z:0}";
        activeCollectibles.Enqueue(collectible);
    }

    private void DespawnOldCollectibles()
    {
        while (activeCollectibles.Count > 0)
        {
            GameObject oldestCollectible = activeCollectibles.Peek();

            if (oldestCollectible == null)
            {
                activeCollectibles.Dequeue();
                continue;
            }

            bool isBehindPlayer = oldestCollectible.transform.position.z < player.position.z - despawnDistance;

            if (!isBehindPlayer)
            {
                break;
            }

            activeCollectibles.Dequeue();
            Destroy(oldestCollectible);
        }
    }
}