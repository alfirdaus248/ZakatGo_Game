using UnityEngine;

public class ZakatCheat : MonoBehaviour
{
    public GameObject arrowIndicator;   
    public float revealDistanceThreshold = 5f;  
    public float cooldownTime = 0.5f;  
    private float lastCheatTime = -5f;  

    public Transform playerTransform;  
    private GameObject nearestNPC;  
    private bool isArrowActive = false;  

    private PlayerLogic playerLogic;

    // --- MODIFIKASI: VARIABEL LIMIT CHEAT ---
    private int cheatUsageCount = 0;
    public int maxCheatUsage = 3; 
    // ----------------------------------------

    public enum CheatMode
    {
        CariMuzaki,
        CariMustahik,
        CariMaling
    }
    private CheatMode currentMode = CheatMode.CariMuzaki; 

    void Start()
    {
        if (arrowIndicator != null)
        {
            arrowIndicator.SetActive(false);
            isArrowActive = false;
        }

        playerLogic = GetComponent<PlayerLogic>();
        if (playerLogic == null && playerTransform != null)
        {
            playerLogic = playerTransform.GetComponent<PlayerLogic>();
        }
    }

    void Update()
    {
        // SPACE UNTUK MENU
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playerLogic != null)
            {
                // Tampilkan sisa kuota di menu bantuan
                string status = cheatUsageCount >= maxCheatUsage ? "(HABIS)" : $"({cheatUsageCount}/{maxCheatUsage})";
                playerLogic.ShowCheatText($"[CHEAT] {status} Pilih: 1.Muzaki | 2.Mustahik | 3.Maling", 3f);
            }
        }

        // INPUT ANGKA UNTUK EKSEKUSI
        if (Time.time > lastCheatTime + cooldownTime)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AttemptCheat(CheatMode.CariMuzaki);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AttemptCheat(CheatMode.CariMustahik);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AttemptCheat(CheatMode.CariMaling);
            }
        }

        // Logika menyembunyikan panah
        if (nearestNPC != null)
        {
            float distanceToNPC = Vector3.Distance(playerTransform.position, nearestNPC.transform.position);
            if (distanceToNPC < revealDistanceThreshold)
            {
                arrowIndicator.SetActive(false); 
                isArrowActive = false;
            }
        }
        else if (isArrowActive) 
        {
            arrowIndicator.SetActive(false);
            isArrowActive = false;
        }

        if (isArrowActive)
        {
            UpdateArrowPositionAndRotation();
        }
    }

    // --- FUNGSI BARU: CEK LIMIT SEBELUM MENCARI ---
    void AttemptCheat(CheatMode newMode)
    {
        // 1. Cek apakah kuota sudah habis
        if (cheatUsageCount >= maxCheatUsage)
        {
            if (playerLogic != null)
            {
                // Tampilkan pesan error jika limit habis
                playerLogic.ShowCheatText($"[CHEAT] GAGAL! Kuota Habis ({cheatUsageCount}/{maxCheatUsage})", 2f);
            }
            return; // Hentikan proses, jangan cari NPC
        }

        // 2. Jika aman, lanjut cari
        currentMode = newMode;
        RevealNearestNPC(); 
        lastCheatTime = Time.time; 
    }

    void RevealNearestNPC()
    {
        nearestNPC = FindTargetByMode();

        if (nearestNPC != null)
        {
            // --- MODIFIKASI: TAMBAH COUNTER HANYA JIKA KETEMU ---
            cheatUsageCount++; 
            // ---------------------------------------------------

            arrowIndicator.SetActive(true);
            isArrowActive = true;
            arrowIndicator.transform.position = playerTransform.position + Vector3.up * 2f;

            float dist = Vector3.Distance(playerTransform.position, nearestNPC.transform.position);
            
            // Format pesan: [CHEAT] (1/3) Target: Nama
            string msg = $"[CHEAT] ({cheatUsageCount}/{maxCheatUsage}) Target: {nearestNPC.name} ({dist:F1}m)";
            
            if (playerLogic != null)
            {
                playerLogic.ShowCheatText(msg, 3f); 
            }
        }
        else
        {
            arrowIndicator.SetActive(false); 
            isArrowActive = false;
            
            // Jika tidak ketemu, jangan kurangi kuota (biar adil)
            string msg = $"[CHEAT] Tidak ada target {currentMode} ({cheatUsageCount}/{maxCheatUsage})";
            if (playerLogic != null) playerLogic.ShowCheatText(msg, 1.5f);
        }
    }

    void UpdateArrowPositionAndRotation()
    {
        if (nearestNPC == null) return; 
        Vector3 direction = (nearestNPC.transform.position - playerTransform.position).normalized;
        if (direction != Vector3.zero) 
        {
            arrowIndicator.transform.rotation = Quaternion.LookRotation(direction);
            arrowIndicator.transform.rotation *= Quaternion.Euler(0, 90, 0); 
        }
        arrowIndicator.transform.position = playerTransform.position + Vector3.up * 2f;
    }

    GameObject FindTargetByMode()
    {
        switch (currentMode)
        {
            case CheatMode.CariMuzaki: return FindNearestMuzaki();
            case CheatMode.CariMustahik: return FindNearestMustahik();
            case CheatMode.CariMaling: return FindNearestMalingWithLoot();
            default: return null;
        }
    }

    GameObject FindNearestMuzaki()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestNPC = null;
        GameObject[] muzakis = GameObject.FindGameObjectsWithTag("Muzaki");
        foreach (GameObject go in muzakis)
        {
            if (go == null) continue;
            Muzaki script = go.GetComponent<Muzaki>();
            if (script != null && !script.HasGiven())
            {
                float d = Vector3.Distance(playerTransform.position, go.transform.position);
                if (d < closestDistance) { closestDistance = d; closestNPC = go; }
            }
        }
        return closestNPC;
    }

    GameObject FindNearestMustahik()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestNPC = null;
        GameObject[] mustahiks = GameObject.FindGameObjectsWithTag("Mustahik");
        foreach (GameObject go in mustahiks)
        {
            if (go == null) continue;
            Mustahik script = go.GetComponent<Mustahik>();
            if (script != null && !script.IsFulfilled())
            {
                float d = Vector3.Distance(playerTransform.position, go.transform.position);
                if (d < closestDistance) { closestDistance = d; closestNPC = go; }
            }
        }
        return closestNPC;
    }

    GameObject FindNearestMalingWithLoot()
    {
        float closestDistance = Mathf.Infinity;
        GameObject closestNPC = null;
        GameObject[] malings = GameObject.FindGameObjectsWithTag("Maling"); 
        foreach (GameObject go in malings)
        {
            if (go == null) continue;
            Maling script = go.GetComponent<Maling>();
            if (script != null && script.GetStolenAmount() > 0)
            {
                float d = Vector3.Distance(playerTransform.position, go.transform.position);
                if (d < closestDistance) { closestDistance = d; closestNPC = go; }
            }
        }
        return closestNPC;
    }

    void OnDrawGizmos()
    {
        if (nearestNPC != null)
        {
            switch (currentMode)
            {
                case CheatMode.CariMuzaki: Gizmos.color = Color.green; break;
                case CheatMode.CariMustahik: Gizmos.color = Color.cyan; break;
                case CheatMode.CariMaling: Gizmos.color = Color.red; break;
            }
            Gizmos.DrawSphere(nearestNPC.transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, nearestNPC.transform.position);
        }
    }
}