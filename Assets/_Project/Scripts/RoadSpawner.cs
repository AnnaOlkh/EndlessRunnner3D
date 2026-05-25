using System.Collections.Generic;
using UnityEngine;

public sealed class RoadSpawner : MonoBehaviour
{
    [SerializeField] private GameObject roadSegmentPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform player;

    [SerializeField] private float segmentLength = 30f;
    [SerializeField] private int visibleSegmentCount = 5;
    [SerializeField] private float despawnDistance = 30f;

    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private float obstacleHeight = 0.6f;

    [SerializeField, Min(1)] private int obstacleGroupsPerSegment = 2;
    [SerializeField, Range(0f, 1f)] private float obstacleChancePerGroup = 0.85f;
    [SerializeField, Range(0f, 1f)] private float twoLaneObstacleChance = 0.6f;
    [SerializeField] private float obstacleZJitter = 2f;

    [SerializeField] private int safeSegmentCount = 2;

    [SerializeField] private SkinManager skinManager;

    private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
    private readonly Queue<GameObject> activeObstacles = new Queue<GameObject>();

    private float nextSegmentStartZ = 0f;
    private int spawnedSegmentCount = 0;

    private void Awake()
    {
        if (skinManager == null)
        {
            skinManager = FindFirstObjectByType<SkinManager>();
        }
    }

    private void Start()
    {
        for (int i = 0; i < visibleSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    private void Update()
    {
        if (player == null || roadSegmentPrefab == null)
        {
            return;
        }

        float requiredRoadEndZ = player.position.z + segmentLength * visibleSegmentCount;

        while (nextSegmentStartZ < requiredRoadEndZ)
        {
            SpawnSegment();
        }

        DespawnOldSegments();
        DespawnOldObstacles();
    }

    public void ConfigureDifficulty(
        int groupsPerSegment,
        float chancePerGroup,
        float twoLaneChance
    )
    {
        obstacleGroupsPerSegment = Mathf.Max(1, groupsPerSegment);
        obstacleChancePerGroup = Mathf.Clamp01(chancePerGroup);
        twoLaneObstacleChance = Mathf.Clamp01(twoLaneChance);
    }

    private void SpawnSegment()
    {
        float segmentStartZ = nextSegmentStartZ;
        float segmentCenterZ = segmentStartZ + segmentLength / 2f;

        Vector3 spawnPosition = new Vector3(0f, 0f, segmentCenterZ);

        GameObject segment = Instantiate(
            roadSegmentPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (skinManager != null)
        {
            skinManager.ApplyRoadSkin(segment);
        }

        segment.name = $"RoadSegment_{segmentStartZ:0}";
        activeSegments.Enqueue(segment);

        if (spawnedSegmentCount >= safeSegmentCount)
        {
            SpawnObstacleGroups(segmentStartZ);
        }

        nextSegmentStartZ += segmentLength;
        spawnedSegmentCount++;
    }

    private void SpawnObstacleGroups(float segmentStartZ)
    {
        if (obstaclePrefab == null)
        {
            return;
        }

        int groupCount = Mathf.Max(1, obstacleGroupsPerSegment);
        float step = segmentLength / (groupCount + 1);

        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            if (Random.value > obstacleChancePerGroup)
            {
                continue;
            }

            float baseZ = segmentStartZ + step * (groupIndex + 1);
            float obstacleZ = baseZ + Random.Range(-obstacleZJitter, obstacleZJitter);

            bool shouldBlockTwoLanes = Random.value < twoLaneObstacleChance;

            if (shouldBlockTwoLanes)
            {
                SpawnTwoLaneObstacleGroup(segmentStartZ, obstacleZ, groupIndex);
            }
            else
            {
                SpawnSingleLaneObstacle(segmentStartZ, obstacleZ, groupIndex);
            }
        }
    }

    private void SpawnSingleLaneObstacle(float segmentStartZ, float obstacleZ, int groupIndex)
    {
        int lane = Random.Range(-1, 2);

        SpawnObstacle(
            lane,
            obstacleZ,
            $"Obstacle_{segmentStartZ:0}_{groupIndex}_{lane}"
        );
    }

    private void SpawnTwoLaneObstacleGroup(float segmentStartZ, float obstacleZ, int groupIndex)
    {
        int freeLane = Random.Range(-1, 2);

        for (int lane = -1; lane <= 1; lane++)
        {
            if (lane == freeLane)
            {
                continue;
            }

            SpawnObstacle(
                lane,
                obstacleZ,
                $"Obstacle_{segmentStartZ:0}_{groupIndex}_{lane}"
            );
        }
    }

    private void SpawnObstacle(int lane, float obstacleZ, string obstacleName)
    {
        float obstacleX = lane * laneDistance;

        Vector3 obstaclePosition = new Vector3(
            obstacleX,
            obstacleHeight,
            obstacleZ
        );

        GameObject obstacle = Instantiate(
            obstaclePrefab,
            obstaclePosition,
            Quaternion.identity
        );

        if (skinManager != null)
        {
            skinManager.ApplyObstacleSkin(obstacle);
        }

        obstacle.name = obstacleName;
        activeObstacles.Enqueue(obstacle);
    }

    private void DespawnOldSegments()
    {
        while (activeSegments.Count > 0)
        {
            GameObject oldestSegment = activeSegments.Peek();

            float oldestSegmentEndZ = oldestSegment.transform.position.z + segmentLength / 2f;

            bool isBehindPlayer = oldestSegmentEndZ < player.position.z - despawnDistance;

            if (!isBehindPlayer)
            {
                break;
            }

            activeSegments.Dequeue();
            Destroy(oldestSegment);
        }
    }

    private void DespawnOldObstacles()
    {
        while (activeObstacles.Count > 0)
        {
            GameObject oldestObstacle = activeObstacles.Peek();

            if (oldestObstacle == null)
            {
                activeObstacles.Dequeue();
                continue;
            }

            bool isBehindPlayer = oldestObstacle.transform.position.z < player.position.z - despawnDistance;

            if (!isBehindPlayer)
            {
                break;
            }

            activeObstacles.Dequeue();
            Destroy(oldestObstacle);
        }
    }
}