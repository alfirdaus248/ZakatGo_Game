using UnityEngine;
using System.Collections.Generic; // 1. Wajib tambahkan ini untuk menggunakan List

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    [Header("Pengaturan Jumlah Spawn Random")]
    public int minMuzaki = 3;
    public int maxMuzaki = 6;

    public int minMustahik = 3;
    public int maxMustahik = 6;

    public int minMaling = 2;
    public int maxMaling = 2;

    public int SpawnedMuzakiCount { get; private set; }
    public int SpawnedMustahikCount { get; private set; }

    [Header("Setup Prefab")]
    public GameObject[] muzakiPrefabs = new GameObject[4];
    public GameObject[] mustahikPrefabs = new GameObject[4];
    public GameObject[] malingPrefabs = new GameObject[4];

    [Header("Setup Player")]
    public GameObject playerGameObject;

    [Header("Area Spawn")]
    public float spawnAreaSize = 10f;
    public float spawnHeight = 1.5f; 
    public float raycastHeight = 10f; 

    // --- TAMBAHAN BARU ---
    [Header("Jarak Antar Objek")]
    [Tooltip("Jarak minimal antar NPC/Player agar tidak bertumpuk")]
    public float minDistanceBetweenNPCs = 2.0f; 
    
    // List untuk menyimpan posisi yang sudah terpakai
    private List<Vector3> takenPositions = new List<Vector3>(); 
    // ---------------------

    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Bersihkan list posisi setiap kali game mulai
        takenPositions.Clear();

        SpawnPlayer();
        SpawnNPCs();
    }

    // --- FUNGSI INI DIMODIFIKASI ---
    private Vector3 GetValidSpawnPosition()
    {
        // Naikkan attempt agar komputer punya kesempatan mencari celah kosong
        int maxAttempts = 30; 

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnAreaSize, spawnAreaSize),
                raycastHeight, 
                Random.Range(-spawnAreaSize, spawnAreaSize)
            );

            RaycastHit hit;
            if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer | obstacleLayer))
            {
                if (hit.collider != null && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Vector3 candidatePos = new Vector3(randomPosition.x, hit.point.y + spawnHeight, randomPosition.z);

                    // --- LOGIKA CEK JARAK ---
                    if (IsPositionValid(candidatePos))
                    {
                        // Jika posisi aman (jauh dari yang lain), simpan ke list
                        takenPositions.Add(candidatePos);
                        return candidatePos; 
                    }
                    // Jika tidak aman, loop akan mengulang mencari posisi lain
                    // -----------------------
                }
            }
        }
        
        Debug.LogWarning("Gagal menemukan posisi spawn valid (Mungkin area terlalu penuh).");
        return new Vector3(0, spawnHeight, 0); 
    }

    // --- FUNGSI BARU: CEK JARAK ---
    private bool IsPositionValid(Vector3 candidate)
    {
        foreach (Vector3 pos in takenPositions)
        {
            // Jika jarak antara kandidat baru dengan posisi lama terlalu dekat
            if (Vector3.Distance(candidate, pos) < minDistanceBetweenNPCs)
            {
                return false; // Posisi tidak valid
            }
        }
        return true; // Posisi valid (aman)
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        if (playerGameObject != null) playerGameObject.transform.position = spawnPosition;
    }

    void SpawnNPCs()
    {
        SpawnedMuzakiCount = Random.Range(minMuzaki, maxMuzaki + 1);
        Debug.Log("Spawn Muzaki sebanyak: " + SpawnedMuzakiCount);

        for (int i = 0; i < SpawnedMuzakiCount; i++)
        {
            if (muzakiPrefabs.Length == 0) break;
            Vector3 spawnPosition = GetValidSpawnPosition();
            GameObject npc = muzakiPrefabs[Random.Range(0, muzakiPrefabs.Length)];
            Instantiate(npc, spawnPosition, Quaternion.identity);
        }

        SpawnedMustahikCount = Random.Range(minMustahik, maxMustahik + 1);
        Debug.Log("Spawn Mustahik sebanyak: " + SpawnedMustahikCount);

        for (int i = 0; i < SpawnedMustahikCount; i++)
        {
            if (mustahikPrefabs.Length == 0) break;
            Vector3 spawnPosition = GetValidSpawnPosition();
            GameObject npc = mustahikPrefabs[Random.Range(0, mustahikPrefabs.Length)];
            Instantiate(npc, spawnPosition, Quaternion.identity);
        }

        int malingCount = Random.Range(minMaling, maxMaling + 1);
        for (int i = 0; i < malingCount; i++)
        {
            if (malingPrefabs.Length == 0) break;
            Vector3 spawnPosition = GetValidSpawnPosition();
            GameObject npc = malingPrefabs[Random.Range(0, malingPrefabs.Length)];
            Instantiate(npc, spawnPosition, Quaternion.identity);
        }
    }
}