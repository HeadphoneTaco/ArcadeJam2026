using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StraightRoadHouseSpawner : MonoBehaviour
{
    private const string GeneratedContainerName = "Generated Houses";

    private enum RoadSide
    {
        Left,
        Right
    }

    [Header("Prefabs")]
    [SerializeField] private GameObject baseHouseStreetPrefab;
    [SerializeField] private GameObject tallHouseStreetPrefab;
    [SerializeField] private GameObject shortHouseStreetPrefab;

    [Header("Street Layout")]
    [SerializeField] private int lotCount = 20;
    [SerializeField] private float firstLotZ = 18.85632f;
    [SerializeField] private float lotSpacing = 40f;
    [SerializeField] private float houseY = 19.46f;
    [SerializeField] private float leftHouseX = -3f;
    [SerializeField] private float rightHouseX = 4.15f;
    [SerializeField] private bool parentGeneratedHouses = true;

    [Header("Generation")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool randomizePrefabs = false;
    [SerializeField] private int randomSeed = 12345;
    [SerializeField] private GameObject[] leftPattern;
    [SerializeField] private GameObject[] rightPattern;

    [Header("Final House")]
    [SerializeField] private bool spawnFinalHouse = true;
    [SerializeField] private GameObject finalHousePrefab;
    [SerializeField] private float finalHouseX = 0f;
    [SerializeField] private float finalHouseY = 19.8f;
    [SerializeField] private float finalHouseDistanceAfterLastLot = 40f;
    [SerializeField] private float finalHouseYawDegrees = -90f;
    [SerializeField] private Vector3 finalHouseWinTriggerCenter = new Vector3(0f, -16f, 0f);
    [SerializeField] private Vector3 finalHouseWinTriggerSize = new Vector3(18f, 12f, 18f);
    [SerializeField] private string finalHouseWinMessage = "YOU MADE IT HOME";

    private readonly List<GameObject> spawnedHouses = new List<GameObject>();

    private void Reset()
    {
        FillPrefabReferences();
        leftPattern = new[] { baseHouseStreetPrefab, tallHouseStreetPrefab };
        rightPattern = new[] { shortHouseStreetPrefab, baseHouseStreetPrefab };
    }

    private void OnValidate()
    {
        FillPrefabReferences();
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnStreet();
        }
    }

    [ContextMenu("Spawn Street")]
    public void SpawnStreet()
    {
        ClearSpawnedHouses();

        if (lotCount <= 0)
        {
            return;
        }

        if (randomizePrefabs)
        {
            Random.State previousRandomState = Random.state;
            Random.InitState(randomSeed);
            SpawnLots();
            Random.state = previousRandomState;
            return;
        }

        SpawnLots();
    }

    private void SpawnLots()
    {
        for (int lotIndex = 0; lotIndex < lotCount; lotIndex++)
        {
            float z = firstLotZ + lotSpacing * lotIndex;

            SpawnHouse(ChoosePrefab(lotIndex, RoadSide.Left), RoadSide.Left, z);
            SpawnHouse(ChoosePrefab(lotIndex, RoadSide.Right), RoadSide.Right, z);
        }

        if (spawnFinalHouse)
        {
            SpawnFinalHouse();
        }
    }

    private void FillPrefabReferences()
    {
#if UNITY_EDITOR
        if (baseHouseStreetPrefab == null)
        {
            baseHouseStreetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/BaseHouseStreet.prefab");
        }

        if (tallHouseStreetPrefab == null)
        {
            tallHouseStreetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TallHouseStreet.prefab");
        }

        if (shortHouseStreetPrefab == null)
        {
            shortHouseStreetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ShortHouseStreet.prefab");
        }
#endif
    }

    [ContextMenu("Clear Spawned Houses")]
    public void ClearSpawnedHouses()
    {
        Transform container = transform.Find(GeneratedContainerName);

        if (container != null)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                DestroyHouse(container.GetChild(i).gameObject);
            }
        }

        for (int i = spawnedHouses.Count - 1; i >= 0; i--)
        {
            GameObject spawnedHouse = spawnedHouses[i];

            if (spawnedHouse == null)
            {
                continue;
            }

            DestroyHouse(spawnedHouse);
        }

        spawnedHouses.Clear();
    }

    private GameObject ChoosePrefab(int lotIndex, RoadSide side)
    {
        if (randomizePrefabs)
        {
            GameObject[] prefabs = { baseHouseStreetPrefab, tallHouseStreetPrefab, shortHouseStreetPrefab };
            return prefabs[Random.Range(0, prefabs.Length)];
        }

        GameObject[] pattern = side == RoadSide.Left ? leftPattern : rightPattern;

        if (pattern != null && pattern.Length > 0)
        {
            GameObject patternedPrefab = pattern[lotIndex % pattern.Length];

            if (patternedPrefab != null)
            {
                return patternedPrefab;
            }
        }

        return GetDefaultPrefab(lotIndex, side);
    }

    private void SpawnHouse(GameObject prefab, RoadSide side, float z)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"{nameof(StraightRoadHouseSpawner)} is missing a prefab for the {side} side.", this);
            return;
        }

        Vector3 localPosition = new Vector3(side == RoadSide.Left ? leftHouseX : rightHouseX, houseY, z);
        Vector3 worldPosition = transform.TransformPoint(localPosition);
        Quaternion worldRotation = transform.rotation;
        Transform parent = parentGeneratedHouses ? GetOrCreateGeneratedContainer() : null;

        GameObject house = Instantiate(prefab, worldPosition, worldRotation, parent);
        house.name = $"{prefab.name}_{side}_{spawnedHouses.Count:00}";

        MirrorHouseToSide(house.transform, side);
        spawnedHouses.Add(house);
    }

    private void SpawnFinalHouse()
    {
        GameObject prefab = GetFinalHousePrefab();

        if (prefab == null)
        {
            Debug.LogWarning($"{nameof(StraightRoadHouseSpawner)} is missing a prefab for the final house.", this);
            return;
        }

        float lastLotZ = firstLotZ + lotSpacing * (lotCount - 1);
        float finalHouseZ = lastLotZ + finalHouseDistanceAfterLastLot;
        Vector3 localPosition = new Vector3(finalHouseX, finalHouseY, finalHouseZ);
        Vector3 worldPosition = transform.TransformPoint(localPosition);
        Quaternion worldRotation = transform.rotation * Quaternion.Euler(0f, finalHouseYawDegrees, 0f);
        Transform parent = parentGeneratedHouses ? GetOrCreateGeneratedContainer() : null;

        GameObject house = Instantiate(prefab, worldPosition, worldRotation, parent);
        house.name = $"{prefab.name}_Final";

        KeepHouseUnmirrored(house.transform);
        spawnedHouses.Add(house);

        CreateFinalHouseWinTrigger(house, parent);
    }

    private GameObject GetFinalHousePrefab()
    {
        return finalHousePrefab != null ? finalHousePrefab : baseHouseStreetPrefab;
    }

    private GameObject GetDefaultPrefab(int lotIndex, RoadSide side)
    {
        if (side == RoadSide.Left)
        {
            return lotIndex % 2 == 0 ? baseHouseStreetPrefab : tallHouseStreetPrefab;
        }

        return lotIndex % 2 == 0 ? shortHouseStreetPrefab : baseHouseStreetPrefab;
    }

    private Transform GetOrCreateGeneratedContainer()
    {
        Transform container = transform.Find(GeneratedContainerName);

        if (container != null)
        {
            return container;
        }

        GameObject containerObject = new GameObject(GeneratedContainerName);
        containerObject.transform.SetParent(transform, false);
        return containerObject.transform;
    }

    private static void DestroyHouse(GameObject house)
    {
        if (Application.isPlaying)
        {
            Destroy(house);
        }
        else
        {
            DestroyImmediate(house);
        }
    }

    private static void MirrorHouseToSide(Transform house, RoadSide side)
    {
        Vector3 scale = house.localScale;
        float xMagnitude = Mathf.Abs(scale.x);
        scale.x = side == RoadSide.Right ? -xMagnitude : xMagnitude;
        house.localScale = scale;
    }

    private static void KeepHouseUnmirrored(Transform house)
    {
        Vector3 scale = house.localScale;
        scale.x = Mathf.Abs(scale.x);
        house.localScale = scale;
    }

    private void CreateFinalHouseWinTrigger(GameObject house, Transform parent)
    {
        GameObject triggerObject = new GameObject($"{house.name}_WinTrigger");
        Vector3 triggerWorldPosition = house.transform.position + house.transform.rotation * finalHouseWinTriggerCenter;
        triggerObject.transform.SetPositionAndRotation(triggerWorldPosition, house.transform.rotation);
        triggerObject.transform.SetParent(parent, true);

        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = finalHouseWinTriggerSize;

        FinalHouseWinTrigger winTrigger = triggerObject.AddComponent<FinalHouseWinTrigger>();
        winTrigger.Initialize(finalHouseWinMessage);

        spawnedHouses.Add(triggerObject);
    }
}
