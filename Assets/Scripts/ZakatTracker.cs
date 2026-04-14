using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ZakatTracker : MonoBehaviour
{
    public static ZakatTracker Instance { get; private set; }

    private List<Muzaki> allMuzaki = new List<Muzaki>();
    private List<Mustahik> allMustahik = new List<Mustahik>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        // Inisialisasi daftar (bisa diisi manual atau otomatis mencari di scene)
        allMuzaki = FindObjectsOfType<Muzaki>().ToList();
        allMustahik = FindObjectsOfType<Mustahik>().ToList();

        // Dengar event fulfilled mustahik
        Mustahik.OnMustahikFulfilledGlobal += OnMustahikFulfilled;
    }

    public void NotifyMuzakiGiven(Muzaki mz, int amountGiven)
    {
        // Optionally track that this muzak has given (Muzaki.HasGiven())
        // Dipakai untuk perhitungan total zakat tersisa
    }

    private void OnMustahikFulfilled(Mustahik m)
    {
        // cek apakah semua mustahik terpenuhi -> trigger win
        if (allMustahik.All(x => x.IsFulfilled()))
        {
            Debug.Log("WIN: Semua mustahik terpenuhi!");
            // panggil GameManager.Win();
        }
    }

    /// <summary>
    /// Hitung total zakat di dunia: inventory player + muzakki yang belum memberi.
    /// (Catat: maling-held tidak termasuk di sini kecuali Anda track maling)
    /// </summary>
    public int ComputeTotalZakatInWorld()
    {
        int player = InventoryManager.Instance != null ? InventoryManager.Instance.CurrentZakat : 0;
        int muzakkiLeft = allMuzaki.Where(m => !m.HasGiven()).Sum(m => m.GetZakatAmount());
        // jika punya sistem maling, tambahkan count maling-held
        return player + muzakkiLeft;
    }

    /// <summary>
    /// Total kebutuhan mustahik (sum of requiredAmount)
    /// </summary>
    public int TotalMustahikNeeds()
    {
        return allMustahik.Sum(m => m.GetRequiredAmount());
    }

    private void Update()
    {
        // Opsional: jika totalZakat < totalNeed -> Auto lose
        if (ComputeTotalZakatInWorld() < TotalMustahikNeeds())
        {
            // Game over
            Debug.Log("Automatic GameOver: total zakat dunia kurang dari kebutuhan mustahik.");
            // GameManager.Lose();
        }
    }
}
