using System;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    public int startingZakat = 0;
    public int capacity = 99; 

    private int currentZakat;

    [Header("Level Progress")]
    [SerializeField] private int totalLevelZakat = 0;   
    [SerializeField] private int totalDistributed = 0;  

    // public event Action<int> OnInventoryChanged;
    public event Action<int, bool> OnInventoryChanged;
    public event Action<int, int> OnProgressChanged; 
    public event Action OnAllZakatDistributed;

    // 1. TAMBAHKAN EVENT BARU UNTUK UI COUNTER
    public event Action OnMuzakiCollected; // Terpanggil saat ambil zakat
    public event Action OnMustahikServed;  // Terpanggil saat beri zakat

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentZakat = Mathf.Clamp(startingZakat, 0, capacity);
    }

    public int CurrentZakat => currentZakat;
    public int TotalLevelZakat => totalLevelZakat;

    public void SetTotalLevelZakat(int total)
    {
        totalLevelZakat = total;
        capacity = Mathf.CeilToInt(total / 2f); 
        if (capacity < 1) capacity = 1;

        totalDistributed = 0; 
        
        OnProgressChanged?.Invoke(totalDistributed, totalLevelZakat);
        // OnInventoryChanged?.Invoke(currentZakat);
        OnInventoryChanged?.Invoke(currentZakat, true);
        
        Debug.Log($"[Inventory] Target Level: {totalLevelZakat}. Capacity: {capacity}");
    }

    public void NotifyZakatDistributed(int amount)
    {
        totalDistributed += amount;
        OnProgressChanged?.Invoke(totalDistributed, totalLevelZakat);

        // 2. PANGGIL EVENT SAAT MUSTAHIK SELESAI
        OnMustahikServed?.Invoke(); 

        if (totalLevelZakat > 0 && totalDistributed >= totalLevelZakat)
        {
            OnAllZakatDistributed?.Invoke();
            GameWin();
        }
    }

    void GameWin()
    {
        PlayerPrefs.SetInt("GameStatus", 1);
        Time.timeScale = 1f; 
        SceneManager.LoadScene("GameResult");
    }

    public bool HasSpaceFor(int amount)
    {
        return (currentZakat + amount) <= capacity;
    }

    public int AddZakat(int amount, bool isFromMuzaki = true)
    {
        if (amount <= 0) return 0;
        
        if (!HasSpaceFor(amount)) 
        {
            Debug.Log($"Inventory Penuh!");
            return 0; 
        }

        currentZakat += amount;
        // OnInventoryChanged?.Invoke(currentZakat);
        // Kirim info ke UI (ZakatUIUpdater)
        OnInventoryChanged?.Invoke(currentZakat, isFromMuzaki);

        // 3. PANGGIL EVENT SAAT MUZAKI SELESAI
        // Asumsi: Setiap kali berhasil AddZakat, berarti 1 Muzaki selesai.
        // OnMuzakiCollected?.Invoke(); 

        // --- LOGIKA DATA MUZAKI ---
        // Hanya panggil event ini jika zakat benar-benar dari Muzaki (bukan Maling)
        if (isFromMuzaki)
        {
            OnMuzakiCollected?.Invoke(); 
            Debug.Log("Zakat Muzaki bertambah (Masuk Data Statistik)");
        }
        else
        {
            Debug.Log("Zakat Maling bertambah (TIDAK Masuk Data Muzaki)");
        }

        return amount;
    }

    public bool RemoveZakat(int amount)
    {
        if (amount <= 0) return false;
        if (currentZakat < amount) return false;

        currentZakat -= amount;
        // OnInventoryChanged?.Invoke(currentZakat);

        // Saat berkurang, kita anggap true saja (atau tidak relevan) untuk UI
        OnInventoryChanged?.Invoke(currentZakat, true);
        return true;
    }

    public bool CanGive(int required)
    {
        return required > 0 && currentZakat >= required;
    }
}