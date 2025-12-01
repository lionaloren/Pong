using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Definisikan ulang Enum PowerUpType di luar kelas (atau di file terpisah)
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
    public float spawnInterval = 15f; // Power Up muncul setiap 15 detik
    public float powerUpDuration = 6f; // Durasi efek
    
    // ⭐ BARU: Referensi langsung ke objek Power Up yang ada di scene (non-prefab)
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
        // ⭐ BARU: Sembunyikan semua Power Up saat game dimulai (sebelum StartGame)
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

    // Dipanggil oleh GameManager saat game dimulai
    public void StartSpawning()
    {
        HideAllPowerUps(); // Pastikan tersembunyi
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnPowerUpRoutine());
    }
    
    // Dipanggil oleh GameManager saat game berakhir atau di-reset
    public void StopSpawningAndClear()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        // Cukup sembunyikan karena tidak ada yang perlu dihancurkan (non-prefab)
        HideAllPowerUps();
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        yield return new WaitForSeconds(spawnInterval / 2f); 
        
        while (true)
        {
            // Cek kondisi game
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
        
        // 1. Pilih Power Up yang sedang tidak aktif (SetActive(false))
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

        // 2. Pilih salah satu Power Up yang tidak aktif secara acak
        int randomIndex = Random.Range(0, inactivePowerUps.Count);
        GameObject selectedPowerUp = inactivePowerUps[randomIndex];

        // 3. Tentukan posisi acak
        float randomX = Random.Range(-xBound, xBound);
        float randomY = Random.Range(-yBound, yBound);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0);

        // 4. Pindahkan dan Aktifkan
        selectedPowerUp.transform.position = spawnPosition;
        selectedPowerUp.SetActive(true);
        
        Debug.Log($"Spawned: {selectedPowerUp.name} at {spawnPosition}");
    }
}