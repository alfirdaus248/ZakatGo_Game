using UnityEngine;
using UnityEngine.UI;

public class GameResultController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject winPanel;  // Masukkan Panel Menang di Inspector
    public GameObject losePanel; // Masukkan Panel Kalah di Inspector

    void Start()
    {
        // Ambil status dari PlayerPrefs (0 = Kalah, 1 = Menang)
        // Default 0 jika tidak ada data
        int gameStatus = PlayerPrefs.GetInt("GameStatus", 0);

        if (gameStatus == 1)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
        
        // Opsional: Hapus data setelah dipakai agar reset
        PlayerPrefs.DeleteKey("GameStatus");
    }

    void ShowWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
        Debug.Log("Menampilkan UI Menang");
    }

    void ShowLose()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(true);
        Debug.Log("Menampilkan UI Kalah");
    }
}