using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text; // <--- TAMBAHAN: Untuk memformat text log dengan rapi

public class EconomyBalancer : MonoBehaviour
{
    [Header("Fuzzy Logic Settings")]
    public AnimationCurve muzakiWealthCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve mustahikNeedCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Economy Constraints")]
    public int minZakat = 3; 
    public int averageZakat = 8;
    public float startDelay = 0.5f;

    private void Start()
    {
        StartCoroutine(BalanceRoutine());
    }

    IEnumerator BalanceRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        BalanceEconomyWithFuzzy();
    }

    void BalanceEconomyWithFuzzy()
    {
        Muzaki[] muzakis = FindObjectsOfType<Muzaki>();
        Mustahik[] mustahiks = FindObjectsOfType<Mustahik>();

        if (muzakis.Length == 0 || mustahiks.Length == 0) return;

        // 1. TENTUKAN TOTAL EKONOMI
        int totalEconomy = averageZakat * muzakis.Length;
        int minReq = mustahiks.Length * minZakat;
        if (totalEconomy < minReq) totalEconomy = minReq;
        if (totalEconomy > 30) totalEconomy = 20;

        // Hitung kapasitas Player nanti
        int playerCapacity = Mathf.CeilToInt(totalEconomy / 2f);

        // 2. HITUNG DISTRIBUSI MUZAKI (DENGAN LIMIT KAPASITAS)
        List<int> muzakiPortions = DistributeByFuzzyLogic(totalEconomy, muzakis.Length, minZakat, muzakiWealthCurve, playerCapacity);
        
        for (int i = 0; i < muzakis.Length; i++)
        {
            muzakis[i].zakatAmount = muzakiPortions[i];
            muzakis[i].randomizeOnStart = false; 
        }

        // 3. HITUNG DISTRIBUSI MUSTAHIK (TANPA LIMIT SPESIFIK)
        List<int> mustahikPortions = DistributeByFuzzyLogic(totalEconomy, mustahiks.Length, minZakat, mustahikNeedCurve, 999);

        for (int i = 0; i < mustahiks.Length; i++)
        {
            mustahiks[i].requiredAmount = mustahikPortions[i];
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetTotalLevelZakat(totalEconomy);
        }

        // --- TAMBAHAN: Tampilkan Data ke Console ---
        LogDistributionData(totalEconomy, playerCapacity, muzakis, mustahiks);
    }

    // --- METODE BARU: Untuk menampilkan log ---
    private void LogDistributionData(int total, int capacity, Muzaki[] muzakis, Mustahik[] mustahiks)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<color=cyan><b>=== LAPORAN DISTRIBUSI ZAKAT ===</b></color>");
        sb.AppendLine($"Total Ekonomi: <b>{total}</b> | Kapasitas Tas Player: <b>{capacity}</b>");
        sb.AppendLine("--------------------------------------------------");

        sb.AppendLine($"<b>DATA MUZAKI ({muzakis.Length} Orang):</b>");
        int sumMuzaki = 0;
        for (int i = 0; i < muzakis.Length; i++)
        {
            sb.AppendLine($" - {muzakis[i].name}: Membawa <color=yellow>{muzakis[i].zakatAmount}</color> Zakat");
            sumMuzaki += muzakis[i].zakatAmount;
        }
        sb.AppendLine($" >> Total dari Muzaki: {sumMuzaki}");

        sb.AppendLine("--------------------------------------------------");

        sb.AppendLine($"<b>DATA MUSTAHIK ({mustahiks.Length} Orang):</b>");
        int sumMustahik = 0;
        for (int i = 0; i < mustahiks.Length; i++)
        {
            sb.AppendLine($" - {mustahiks[i].name}: Butuh <color=green>{mustahiks[i].requiredAmount}</color> Zakat");
            sumMustahik += mustahiks[i].requiredAmount;
        }
        sb.AppendLine($" >> Total Kebutuhan: {sumMustahik}");
        sb.AppendLine("==================================================");

        Debug.Log(sb.ToString());
    }

    private List<int> DistributeByFuzzyLogic(int totalValue, int count, int minValue, AnimationCurve fuzzyCurve, int maxValueLimit)
    {
        List<float> rawWeights = new List<float>();
        float totalWeight = 0f;

        // A. FUZZIFICATION
        for (int i = 0; i < count; i++)
        {
            float w = fuzzyCurve.Evaluate(Random.value);
            if (w < 0.1f) w = 0.1f; 
            rawWeights.Add(w);
            totalWeight += w;
        }

        // B. NORMALISASI
        List<int> finalValues = new List<int>();
        int currentSum = 0;
        int reservedValue = count * minValue;
        int distributableValue = totalValue - reservedValue; 

        for (int i = 0; i < count; i++)
        {
            float ratio = rawWeights[i] / totalWeight;
            int share = Mathf.FloorToInt(distributableValue * ratio);
            int finalAmount = minValue + share;
            
            // CLAMPING: Pastikan tidak melebihi batas max (Capacity Player)
            if (finalAmount > maxValueLimit) finalAmount = maxValueLimit;
            
            finalValues.Add(finalAmount);
            currentSum += finalAmount;
        }

        // C. ERROR CORRECTION
        int remainder = totalValue - currentSum;
        int safety = 0;
        while (remainder > 0 && safety < 1000)
        {
            safety++;
            int idx = Random.Range(0, count);
            // Hanya tambah jika belum mentok limit
            if (finalValues[idx] < maxValueLimit)
            {
                finalValues[idx]++;
                remainder--;
            }
        }

        return finalValues;
    }
}