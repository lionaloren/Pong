using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType 
{
    DoublePoint, 
    ChangeDirection,
    SpeedUp
}

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Power Up Settings")]
    public float spawnInterval = 15f; 

    public float powerUpDuration = 6f; 

    [Tooltip("Drag objek Power Up yang ada di scene ke sini.")]
    public GameObject[] powerUpObjects; 

    [Header("Spawn Boundaries")]
    public float xBound = 4f; 
    public float yBound = 2.5f; 

    private Coroutine spawnCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        HideAllPowerUps();
    }

    private void HideAllPowerUps()
    {
        foreach (GameObject pu in powerUpObjects)
        {
            if (pu != null)
            {
                pu.SetActive(false);
            }
        }
    }

    public void StartSpawning()
    {
        HideAllPowerUps(); 

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnPowerUpRoutine());
    }

    public void StopSpawningAndClear()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        HideAllPowerUps();
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(spawnInterval / 2f); 

        while (true)
        {

            if (GameManager.Instance != null && (GameManager.Instance.isGameRunning || GameManager.Instance.isGoldenGoal))
            {
                SpawnRandomPowerUp();
            }
            yield return new WaitForSeconds(spawnInterval); 
        }
    }

    void SpawnRandomPowerUp()
    {
        if (powerUpObjects.Length == 0)
        {
            Debug.LogError("PowerUp Objects array is empty! Cannot spawn.");
            return;
        }

        List<GameObject> inactivePowerUps = new List<GameObject>();
        foreach(GameObject pu in powerUpObjects)
        {
            if (pu != null && !pu.activeSelf)
            {
                inactivePowerUps.Add(pu);
            }
        }

        if (inactivePowerUps.Count == 0)
        {
            Debug.LogWarning("Semua Power Up sedang aktif, tidak bisa spawn lagi.");
            return;
        }

        int randomIndex = Random.Range(0, inactivePowerUps.Count);
        GameObject selectedPowerUp = inactivePowerUps[randomIndex];

        float randomX = Random.Range(-xBound, xBound);
        float randomY = Random.Range(-yBound, yBound);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        selectedPowerUp.transform.position = spawnPosition;
        selectedPowerUp.SetActive(true);

        Debug.Log($"Spawned: {selectedPowerUp.name} at {spawnPosition}");
    }
}
