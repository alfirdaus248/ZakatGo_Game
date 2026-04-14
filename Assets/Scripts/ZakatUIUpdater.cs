using UnityEngine;
using TMPro; // Pastikan ini diimpor jika menggunakan TextMeshPro

/// <summary>
/// Bertanggung jawab untuk memperbarui tampilan UI Zakat (misalnya 1/8).
/// </summary>
public class ZakatUIUpdater : MonoBehaviour
{
    [Tooltip("Komponen TextMeshProUGUI untuk menampilkan jumlah Zakat saat ini/kapasitas.")]
    public TextMeshProUGUI zakatText;

    private void Start()
    {
        // Pastikan InventoryManager sudah ada dan inisialisasi
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ZakatUIUpdater] InventoryManager.Instance belum tersedia saat Start.");
            return;
        }

        // Pastikan komponen Text sudah terhubung
        if (zakatText == null)
        {
            Debug.LogError("[ZakatUIUpdater] TextMeshProUGUI belum terhubung!");
            return;
        }

        // Langganan ke event saat inventori berubah
        InventoryManager.Instance.OnInventoryChanged += UpdateZakatDisplay;

        // Panggil pertama kali untuk menampilkan nilai awal
        // UpdateZakatDisplay(InventoryManager.Instance.CurrentZakat);
        UpdateZakatDisplay(InventoryManager.Instance.CurrentZakat, true);
    }

    private void OnDestroy()
    {
        // Berhenti berlangganan saat objek dihancurkan untuk mencegah error
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateZakatDisplay;
        }
    }

    /// <summary>
    /// Metode yang dipanggil ketika inventori Zakat berubah.
    /// </summary>
    /// <param name="newZakatAmount">Jumlah Zakat saat ini.</param>
    private void UpdateZakatDisplay(int newZakatAmount, bool isFromMuzaki)
    {
        int capacity = InventoryManager.Instance.capacity;

        // Format teks: "current/capacity"
        string display = $"{newZakatAmount}/{capacity}";
        
        // Atur warna berdasarkan status (misalnya, merah jika penuh)
        if (newZakatAmount >= capacity)
        {
            zakatText.text = $"<color=red>{display}</color>";
        }
        else
        {
            zakatText.text = display;
        }
    }
}