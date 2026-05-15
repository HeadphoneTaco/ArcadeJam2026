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
        transform.position = new Vector3(transform.position.x, transform.position.y, playerCapsule.transform.position.z + _offset.z); 
    }

    private IEnumerator spawnCarsWithDelay()
    {
        while (_spawnCars)
        {
            int spawnPointOne = Random.Range(0, carSpawnPositions.Length);
            int spawnPointTwo = Random.Range(0, carSpawnPositions.Length);
            int spawnPointThree = Random.Range(0, carSpawnPositions.Length);

            while (spawnPointTwo == spawnPointOne)
            {
                spawnPointTwo = Random.Range(0, carSpawnPositions.Length);
            }
            while (spawnPointThree == spawnPointTwo || spawnPointThree == spawnPointOne)
            {
                spawnPointThree = Random.Range(0, carSpawnPositions.Length);
            }

            int howManyCarsToSpawn = Random.Range(1, 4);

            switch (howManyCarsToSpawn)
            {
                case 1:
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointOne].transform.position, Quaternion.identity);
                    break;

                case 2:
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointOne].transform.position, Quaternion.identity);
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointTwo].transform.position, Quaternion.identity);
                    break;

                case 3:
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointOne].transform.position, Quaternion.identity);
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointTwo].transform.position, Quaternion.identity);
                    Instantiate(carPrefabs[Random.Range(0, carPrefabs.Length)], carSpawnPositions[spawnPointThree].transform.position, Quaternion.identity);
                    break;
                default:
                    break;
            }

            yield return new WaitForSeconds(carSpawnInterval);
        }
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
                Debug.LogWarning($"{ratPrefab.name} is missing a RatMover component. Add RatMover to the rat prefab and assign its hit trigger box in the inspector.", rat);
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
