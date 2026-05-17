using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BasicCarSpawner : MonoBehaviour
{
    private Vector3 _offset;

    private bool _spawnCars;

    [SerializeField] private GameObject playerCapsule;

    [SerializeField] private GameObject[] carSpawnPositions;

    [SerializeField] private GameObject[] carPrefabs;

    [SerializeField] private float carSpawnInterval;

    [Header("Bottle Spawning")]
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private float bottleMoveSpeed = 0.7f;
    [SerializeField] private float bottleSpawnYOffset;

    [Header("Rat Spawning")]
    [SerializeField] private bool spawnRats = true;
    [SerializeField] private GameObject[] ratPrefabs;
    [SerializeField] private Transform[] leftRatSpawnPositions;
    [SerializeField] private Transform[] rightRatSpawnPositions;
    [SerializeField] private float ratSpawnInterval = 4f;
    [SerializeField] private Vector2 ratMoveSpeedRange = new Vector2(10f, 14f);
    [SerializeField] private Vector3 ratMoveDirection = Vector3.back;
    [SerializeField] private bool useRatSpawnPointForward;

    private void Reset()
    {
        FillRatDefaults();
    }

    private void OnValidate()
    {
        FillRatDefaults();
    }

    void Start()
    {
        FillRatDefaults();

        if (playerCapsule == null)
        {
            enabled = false;
            return;
        }

        _offset = transform.position - playerCapsule.transform.position;
        _spawnCars = true;

        StartCoroutine(spawnCarsWithDelay());

        if (spawnRats)
        {
            StartCoroutine(SpawnRatsWithDelay());
        }
    }

    void Update()
    {
        if (playerCapsule == null)
        {
            return;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, playerCapsule.transform.position.z + _offset.z); 
    }

    private IEnumerator spawnCarsWithDelay()
    {
        while (_spawnCars)
        {
            if (carSpawnPositions == null || carSpawnPositions.Length == 0)
            {
                yield return new WaitForSeconds(carSpawnInterval);
                continue;
            }

            List<int> selectedCarSpawnPoints = GetRandomSpawnPointIndexes(Mathf.Min(3, carSpawnPositions.Length));

            if (selectedCarSpawnPoints.Count == 0)
            {
                yield return new WaitForSeconds(carSpawnInterval);
                continue;
            }

            int howManyCarsToSpawn = Random.Range(1, selectedCarSpawnPoints.Count + 1);

            for (int i = 0; i < howManyCarsToSpawn; i++)
            {
                SpawnCarAt(selectedCarSpawnPoints[i]);
            }

            SpawnBottleInOpenSpawnPoint(selectedCarSpawnPoints, howManyCarsToSpawn);

            yield return new WaitForSeconds(carSpawnInterval);
        }
    }

    private List<int> GetRandomSpawnPointIndexes(int amount)
    {
        List<int> availableSpawnPoints = new List<int>();

        for (int i = 0; i < carSpawnPositions.Length; i++)
        {
            if (carSpawnPositions[i] != null)
            {
                availableSpawnPoints.Add(i);
            }
        }

        List<int> selectedSpawnPoints = new List<int>();

        while (selectedSpawnPoints.Count < amount && availableSpawnPoints.Count > 0)
        {
            int availableIndex = Random.Range(0, availableSpawnPoints.Count);
            selectedSpawnPoints.Add(availableSpawnPoints[availableIndex]);
            availableSpawnPoints.RemoveAt(availableIndex);
        }

        return selectedSpawnPoints;
    }

    private void SpawnCarAt(int spawnPointIndex)
    {
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            return;
        }

        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        if (carPrefab == null)
        {
            return;
        }

        Instantiate(carPrefab, carSpawnPositions[spawnPointIndex].transform.position, Quaternion.identity);
    }

    private void SpawnBottleInOpenSpawnPoint(List<int> selectedCarSpawnPoints, int spawnedCarCount)
    {
        if (bottlePrefab == null || carSpawnPositions == null || carSpawnPositions.Length == 0)
        {
            return;
        }

        List<int> openSpawnPoints = new List<int>();

        for (int i = 0; i < carSpawnPositions.Length; i++)
        {
            if (carSpawnPositions[i] == null)
            {
                continue;
            }

            bool hasCar = false;

            for (int j = 0; j < spawnedCarCount; j++)
            {
                if (selectedCarSpawnPoints[j] == i)
                {
                    hasCar = true;
                    break;
                }
            }

            if (!hasCar)
            {
                openSpawnPoints.Add(i);
            }
        }

        if (openSpawnPoints.Count == 0)
        {
            return;
        }

        int spawnPointIndex = openSpawnPoints[Random.Range(0, openSpawnPoints.Count)];
        Vector3 bottleSpawnPosition = carSpawnPositions[spawnPointIndex].transform.position + Vector3.up * bottleSpawnYOffset;
        GameObject bottle = Instantiate(bottlePrefab, bottleSpawnPosition, Quaternion.identity);
        BottlePickup bottlePickup = bottle.GetComponent<BottlePickup>();

        if (bottlePickup == null)
        {
            bottlePickup = bottle.AddComponent<BottlePickup>();
        }

        bottlePickup.Initialize(bottleMoveSpeed);
    }

    private IEnumerator SpawnRatsWithDelay()
    {
        while (spawnRats)
        {
            SpawnRatsAtPositions(leftRatSpawnPositions);
            SpawnRatsAtPositions(rightRatSpawnPositions);

            yield return new WaitForSeconds(ratSpawnInterval);
        }
    }

    private void SpawnRatsAtPositions(Transform[] spawnPositions)
    {
        if (ratPrefabs == null || ratPrefabs.Length == 0 || spawnPositions == null)
        {
            return;
        }

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            Transform spawnPosition = spawnPositions[i];

            if (spawnPosition == null)
            {
                continue;
            }

            GameObject ratPrefab = ratPrefabs[Random.Range(0, ratPrefabs.Length)];

            if (ratPrefab == null)
            {
                continue;
            }

            Quaternion rotation = useRatSpawnPointForward ? spawnPosition.rotation : Quaternion.identity;
            GameObject rat = Instantiate(ratPrefab, spawnPosition.position, rotation);
            RatMover ratMover = rat.GetComponent<RatMover>();

            if (ratMover == null)
            {
                ratMover = rat.AddComponent<RatMover>();
                Debug.LogWarning($"{ratPrefab.name} is missing a RatMover component. Add RatMover to the rat prefab and assign its solid hit box collider in the inspector.", rat);
            }

            Vector3 moveDirection = useRatSpawnPointForward ? spawnPosition.forward : ratMoveDirection;
            ratMover.Initialize(GetRandomRatMoveSpeed(), moveDirection);
        }
    }

    private float GetRandomRatMoveSpeed()
    {
        float minSpeed = Mathf.Min(ratMoveSpeedRange.x, ratMoveSpeedRange.y);
        float maxSpeed = Mathf.Max(ratMoveSpeedRange.x, ratMoveSpeedRange.y);
        return Random.Range(minSpeed, maxSpeed);
    }

    private void FillRatDefaults()
    {
        if (leftRatSpawnPositions == null || leftRatSpawnPositions.Length == 0)
        {
            leftRatSpawnPositions = FindChildTransformsByName("RatSpawnLocationLeft");
        }

        if (rightRatSpawnPositions == null || rightRatSpawnPositions.Length == 0)
        {
            rightRatSpawnPositions = FindChildTransformsByName("RatSpawnLocationRight");
        }

#if UNITY_EDITOR
        if (ratPrefabs == null || ratPrefabs.Length == 0)
        {
            ratPrefabs = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Rat.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RatSmall.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/RatLarge.prefab")
            };
        }
#endif
    }

    private Transform[] FindChildTransformsByName(string namePrefix)
    {
        List<Transform> matches = new List<Transform>();
        AddMatchingChildTransforms(transform, namePrefix, matches);
        matches.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        return matches.ToArray();
    }

    private void AddMatchingChildTransforms(Transform parent, string namePrefix, List<Transform> matches)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name.StartsWith(namePrefix))
            {
                matches.Add(child);
            }

            AddMatchingChildTransforms(child, namePrefix, matches);
        }
    }
}
