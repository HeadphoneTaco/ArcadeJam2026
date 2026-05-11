using UnityEngine;
using System.Collections;

public class BasicCarSpawner : MonoBehaviour
{
    private Vector3 _offset;

    private bool _spawnCars;

    [SerializeField] private GameObject playerCapsule;

    [SerializeField] private GameObject[] carSpawnPositions;

    [SerializeField] private GameObject[] carPrefabs;

    [SerializeField] private float carSpawnInterval;

    void Start()
    {
        _offset = transform.position - playerCapsule.transform.position;
        _spawnCars = true;

        StartCoroutine(spawnCarsWithDelay());
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
}
