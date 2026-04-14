using UnityEngine;
using TMPro; // Wajib menggunakan TextMeshPro
using System.Collections;

public class NPCProgressUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag Text TMP untuk jumlah Muzaki di sini")]
    public TextMeshProUGUI muzakiText;

    [Tooltip("Drag Text TMP untuk jumlah Mustahik di sini")]
    public TextMeshProUGUI mustahikText;

    // Variabel internal untuk menyimpan sisa jumlah
    private int remainingMuzaki;
    private int muzakitotal;
    private int remainingMustahik;
    private int mustahikTotal;

    private void Start()
    {
        // Kita gunakan Coroutine untuk menunggu Spawner selesai inisialisasi
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        // Tunggu sampai frame berikutnya agar Spawner.Start() selesai dijalankan
        yield return null; 

        if (Spawner.Instance != null)
        {
            // Ambil jumlah awal dari Spawner
            // remainingMuzaki = Spawner.Instance.SpawnedMuzakiCount;
            remainingMuzaki = 0;
            muzakitotal = Spawner.Instance.SpawnedMuzakiCount;
            // remainingMustahik = Spawner.Instance.SpawnedMustahikCount;
            remainingMustahik = 0;
            mustahikTotal = Spawner.Instance.SpawnedMustahikCount;
            
            UpdateUIText();
        }
        else
        {
            Debug.LogError("Spawner Instance tidak ditemukan! Pastikan Spawner ada di scene.");
        }
    }

    private void OnEnable()
    {
        // Subscribe ke event InventoryManager
        // Kita butuh delay sedikit karena InventoryManager Singleton mungkin belum awake
        StartCoroutine(SubscribeToEvents());
    }

    IEnumerator SubscribeToEvents()
    {
        yield return null; // Tunggu instance siap
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnMuzakiCollected += DecreaseMuzaki;
            InventoryManager.Instance.OnMustahikServed += DecreaseMustahik;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe untuk mencegah error memory leak
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnMuzakiCollected -= DecreaseMuzaki;
            InventoryManager.Instance.OnMustahikServed -= DecreaseMustahik;
        }
    }

    // Dipanggil saat Event OnMuzakiCollected terjadi
    void DecreaseMuzaki()
    {
        // remainingMuzaki--;
        remainingMuzaki++;
        // if (remainingMuzaki < 0) remainingMuzaki = 0;
        UpdateUIText();
    }

    // Dipanggil saat Event OnMustahikServed terjadi
    void DecreaseMustahik()
    {
        // remainingMustahik--;
        remainingMustahik++;
        // if (remainingMustahik < 0) remainingMustahik = 0;
        UpdateUIText();
    }

    void UpdateUIText()
    {
        if (muzakiText != null)
            // muzakiText.text = $"Muzaki Tersisa: {remainingMuzaki}";
            muzakiText.text = $"{remainingMuzaki}/{muzakitotal}";
        
        if (mustahikText != null)
            // mustahikText.text = $"Mustahik Tersisa: {remainingMustahik}";
            mustahikText.text = $"{remainingMustahik}/{mustahikTotal}";
    }
}